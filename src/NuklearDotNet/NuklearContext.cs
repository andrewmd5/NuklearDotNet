using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NuklearDotNet;

/// <summary>
/// Instance-based Nuklear GUI context. Owns all native resources and implements
/// <see cref="IDisposable"/> for deterministic cleanup.
/// </summary>
public sealed unsafe partial class NuklearContext : IDisposable {
	nk_context* _ctx;
	NuklearDevice? _dev;

	nk_allocator* _allocator;
	nk_font_atlas* _fontAtlas;
	nk_draw_null_texture* _nullTexture;
	nk_convert_config* _convertCfg;

	nk_buffer* _commands, _vertices, _indices;

	nk_draw_vertex_layout_element* _vertexLayout;

	IFrameBuffered? _frameBuffered;
	INuklearDeviceRenderHooks? _renderHooks;

	bool _forceUpdateQueued;
	bool _disposed;

	static readonly nuint VertexPositionOffset = 0;
	static readonly nuint VertexUVOffset = (nuint)sizeof(NkVector2f);
	static readonly nuint VertexColorOffset = (nuint)(sizeof(NkVector2f) * 2);

	NuklearDevice GetDeviceOrThrow() =>
		_dev ?? throw new InvalidOperationException("NuklearContext has been disposed");

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	static void* NkAlloc(NkHandle handle, void* old, nuint size) {
		return NativeMemory.AllocZeroed(size);
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	static void NkFree(NkHandle handle, void* old) {
		if (old != null)
			NativeMemory.Free(old);
	}

	static void* ManagedAlloc(nuint size) {
		void* mem = NativeMemory.AllocZeroed(size);
		if (mem == null)
			throw new OutOfMemoryException("Cannot allocate native memory");
		return mem;
	}

	/// <summary>
	/// Raw pointer to the underlying <c>nk_context</c>. Intended for interop with
	/// subsystems that need the native context directly (e.g., <see cref="NkConsoleContext"/>).
	/// </summary>
	public nk_context* NativeContext => _ctx;

	public float DefaultFontHeight { get; set; }

	void FontStash(FontStashAction? A = null) {
		Nuklear.nk_font_atlas_init_default(_fontAtlas);
		Nuklear.nk_font_atlas_begin(_fontAtlas);

		if (DefaultFontHeight > 0)
			Nuklear.nk_font_atlas_add_default_ex(_fontAtlas, DefaultFontHeight);

		A?.Invoke(_fontAtlas);

		int W, H;
		IntPtr Image = Nuklear.nk_font_atlas_bake(_fontAtlas, &W, &H, nk_font_atlas_format.NK_FONT_ATLAS_RGBA32);

		int TexHandle = GetDeviceOrThrow().CreateTextureHandle(W, H, Image);
		Nuklear.nk_font_atlas_end(_fontAtlas, Nuklear.nk_handle_id(TexHandle), _nullTexture);

		var dev = GetDeviceOrThrow();
		if (dev.CustomFont is not null)
			Nuklear.nk_style_set_font(_ctx, &dev.CustomFont->handle);
		else if (_fontAtlas->default_font != null)
			Nuklear.nk_style_set_font(_ctx, &_fontAtlas->default_font->handle);
	}

	/// <summary>
	/// Validates that C# struct sizes match their native counterparts.
	/// Call during development to catch layout mismatches early.
	/// </summary>
	public static void ValidateStructSizes() {
		void Check(string name, int managed, int native) {
			if (managed != native)
				throw new InvalidOperationException(
					$"Struct size mismatch: {name} is {managed} bytes in C# but {native} bytes in native");
		}

		Check("nk_context", sizeof(nk_context), Nuklear.nk_debug_sizeof_context());
		Check("nk_buffer", sizeof(nk_buffer), Nuklear.nk_debug_sizeof_buffer());
		Check("nk_convert_config", sizeof(nk_convert_config), Nuklear.nk_debug_sizeof_convert_config());
		Check("nk_draw_null_texture", sizeof(nk_draw_null_texture), Nuklear.nk_debug_sizeof_draw_null_texture());
		Check("NkHandle", sizeof(NkHandle), Nuklear.nk_debug_sizeof_handle());
		Check("nk_allocator", sizeof(nk_allocator), Nuklear.nk_debug_sizeof_allocator());
		Check("nk_draw_list", sizeof(nk_draw_list), Nuklear.nk_debug_sizeof_draw_list());
		Check("nk_style", sizeof(nk_style), Nuklear.nk_debug_sizeof_style());
		Check("nk_input", sizeof(nk_input), Nuklear.nk_debug_sizeof_input());
		Check("nk_font_atlas", sizeof(nk_font_atlas), Nuklear.nk_debug_sizeof_font_atlas());
		Check("nk_draw_command", sizeof(nk_draw_command), Nuklear.nk_debug_sizeof_draw_command());
		Check("nk_draw_vertex_layout_element", sizeof(nk_draw_vertex_layout_element), Nuklear.nk_debug_sizeof_draw_vertex_layout_element());
		Check("nk_cursor", sizeof(nk_cursor), Nuklear.nk_debug_sizeof_cursor());
		Check("nk_image", sizeof(nk_image), Nuklear.nk_debug_sizeof_image());
		Check("nk_font", sizeof(nk_font), Nuklear.nk_debug_sizeof_font());
		Check("nk_user_font", sizeof(nk_user_font), Nuklear.nk_debug_sizeof_user_font());
		Check("nk_baked_font", sizeof(nk_baked_font), Nuklear.nk_debug_sizeof_baked_font());
		Check("nk_font_glyph", sizeof(nk_font_glyph), Nuklear.nk_debug_sizeof_font_glyph());
		Check("nk_font_config", sizeof(nk_font_config), Nuklear.nk_debug_sizeof_font_config());
	}

	public void HandleInput() {
		Nuklear.nk_input_begin(_ctx);

		var dev = GetDeviceOrThrow();
		while (dev.HasPendingEvents) {
			NuklearEvent E = dev.DequeueEvent();

			switch (E.EvtType) {
				case NuklearEvent.EventType.MouseButton:
					Nuklear.nk_input_button(_ctx, (nk_buttons)E.MButton, E.X, E.Y, E.Down ? 1 : 0);
					break;

				case NuklearEvent.EventType.MouseMove:
					Nuklear.nk_input_motion(_ctx, E.X, E.Y);
					break;

				case NuklearEvent.EventType.Scroll:
					Nuklear.nk_input_scroll(_ctx, new nk_vec2() { x = E.ScrollX, y = E.ScrollY });
					break;

				case NuklearEvent.EventType.Text:
					for (int i = 0; i < E.Text.Length; i++) {
						if (!char.IsControl(E.Text[i]))
							Nuklear.nk_input_unicode(_ctx, E.Text[i]);
					}
					break;

				case NuklearEvent.EventType.KeyboardKey:
					Nuklear.nk_input_key(_ctx, E.Key, E.Down ? 1 : 0);
					break;

				case NuklearEvent.EventType.ForceUpdate:
					break;

				default:
					throw new NotImplementedException();
			}
		}

		Nuklear.nk_input_end(_ctx);
	}

	void RenderDrawCommand(nk_draw_command* Cmd, ref uint Offset) {
		if (Cmd->elem_count == 0)
			return;

		GetDeviceOrThrow().Render(Cmd->userdata, Cmd->texture.id, Cmd->clip_rect, Offset, Cmd->elem_count);
		Offset += Cmd->elem_count;
	}

	public void Render() {
		NkConvertResult R = (NkConvertResult)Nuklear.nk_convert(_ctx, _commands, _vertices, _indices, _convertCfg);
		if (R != NkConvertResult.Success)
			throw new InvalidOperationException($"nk_convert failed: {R}");

		int vertCount = (int)(_vertices->allocated / (nuint)sizeof(NkVertex));
		int indexCount = (int)(_indices->allocated / (nuint)sizeof(ushort));
		NkVertex* VertsPtr = (NkVertex*)_vertices->memory.ptr;
		ushort* IndicesPtr = (ushort*)_indices->memory.ptr;

		var vertsSpan = new ReadOnlySpan<NkVertex>(VertsPtr, vertCount);
		var indicesSpan = new ReadOnlySpan<ushort>(IndicesPtr, indexCount);

		GetDeviceOrThrow().SetBuffer(vertsSpan, indicesSpan);
		_frameBuffered?.BeginBuffering();

		uint Offset = 0;
		_renderHooks?.BeginRender();

		nk_draw_command* Cmd = Nuklear.nk__draw_begin(_ctx, _commands);
		while (Cmd != null) {
			RenderDrawCommand(Cmd, ref Offset);
			Cmd = Nuklear.nk__draw_next(Cmd, _commands, _ctx);
		}

		_renderHooks?.EndRender();
		_frameBuffered?.EndBuffering();

		Nuklear.nk_buffer_clear(_commands);
		Nuklear.nk_buffer_clear(_vertices);
		Nuklear.nk_buffer_clear(_indices);
		Nuklear.nk_clear(_ctx);

		_frameBuffered?.RenderFinal();
	}

	/// <exception cref="OutOfMemoryException">Native allocation failed.</exception>
	/// <exception cref="InvalidOperationException">Device initialization or font atlas baking failed.</exception>
	public NuklearContext(NuklearDevice device, float fontSize = 0) {
		_dev = device;
		_frameBuffered = device as IFrameBuffered;
		_renderHooks = device as INuklearDeviceRenderHooks;

		DefaultFontHeight = fontSize;

		int nativeContextSize = Nuklear.nk_debug_sizeof_context();
		int nativeFontAtlasSize = Nuklear.nk_debug_sizeof_font_atlas();

		_ctx = (nk_context*)ManagedAlloc((nuint)nativeContextSize);
		_allocator = (nk_allocator*)ManagedAlloc((nuint)sizeof(nk_allocator));
		_fontAtlas = (nk_font_atlas*)ManagedAlloc((nuint)nativeFontAtlasSize);
		_nullTexture = (nk_draw_null_texture*)ManagedAlloc((nuint)sizeof(nk_draw_null_texture));
		_convertCfg = (nk_convert_config*)ManagedAlloc((nuint)sizeof(nk_convert_config));
		_commands = (nk_buffer*)ManagedAlloc((nuint)sizeof(nk_buffer));
		_vertices = (nk_buffer*)ManagedAlloc((nuint)sizeof(nk_buffer));
		_indices = (nk_buffer*)ManagedAlloc((nuint)sizeof(nk_buffer));

		_vertexLayout = (nk_draw_vertex_layout_element*)ManagedAlloc((nuint)(sizeof(nk_draw_vertex_layout_element) * 4));
		_vertexLayout[0] = new nk_draw_vertex_layout_element(nk_draw_vertex_layout_attribute.NK_VERTEX_POSITION, nk_draw_vertex_layout_format.NK_FORMAT_FLOAT,
			VertexPositionOffset);
		_vertexLayout[1] = new nk_draw_vertex_layout_element(nk_draw_vertex_layout_attribute.NK_VERTEX_TEXCOORD, nk_draw_vertex_layout_format.NK_FORMAT_FLOAT,
			VertexUVOffset);
		_vertexLayout[2] = new nk_draw_vertex_layout_element(nk_draw_vertex_layout_attribute.NK_VERTEX_COLOR, nk_draw_vertex_layout_format.NK_FORMAT_R8G8B8A8,
			VertexColorOffset);
		_vertexLayout[3] = nk_draw_vertex_layout_element.NK_VERTEX_LAYOUT_END;

		_allocator->alloc = &NkAlloc;
		_allocator->free = &NkFree;

		Nuklear.nk_init_default(_ctx, null);

		(device as INuklearDeviceInit)?.Init();

		if (device is INuklearDeviceFontStash fontStasher)
			FontStash(fontStasher.FontStash);
		else
			FontStash();

		_convertCfg->shape_AA = nk_anti_aliasing.NK_ANTI_ALIASING_ON;
		_convertCfg->line_AA = nk_anti_aliasing.NK_ANTI_ALIASING_ON;
		_convertCfg->vertex_layout = _vertexLayout;
		_convertCfg->vertex_size = (nuint)sizeof(NkVertex);
		_convertCfg->vertex_alignment = 1;
		_convertCfg->circle_segment_count = 64;
		_convertCfg->curve_segment_count = 64;
		_convertCfg->arc_segment_count = 64;
		_convertCfg->global_alpha = 1.0f;
		_convertCfg->null_tex = *_nullTexture;

		Nuklear.nk_buffer_init_default(_commands);
		Nuklear.nk_buffer_init_default(_vertices);
		Nuklear.nk_buffer_init_default(_indices);
	}

	public void Frame(Action A) {
		if (_disposed)
			throw new ObjectDisposedException(nameof(NuklearContext));

		if (_forceUpdateQueued) {
			_forceUpdateQueued = false;
			GetDeviceOrThrow().ForceUpdate();
		}

		HandleInput();
		A();
		Render();
	}

	public void SetDeltaTime(float Delta) {
		if (_ctx != null)
			_ctx->delta_time_Seconds = Delta;
	}

	public bool Window(string Name, string Title, NkRect Bounds, NkPanelFlags Flags, Action A) {
		bool Res = true;

		int nameLen = NkStringHelper.GetUtf8ByteCount(Name);
		int titleLen = NkStringHelper.GetUtf8ByteCount(Title);
		Span<byte> nameUtf8 = nameLen <= 512 ? stackalloc byte[nameLen] : new byte[nameLen];
		Span<byte> titleUtf8 = titleLen <= 512 ? stackalloc byte[titleLen] : new byte[titleLen];
		NkStringHelper.GetUtf8(Name, nameUtf8);
		NkStringHelper.GetUtf8(Title, titleUtf8);

		fixed (byte* pName = nameUtf8)
		fixed (byte* pTitle = titleUtf8) {
			if (Nuklear.nk_begin_titled(_ctx, pName, pTitle, Bounds, (uint)Flags) != 0)
				A?.Invoke();
			else
				Res = false;
		}

		Nuklear.nk_end(_ctx);
		return Res;
	}

	public bool Window(string Title, NkRect Bounds, NkPanelFlags Flags, Action A) => Window(Title, Title, Bounds, Flags, A);

	public bool WindowIsClosed(string Name) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Name)];
		NkStringHelper.GetUtf8(Name, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_window_is_closed(_ctx, p) != 0;
	}

	public bool WindowIsHidden(string Name) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Name)];
		NkStringHelper.GetUtf8(Name, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_window_is_hidden(_ctx, p) != 0;
	}

	public bool WindowIsCollapsed(string Name) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Name)];
		NkStringHelper.GetUtf8(Name, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_window_is_collapsed(_ctx, p) != 0;
	}

	public bool Group(string Name, string Title, NkPanelFlags Flags, Action A) {
		int nameLen = NkStringHelper.GetUtf8ByteCount(Name);
		int titleLen = NkStringHelper.GetUtf8ByteCount(Title);
		Span<byte> nameUtf8 = nameLen <= 512 ? stackalloc byte[nameLen] : new byte[nameLen];
		Span<byte> titleUtf8 = titleLen <= 512 ? stackalloc byte[titleLen] : new byte[titleLen];
		NkStringHelper.GetUtf8(Name, nameUtf8);
		NkStringHelper.GetUtf8(Title, titleUtf8);

		fixed (byte* pName = nameUtf8)
		fixed (byte* pTitle = titleUtf8) {
			if (Nuklear.nk_group_begin_titled(_ctx, pName, pTitle, (uint)Flags) != 0) {
				A?.Invoke();
				Nuklear.nk_group_end(_ctx);
				return true;
			}
		}
		return false;
	}

	public bool Group(string Name, NkPanelFlags Flags, Action A) => Group(Name, Name, Flags, A);

	public bool ButtonLabel(string Label) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Label)];
		NkStringHelper.GetUtf8(Label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_button_label(_ctx, p) != 0;
	}

	public bool ButtonText(string Text) {
		int byteCount = NkStringHelper.GetUtf8ByteCount(Text);
		Span<byte> utf8 = stackalloc byte[byteCount];
		int written = NkStringHelper.GetUtf8(Text, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_button_text(_ctx, p, written) != 0;
	}

	public bool ButtonText(char Char) => ButtonText(Char.ToString());

	public void LayoutRowStatic(float Height, int ItemWidth, int Cols) {
		Nuklear.nk_layout_row_static(_ctx, Height, ItemWidth, Cols);
	}

	public void LayoutRowDynamic(float Height = 0, int Cols = 1) {
		Nuklear.nk_layout_row_dynamic(_ctx, Height, Cols);
	}

	public void Label(string Txt, NkTextAlign TextAlign = (NkTextAlign)NkTextAlignment.NK_TEXT_LEFT) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Txt)];
		NkStringHelper.GetUtf8(Txt, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_label(_ctx, p, (uint)TextAlign);
	}

	public void LabelWrap(string Txt) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Txt)];
		NkStringHelper.GetUtf8(Txt, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_label_wrap(_ctx, p);
	}

	public void LabelColored(string Txt, NkColor Clr, NkTextAlign TextAlign = (NkTextAlign)NkTextAlignment.NK_TEXT_LEFT) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Txt)];
		NkStringHelper.GetUtf8(Txt, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_label_colored(_ctx, p, (uint)TextAlign, Clr);
	}

	public void LabelColored(string Txt, NkColor Clr) {
		LabelColored(Txt, Clr, (NkTextAlign)NkTextAlignment.NK_TEXT_LEFT);
	}

	public void LabelColoredWrap(string Txt, NkColor Clr) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Txt)];
		NkStringHelper.GetUtf8(Txt, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_label_colored_wrap(_ctx, p, Clr);
	}

	public NkRect WindowGetBounds() {
		return Nuklear.nk_window_get_bounds(_ctx);
	}

	public NkEditEvents EditString(NkEditTypes EditType, byte[] Buffer, ref int length, int maxLength,
		delegate* unmanaged[Cdecl]<nk_text_edit*, uint, int> filter) {
		fixed (byte* pBuf = Buffer)
		fixed (int* pLen = &length) {
			return (NkEditEvents)Nuklear.nk_edit_string(_ctx, (uint)EditType, pBuf, pLen, maxLength, filter);
		}
	}

	public NkEditEvents EditString(NkEditTypes EditType, StringBuilder Buffer) {
		int maxLen = Math.Max(Buffer.Capacity, 256);
		Span<byte> buf = maxLen <= 1024 ? stackalloc byte[maxLen] : new byte[maxLen];
		string current = Buffer.ToString();
		int len = Encoding.UTF8.GetBytes(current, buf);

		NkEditEvents result;
		fixed (byte* pBuf = buf) {
			int lenCopy = len;
			result = (NkEditEvents)Nuklear.nk_edit_string(_ctx, (uint)EditType, pBuf, &lenCopy, maxLen, null);
			len = lenCopy;
		}

		string newStr = Encoding.UTF8.GetString(buf[..len]);
		Buffer.Clear();
		Buffer.Append(newStr);
		return result;
	}

	public delegate int EditFilterDelegate(ref nk_text_edit TextBox, uint Rune);
	static EditFilterDelegate? s_currentEditFilter;

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	static int EditFilterTrampoline(nk_text_edit* te, uint unicode) {
		if (s_currentEditFilter is not null)
			return s_currentEditFilter(ref *te, unicode);
		return 1;
	}

	public NkEditEvents EditString(NkEditTypes EditType, StringBuilder Buffer, EditFilterDelegate Filter) {
		int maxLen = Math.Max(Buffer.Capacity, 256);
		Span<byte> buf = maxLen <= 1024 ? stackalloc byte[maxLen] : new byte[maxLen];
		string current = Buffer.ToString();
		int len = Encoding.UTF8.GetBytes(current, buf);

		s_currentEditFilter = Filter;
		NkEditEvents result;
		try {
			fixed (byte* pBuf = buf) {
				int lenCopy = len;
				result = (NkEditEvents)Nuklear.nk_edit_string(_ctx, (uint)EditType, pBuf, &lenCopy, maxLen,
					&EditFilterTrampoline);
				len = lenCopy;
			}
		} finally {
			s_currentEditFilter = null;
		}

		string newStr = Encoding.UTF8.GetString(buf[..len]);
		Buffer.Clear();
		Buffer.Append(newStr);
		return result;
	}

	public bool IsKeyPressed(NkKeys Key) {
		return Nuklear.nk_input_is_key_pressed(&_ctx->input, Key) != 0;
	}

	public void QueueForceUpdate() {
		_forceUpdateQueued = true;
	}

	public void WindowClose(string Name) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Name)];
		NkStringHelper.GetUtf8(Name, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_window_close(_ctx, p);
	}

	/// <summary>
	/// Displays a float slider and returns the updated value.
	/// </summary>
	public float SlideFloat(float min, float value, float max, float step) {
		return Nuklear.nk_slide_float(_ctx, min, value, max, step);
	}

	/// <summary>
	/// Displays an int slider and returns the updated value.
	/// </summary>
	public int SlideInt(int min, int value, int max, int step) {
		return Nuklear.nk_slide_int(_ctx, min, value, max, step);
	}

	/// <summary>
	/// Displays a progress bar and returns the updated value.
	/// </summary>
	public nuint Progress(nuint current, nuint max, nk_modify modifiable = nk_modify.NK_MODIFIABLE) {
		return Nuklear.nk_prog(_ctx, current, max, (int)modifiable);
	}

	/// <summary>
	/// Displays a checkbox and returns the updated active state.
	/// </summary>
	public bool CheckLabel(string label, bool active) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_check_label(_ctx, p, active ? 1 : 0) != 0;
	}

	/// <summary>
	/// Displays a checkbox that mutates the active state by reference.
	/// Returns true if the checkbox was toggled this frame.
	/// </summary>
	public bool CheckboxLabel(string label, ref bool active) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		int val = active ? 1 : 0;
		bool toggled;
		fixed (byte* p = utf8)
			toggled = Nuklear.nk_checkbox_label(_ctx, p, &val) != 0;
		active = val != 0;
		return toggled;
	}

	/// <summary>
	/// Displays a radio button and returns whether it is selected.
	/// </summary>
	public bool OptionLabel(string label, bool active) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_option_label(_ctx, p, active ? 1 : 0) != 0;
	}

	/// <summary>
	/// Displays a color picker widget and returns the updated color.
	/// </summary>
	public nk_colorf ColorPicker(nk_colorf color, nk_color_format format = nk_color_format.NK_RGBA) {
		return Nuklear.nk_color_picker(_ctx, color, format);
	}

	/// <summary>
	/// Displays an editable float property field and returns the updated value.
	/// </summary>
	public float PropertyFloat(string name, float value, in NkPropertyRange<float> range) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_propertyf(_ctx, p, range.Min, value, range.Max, range.Step, range.IncPerPixel);
	}

	/// <summary>
	/// Displays an editable int property field and returns the updated value.
	/// </summary>
	public int PropertyInt(string name, int value, in NkPropertyRange<int> range) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_propertyi(_ctx, p, range.Min, value, range.Max, range.Step, range.IncPerPixel);
	}

	/// <summary>
	/// Displays an editable double property field and returns the updated value.
	/// </summary>
	public double PropertyDouble(string name, double value, in NkPropertyRange<double> range) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_propertyd(_ctx, p, range.Min, value, range.Max, range.Step, range.IncPerPixel);
	}

	/// <summary>
	/// Displays a float knob widget. Returns true if the value was modified by the user.
	/// </summary>
	public bool KnobFloat(float min, ref float value, float max, float step) {
		fixed (float* pVal = &value)
			return Nuklear.nk_knob_float(_ctx, min, pVal, max, step, nk_heading.NK_UP, 0) != 0;
	}

	/// <summary>
	/// Displays an int knob widget. Returns true if the value was modified by the user.
	/// </summary>
	public bool KnobInt(int min, ref int value, int max, int step) {
		fixed (int* pVal = &value)
			return Nuklear.nk_knob_int(_ctx, min, pVal, max, step, nk_heading.NK_UP, 0) != 0;
	}

	/// <summary>
	/// Displays a colored button and returns whether it was clicked.
	/// </summary>
	public bool ButtonColor(NkColor color) {
		return Nuklear.nk_button_color(_ctx, color) != 0;
	}

	/// <summary>
	/// Displays a symbol button and returns whether it was clicked.
	/// </summary>
	public bool ButtonSymbol(nk_symbol_type symbol) {
		return Nuklear.nk_button_symbol(_ctx, symbol) != 0;
	}

	/// <summary>
	/// Displays a selectable label and returns the updated selection state.
	/// </summary>
	public bool SelectLabel(string label, NkTextAlign align, bool selected) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_select_label(_ctx, p, (uint)align, selected ? 1 : 0) != 0;
	}

	/// <summary>
	/// Displays an empty spacer widget.
	/// </summary>
	public void Spacer() {
		Nuklear.nk_spacer(_ctx);
	}

	/// <summary>
	/// Displays a horizontal rule (separator line).
	/// </summary>
	public void RuleHorizontal(NkColor color, int rounding = 0) {
		Nuklear.nk_rule_horizontal(_ctx, color, rounding);
	}

	/// <summary>
	/// Begins a line chart, invokes the callback to push values, then ends the chart.
	/// </summary>
	public bool ChartLines(int count, float min, float max, Action<int> pushValues) {
		return ChartImpl(nk_chart_type.NK_CHART_LINES, count, min, max, pushValues);
	}

	/// <summary>
	/// Begins a column chart, invokes the callback to push values, then ends the chart.
	/// </summary>
	public bool ChartColumns(int count, float min, float max, Action<int> pushValues) {
		return ChartImpl(nk_chart_type.NK_CHART_COLUMN, count, min, max, pushValues);
	}

	bool ChartImpl(nk_chart_type type, int count, float min, float max, Action<int> pushValues) {
		if (Nuklear.nk_chart_begin(_ctx, type, count, min, max) != 0) {
			pushValues(count);
			Nuklear.nk_chart_end(_ctx);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Pushes a value to the current chart being built.
	/// </summary>
	public void ChartPush(float value) {
		Nuklear.nk_chart_push(_ctx, value);
	}

	/// <summary>
	/// Displays a simple plot chart from a span of float values.
	/// </summary>
	public void Plot(nk_chart_type type, ReadOnlySpan<float> values) {
		fixed (float* p = values)
			Nuklear.nk_plot(_ctx, type, p, values.Length, 0);
	}

	/// <summary>
	/// Displays a simple text tooltip at the current widget position.
	/// </summary>
	public void Tooltip(string text) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(text)];
		NkStringHelper.GetUtf8(text, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_tooltip(_ctx, p);
	}

	/// <summary>
	/// Begins a custom tooltip with the given width, invokes the content callback, then ends it.
	/// </summary>
	public void Tooltip(float width, Action content) {
		if (Nuklear.nk_tooltip_begin(_ctx, width) != 0) {
			content();
			Nuklear.nk_tooltip_end(_ctx);
		}
	}

	/// <summary>
	/// Displays a static popup window, invoking the content callback if open.
	/// </summary>
	public bool PopupStatic(string title, NkPanelFlags flags, NkRect bounds, Action content) {
		return PopupImpl(nk_popup_type.NK_POPUP_STATIC, title, flags, bounds, content);
	}

	/// <summary>
	/// Displays a dynamic popup window, invoking the content callback if open.
	/// </summary>
	public bool PopupDynamic(string title, NkPanelFlags flags, NkRect bounds, Action content) {
		return PopupImpl(nk_popup_type.NK_POPUP_DYNAMIC, title, flags, bounds, content);
	}

	bool PopupImpl(nk_popup_type type, string title, NkPanelFlags flags, NkRect bounds, Action content) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(title)];
		NkStringHelper.GetUtf8(title, utf8);
		fixed (byte* p = utf8) {
			if (Nuklear.nk_popup_begin(_ctx, type, p, (uint)flags, bounds) != 0) {
				content();
				Nuklear.nk_popup_end(_ctx);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Closes the currently open popup.
	/// </summary>
	public void PopupClose() {
		Nuklear.nk_popup_close(_ctx);
	}

	/// <summary>
	/// Displays a combo box from a string array and returns the updated selected index.
	/// </summary>
	public int Combo(string[] items, int selected, int itemHeight, nk_vec2 size) {
		int count = items.Length;
		int totalBytes = 0;
		for (int i = 0; i < count; i++)
			totalBytes += NkStringHelper.GetUtf8ByteCount(items[i]);

		Span<byte> utf8Buf = totalBytes <= 2048 ? stackalloc byte[totalBytes] : new byte[totalBytes];
		byte** ptrs = stackalloc byte*[count];

		Span<int> offsets = count <= 64 ? stackalloc int[count] : new int[count];
		int offset = 0;
		for (int i = 0; i < count; i++) {
			offsets[i] = offset;
			int len = NkStringHelper.GetUtf8ByteCount(items[i]);
			NkStringHelper.GetUtf8(items[i], utf8Buf.Slice(offset, len));
			offset += len;
		}

		fixed (byte* basePtr = utf8Buf) {
			for (int i = 0; i < count; i++)
				ptrs[i] = basePtr + offsets[i];
			return Nuklear.nk_combo(_ctx, ptrs, count, selected, itemHeight, size);
		}
	}

	/// <summary>
	/// Begins a custom combo box with a label, invokes the content callback, then ends it.
	/// </summary>
	public bool ComboBeginLabel(string label, nk_vec2 size, Action content) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8) {
			if (Nuklear.nk_combo_begin_label(_ctx, p, size) != 0) {
				content();
				Nuklear.nk_combo_end(_ctx);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Displays a contextual (right-click) menu, invoking the content callback if open.
	/// </summary>
	public bool Contextual(NkPanelFlags flags, nk_vec2 size, NkRect triggerBounds, Action content) {
		if (Nuklear.nk_contextual_begin(_ctx, (uint)flags, size, triggerBounds) != 0) {
			content();
			Nuklear.nk_contextual_end(_ctx);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Displays a labeled item in a contextual menu and returns whether it was clicked.
	/// </summary>
	public bool ContextualItemLabel(string label, NkTextAlign align = (NkTextAlign)NkTextAlignment.NK_TEXT_LEFT) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_contextual_item_label(_ctx, p, (uint)align) != 0;
	}

	/// <summary>
	/// Displays a collapsible tree node, invoking the content callback if expanded.
	/// </summary>
	public bool TreePush(nk_tree_type type, string title, nk_collapse_states initialState, Action content,
		[CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
		int titleLen = NkStringHelper.GetUtf8ByteCount(title);
		Span<byte> titleUtf8 = titleLen <= 512 ? stackalloc byte[titleLen] : new byte[titleLen];
		NkStringHelper.GetUtf8(title, titleUtf8);

		int hashLen = NkStringHelper.GetUtf8ByteCount(file);
		Span<byte> hashUtf8 = hashLen <= 512 ? stackalloc byte[hashLen] : new byte[hashLen];
		NkStringHelper.GetUtf8(file, hashUtf8);

		fixed (byte* pTitle = titleUtf8)
		fixed (byte* pHash = hashUtf8) {
			if (Nuklear.nk_tree_push_hashed(_ctx, type, pTitle, initialState, pHash, hashLen, line) != 0) {
				content();
				Nuklear.nk_tree_pop(_ctx);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Wraps content in a menu bar.
	/// </summary>
	public void MenuBar(Action content) {
		Nuklear.nk_menubar_begin(_ctx);
		content();
		Nuklear.nk_menubar_end(_ctx);
	}

	/// <summary>
	/// Begins a menu with a label, invokes the content callback if open, then ends it.
	/// </summary>
	public bool MenuBeginLabel(string label, NkTextAlign align, nk_vec2 size, Action content) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8) {
			if (Nuklear.nk_menu_begin_label(_ctx, p, (uint)align, size) != 0) {
				content();
				Nuklear.nk_menu_end(_ctx);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Displays a menu item with a label and returns whether it was clicked.
	/// </summary>
	public bool MenuItemLabel(string label, NkTextAlign align = (NkTextAlign)NkTextAlignment.NK_TEXT_LEFT) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_menu_item_label(_ctx, p, (uint)align) != 0;
	}

	static Action<string>? s_clipboardCopyFunc;
	static Func<string>? s_clipboardPasteFunc;

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	static void NkClipboardCopy(NkHandle handle, byte* str, int len) {
		if (s_clipboardCopyFunc is not null && len > 0) {
			string text = Encoding.UTF8.GetString(str, len);
			s_clipboardCopyFunc(text);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	static void NkClipboardPaste(NkHandle handle, nk_text_edit* edit) {
		if (s_clipboardPasteFunc is not null) {
			string text = s_clipboardPasteFunc();
			if (!string.IsNullOrEmpty(text)) {
				int byteCount = Encoding.UTF8.GetByteCount(text);
				Span<byte> bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : new byte[byteCount];
				Encoding.UTF8.GetBytes(text, bytes);
				fixed (byte* pBytes = bytes)
					Nuklear.nk_textedit_paste(edit, pBytes, byteCount);
			}
		}
	}

	public void SetClipboardCallback(Action<string> CopyFunc, Func<string> PasteFunc) {
		s_clipboardCopyFunc = CopyFunc;
		s_clipboardPasteFunc = PasteFunc;

		_ctx->clip.copy = &NkClipboardCopy;
		_ctx->clip.paste = &NkClipboardPaste;
	}

	public void Dispose() {
		if (_disposed)
			return;
		_disposed = true;

		Nuklear.nk_buffer_free(_commands);
		Nuklear.nk_buffer_free(_vertices);
		Nuklear.nk_buffer_free(_indices);

		Nuklear.nk_font_atlas_cleanup(_fontAtlas);
		Nuklear.nk_font_atlas_clear(_fontAtlas);

		Nuklear.nk_free(_ctx);

		NativeMemory.Free(_commands);
		NativeMemory.Free(_vertices);
		NativeMemory.Free(_indices);
		NativeMemory.Free(_fontAtlas);
		NativeMemory.Free(_ctx);
		NativeMemory.Free(_convertCfg);
		NativeMemory.Free(_nullTexture);
		NativeMemory.Free(_allocator);
		NativeMemory.Free(_vertexLayout);

		_commands = null;
		_vertices = null;
		_indices = null;
		_fontAtlas = null;
		_ctx = null;
		_convertCfg = null;
		_nullTexture = null;
		_allocator = null;
		_vertexLayout = null;
		_frameBuffered = null;
		_renderHooks = null;
		_dev = null;
	}
}
