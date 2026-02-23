using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NuklearDotNet {
	public unsafe delegate void FontStashAction(nk_font_atlas* Atlas);

	public unsafe interface INuklearDeviceInit {
		void Init();
	}

	public unsafe interface INuklearDeviceFontStash {
		void FontStash(nk_font_atlas* Atlas);
	}

	public interface INuklearDeviceRenderHooks {
		void BeginRender();
		void EndRender();
	}

	public static unsafe partial class NuklearAPI {
		public static nk_context* Ctx { get; private set; }
		public static NuklearDevice? Dev { get; private set; }

		static nk_allocator* Allocator;
		static nk_font_atlas* FontAtlas;
		static nk_draw_null_texture* NullTexture;
		static nk_convert_config* ConvertCfg;

		static nk_buffer* Commands, Vertices, Indices;

		static nk_draw_vertex_layout_element* VertexLayout;

		static IFrameBuffered? FrameBuffered;
		static INuklearDeviceRenderHooks? RenderHooks;

		static bool ForceUpdateQueued;
		static bool Initialized;

		static readonly nuint VertexPositionOffset = 0;
		static readonly nuint VertexUVOffset = (nuint)sizeof(NkVector2f);
		static readonly nuint VertexColorOffset = (nuint)(sizeof(NkVector2f) * 2);

		static NuklearDevice GetDeviceOrThrow() =>
			Dev ?? throw new InvalidOperationException("NuklearAPI.Init has not been called");

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

		public static float DefaultFontHeight { get; set; }

		static void FontStash(FontStashAction? A = null) {
			Nuklear.nk_font_atlas_init_default(FontAtlas);
			Nuklear.nk_font_atlas_begin(FontAtlas);

			if (DefaultFontHeight > 0)
				Nuklear.nk_font_atlas_add_default_ex(FontAtlas, DefaultFontHeight);

			A?.Invoke(FontAtlas);

			int W, H;
			IntPtr Image = Nuklear.nk_font_atlas_bake(FontAtlas, &W, &H, nk_font_atlas_format.NK_FONT_ATLAS_RGBA32);

			int TexHandle = GetDeviceOrThrow().CreateTextureHandle(W, H, Image);
			Nuklear.nk_font_atlas_end(FontAtlas, Nuklear.nk_handle_id(TexHandle), NullTexture);

			if (FontAtlas->default_font != null)
				Nuklear.nk_style_set_font(Ctx, &FontAtlas->default_font->handle);
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

		public static void HandleInput() {
			Nuklear.nk_input_begin(Ctx);

			var dev = GetDeviceOrThrow();
			while (dev.HasPendingEvents) {
				NuklearEvent E = dev.DequeueEvent();

				switch (E.EvtType) {
					case NuklearEvent.EventType.MouseButton:
						Nuklear.nk_input_button(Ctx, (nk_buttons)E.MButton, E.X, E.Y, E.Down ? 1 : 0);
						break;

					case NuklearEvent.EventType.MouseMove:
						Nuklear.nk_input_motion(Ctx, E.X, E.Y);
						break;

					case NuklearEvent.EventType.Scroll:
						Nuklear.nk_input_scroll(Ctx, new nk_vec2() { x = E.ScrollX, y = E.ScrollY });
						break;

					case NuklearEvent.EventType.Text:
						for (int i = 0; i < E.Text.Length; i++) {
							if (!char.IsControl(E.Text[i]))
								Nuklear.nk_input_unicode(Ctx, E.Text[i]);
						}
						break;

					case NuklearEvent.EventType.KeyboardKey:
						Nuklear.nk_input_key(Ctx, E.Key, E.Down ? 1 : 0);
						break;

					case NuklearEvent.EventType.ForceUpdate:
						break;

					default:
						throw new NotImplementedException();
				}
			}

			Nuklear.nk_input_end(Ctx);
		}

		static void RenderDrawCommand(nk_draw_command* Cmd, ref uint Offset) {
			if (Cmd->elem_count == 0)
				return;

			GetDeviceOrThrow().Render(Cmd->userdata, Cmd->texture.id, Cmd->clip_rect, Offset, Cmd->elem_count);
			Offset += Cmd->elem_count;
		}

		public static void Render() {
			NkConvertResult R = (NkConvertResult)Nuklear.nk_convert(Ctx, Commands, Vertices, Indices, ConvertCfg);
			if (R != NkConvertResult.Success)
				throw new InvalidOperationException($"nk_convert failed: {R}");

			int vertCount = (int)(Vertices->allocated / (nuint)sizeof(NkVertex));
			int indexCount = (int)(Indices->allocated / (nuint)sizeof(ushort));
			NkVertex* VertsPtr = (NkVertex*)Vertices->memory.ptr;
			ushort* IndicesPtr = (ushort*)Indices->memory.ptr;

			var vertsSpan = new ReadOnlySpan<NkVertex>(VertsPtr, vertCount);
			var indicesSpan = new ReadOnlySpan<ushort>(IndicesPtr, indexCount);

			GetDeviceOrThrow().SetBuffer(vertsSpan, indicesSpan);
			FrameBuffered?.BeginBuffering();

			uint Offset = 0;
			RenderHooks?.BeginRender();

			nk_draw_command* Cmd = Nuklear.nk__draw_begin(Ctx, Commands);
			while (Cmd != null) {
				RenderDrawCommand(Cmd, ref Offset);
				Cmd = Nuklear.nk__draw_next(Cmd, Commands, Ctx);
			}

			RenderHooks?.EndRender();
			FrameBuffered?.EndBuffering();

			Nuklear.nk_buffer_clear(Commands);
			Nuklear.nk_buffer_clear(Vertices);
			Nuklear.nk_buffer_clear(Indices);
			Nuklear.nk_clear(Ctx);

			FrameBuffered?.RenderFinal();
		}

		public static void Init(NuklearDevice Device) {
			if (Initialized)
				throw new InvalidOperationException("NuklearAPI.Init has already been called");

			Initialized = true;
			Dev = Device;
			FrameBuffered = Device as IFrameBuffered;
			RenderHooks = Device as INuklearDeviceRenderHooks;

			int nativeContextSize = Nuklear.nk_debug_sizeof_context();
			int nativeFontAtlasSize = Nuklear.nk_debug_sizeof_font_atlas();

			Ctx = (nk_context*)ManagedAlloc((nuint)nativeContextSize);
			Allocator = (nk_allocator*)ManagedAlloc((nuint)sizeof(nk_allocator));
			FontAtlas = (nk_font_atlas*)ManagedAlloc((nuint)nativeFontAtlasSize);
			NullTexture = (nk_draw_null_texture*)ManagedAlloc((nuint)sizeof(nk_draw_null_texture));
			ConvertCfg = (nk_convert_config*)ManagedAlloc((nuint)sizeof(nk_convert_config));
			Commands = (nk_buffer*)ManagedAlloc((nuint)sizeof(nk_buffer));
			Vertices = (nk_buffer*)ManagedAlloc((nuint)sizeof(nk_buffer));
			Indices = (nk_buffer*)ManagedAlloc((nuint)sizeof(nk_buffer));

			VertexLayout = (nk_draw_vertex_layout_element*)ManagedAlloc((nuint)(sizeof(nk_draw_vertex_layout_element) * 4));
			VertexLayout[0] = new nk_draw_vertex_layout_element(nk_draw_vertex_layout_attribute.NK_VERTEX_POSITION, nk_draw_vertex_layout_format.NK_FORMAT_FLOAT,
				VertexPositionOffset);
			VertexLayout[1] = new nk_draw_vertex_layout_element(nk_draw_vertex_layout_attribute.NK_VERTEX_TEXCOORD, nk_draw_vertex_layout_format.NK_FORMAT_FLOAT,
				VertexUVOffset);
			VertexLayout[2] = new nk_draw_vertex_layout_element(nk_draw_vertex_layout_attribute.NK_VERTEX_COLOR, nk_draw_vertex_layout_format.NK_FORMAT_R8G8B8A8,
				VertexColorOffset);
			VertexLayout[3] = nk_draw_vertex_layout_element.NK_VERTEX_LAYOUT_END;

			Allocator->alloc = &NkAlloc;
			Allocator->free = &NkFree;

			Nuklear.nk_init_default(Ctx, null);

			(Device as INuklearDeviceInit)?.Init();

			if (Device is INuklearDeviceFontStash fontStasher)
				FontStash(fontStasher.FontStash);
			else
				FontStash();

			ConvertCfg->shape_AA = nk_anti_aliasing.NK_ANTI_ALIASING_ON;
			ConvertCfg->line_AA = nk_anti_aliasing.NK_ANTI_ALIASING_ON;
			ConvertCfg->vertex_layout = VertexLayout;
			ConvertCfg->vertex_size = (nuint)sizeof(NkVertex);
			ConvertCfg->vertex_alignment = 1;
			ConvertCfg->circle_segment_count = 22;
			ConvertCfg->curve_segment_count = 22;
			ConvertCfg->arc_segment_count = 22;
			ConvertCfg->global_alpha = 1.0f;
			ConvertCfg->null_tex = *NullTexture;

			Nuklear.nk_buffer_init_default(Commands);
			Nuklear.nk_buffer_init_default(Vertices);
			Nuklear.nk_buffer_init_default(Indices);
		}

		public static void Frame(Action A) {
			if (!Initialized)
				throw new InvalidOperationException("NuklearAPI.Init has not been called");

			if (ForceUpdateQueued) {
				ForceUpdateQueued = false;
				GetDeviceOrThrow().ForceUpdate();
			}

			HandleInput();
			A();
			Render();
		}

		public static void SetDeltaTime(float Delta) {
			if (Ctx != null)
				Ctx->delta_time_Seconds = Delta;
		}

		public static bool Window(string Name, string Title, NkRect Bounds, NkPanelFlags Flags, Action A) {
			bool Res = true;

			int nameLen = NkStringHelper.GetUtf8ByteCount(Name);
			int titleLen = NkStringHelper.GetUtf8ByteCount(Title);
			Span<byte> nameUtf8 = nameLen <= 512 ? stackalloc byte[nameLen] : new byte[nameLen];
			Span<byte> titleUtf8 = titleLen <= 512 ? stackalloc byte[titleLen] : new byte[titleLen];
			NkStringHelper.GetUtf8(Name, nameUtf8);
			NkStringHelper.GetUtf8(Title, titleUtf8);

			fixed (byte* pName = nameUtf8)
			fixed (byte* pTitle = titleUtf8) {
				if (Nuklear.nk_begin_titled(Ctx, pName, pTitle, Bounds, (uint)Flags) != 0)
					A?.Invoke();
				else
					Res = false;
			}

			Nuklear.nk_end(Ctx);
			return Res;
		}

		public static bool Window(string Title, NkRect Bounds, NkPanelFlags Flags, Action A) => Window(Title, Title, Bounds, Flags, A);

		public static bool WindowIsClosed(string Name) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Name)];
			NkStringHelper.GetUtf8(Name, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_window_is_closed(Ctx, p) != 0;
		}

		public static bool WindowIsHidden(string Name) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Name)];
			NkStringHelper.GetUtf8(Name, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_window_is_hidden(Ctx, p) != 0;
		}

		public static bool WindowIsCollapsed(string Name) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Name)];
			NkStringHelper.GetUtf8(Name, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_window_is_collapsed(Ctx, p) != 0;
		}

		public static bool Group(string Name, string Title, NkPanelFlags Flags, Action A) {
			int nameLen = NkStringHelper.GetUtf8ByteCount(Name);
			int titleLen = NkStringHelper.GetUtf8ByteCount(Title);
			Span<byte> nameUtf8 = nameLen <= 512 ? stackalloc byte[nameLen] : new byte[nameLen];
			Span<byte> titleUtf8 = titleLen <= 512 ? stackalloc byte[titleLen] : new byte[titleLen];
			NkStringHelper.GetUtf8(Name, nameUtf8);
			NkStringHelper.GetUtf8(Title, titleUtf8);

			fixed (byte* pName = nameUtf8)
			fixed (byte* pTitle = titleUtf8) {
				if (Nuklear.nk_group_begin_titled(Ctx, pName, pTitle, (uint)Flags) != 0) {
					A?.Invoke();
					Nuklear.nk_group_end(Ctx);
					return true;
				}
			}
			return false;
		}

		public static bool Group(string Name, NkPanelFlags Flags, Action A) => Group(Name, Name, Flags, A);

		public static bool ButtonLabel(string Label) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Label)];
			NkStringHelper.GetUtf8(Label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_button_label(Ctx, p) != 0;
		}

		public static bool ButtonText(string Text) {
			int byteCount = NkStringHelper.GetUtf8ByteCount(Text);
			Span<byte> utf8 = stackalloc byte[byteCount];
			int written = NkStringHelper.GetUtf8(Text, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_button_text(Ctx, p, written) != 0;
		}

		public static bool ButtonText(char Char) => ButtonText(Char.ToString());

		public static void LayoutRowStatic(float Height, int ItemWidth, int Cols) {
			Nuklear.nk_layout_row_static(Ctx, Height, ItemWidth, Cols);
		}

		public static void LayoutRowDynamic(float Height = 0, int Cols = 1) {
			Nuklear.nk_layout_row_dynamic(Ctx, Height, Cols);
		}

		public static void Label(string Txt, NkTextAlign TextAlign = (NkTextAlign)NkTextAlignment.NK_TEXT_LEFT) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Txt)];
			NkStringHelper.GetUtf8(Txt, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_label(Ctx, p, (uint)TextAlign);
		}

		public static void LabelWrap(string Txt) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Txt)];
			NkStringHelper.GetUtf8(Txt, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_label_wrap(Ctx, p);
		}

		public static void LabelColored(string Txt, NkColor Clr, NkTextAlign TextAlign = (NkTextAlign)NkTextAlignment.NK_TEXT_LEFT) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Txt)];
			NkStringHelper.GetUtf8(Txt, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_label_colored(Ctx, p, (uint)TextAlign, Clr);
		}

		public static void LabelColored(string Txt, NkColor Clr) {
			LabelColored(Txt, Clr, (NkTextAlign)NkTextAlignment.NK_TEXT_LEFT);
		}

		public static void LabelColoredWrap(string Txt, NkColor Clr) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Txt)];
			NkStringHelper.GetUtf8(Txt, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_label_colored_wrap(Ctx, p, Clr);
		}

		public static NkRect WindowGetBounds() {
			return Nuklear.nk_window_get_bounds(Ctx);
		}

		public static NkEditEvents EditString(NkEditTypes EditType, byte[] Buffer, ref int length, int maxLength,
			delegate* unmanaged[Cdecl]<nk_text_edit*, uint, int> filter) {
			fixed (byte* pBuf = Buffer)
			fixed (int* pLen = &length) {
				return (NkEditEvents)Nuklear.nk_edit_string(Ctx, (uint)EditType, pBuf, pLen, maxLength, filter);
			}
		}

		public static NkEditEvents EditString(NkEditTypes EditType, StringBuilder Buffer) {
			int maxLen = Math.Max(Buffer.Capacity, 256);
			Span<byte> buf = maxLen <= 1024 ? stackalloc byte[maxLen] : new byte[maxLen];
			string current = Buffer.ToString();
			int len = Encoding.UTF8.GetBytes(current, buf);

			NkEditEvents result;
			fixed (byte* pBuf = buf) {
				int lenCopy = len;
				result = (NkEditEvents)Nuklear.nk_edit_string(Ctx, (uint)EditType, pBuf, &lenCopy, maxLen, null);
				len = lenCopy;
			}

			string newStr = Encoding.UTF8.GetString(buf[..len]);
			Buffer.Clear();
			Buffer.Append(newStr);
			return result;
		}

		public delegate int EditFilterDelegate(ref nk_text_edit TextBox, uint Rune);
		private static EditFilterDelegate? s_currentEditFilter;

		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
		static int EditFilterTrampoline(nk_text_edit* te, uint unicode) {
			if (s_currentEditFilter is not null)
				return s_currentEditFilter(ref *te, unicode);
			return 1;
		}

		public static NkEditEvents EditString(NkEditTypes EditType, StringBuilder Buffer, EditFilterDelegate Filter) {
			int maxLen = Math.Max(Buffer.Capacity, 256);
			Span<byte> buf = maxLen <= 1024 ? stackalloc byte[maxLen] : new byte[maxLen];
			string current = Buffer.ToString();
			int len = Encoding.UTF8.GetBytes(current, buf);

			s_currentEditFilter = Filter;
			NkEditEvents result;
			try {
				fixed (byte* pBuf = buf) {
					int lenCopy = len;
					result = (NkEditEvents)Nuklear.nk_edit_string(Ctx, (uint)EditType, pBuf, &lenCopy, maxLen,
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

		public static bool IsKeyPressed(NkKeys Key) {
			return Nuklear.nk_input_is_key_pressed(&Ctx->input, Key) != 0;
		}

		public static void QueueForceUpdate() {
			ForceUpdateQueued = true;
		}

		public static void WindowClose(string Name) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(Name)];
			NkStringHelper.GetUtf8(Name, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_window_close(Ctx, p);
		}

		/// <summary>
		/// Displays a float slider and returns the updated value.
		/// </summary>
		public static float SlideFloat(float min, float value, float max, float step) {
			return Nuklear.nk_slide_float(Ctx, min, value, max, step);
		}

		/// <summary>
		/// Displays an int slider and returns the updated value.
		/// </summary>
		public static int SlideInt(int min, int value, int max, int step) {
			return Nuklear.nk_slide_int(Ctx, min, value, max, step);
		}

		/// <summary>
		/// Displays a progress bar and returns the updated value.
		/// </summary>
		public static nuint Progress(nuint current, nuint max, nk_modify modifiable = nk_modify.NK_MODIFIABLE) {
			return Nuklear.nk_prog(Ctx, current, max, (int)modifiable);
		}

		/// <summary>
		/// Displays a checkbox and returns the updated active state.
		/// </summary>
		public static bool CheckLabel(string label, bool active) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_check_label(Ctx, p, active ? 1 : 0) != 0;
		}

		/// <summary>
		/// Displays a checkbox that mutates the active state by reference.
		/// Returns true if the checkbox was toggled this frame.
		/// </summary>
		public static bool CheckboxLabel(string label, ref bool active) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			int val = active ? 1 : 0;
			bool toggled;
			fixed (byte* p = utf8)
				toggled = Nuklear.nk_checkbox_label(Ctx, p, &val) != 0;
			active = val != 0;
			return toggled;
		}

		/// <summary>
		/// Displays a radio button and returns whether it is selected.
		/// </summary>
		public static bool OptionLabel(string label, bool active) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_option_label(Ctx, p, active ? 1 : 0) != 0;
		}

		/// <summary>
		/// Displays a color picker widget and returns the updated color.
		/// </summary>
		public static nk_colorf ColorPicker(nk_colorf color, nk_color_format format = nk_color_format.NK_RGBA) {
			return Nuklear.nk_color_picker(Ctx, color, format);
		}

		/// <summary>
		/// Displays an editable float property field and returns the updated value.
		/// </summary>
		public static float PropertyFloat(string name, float value, in NkPropertyRange<float> range) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_propertyf(Ctx, p, range.Min, value, range.Max, range.Step, range.IncPerPixel);
		}

		/// <summary>
		/// Displays an editable int property field and returns the updated value.
		/// </summary>
		public static int PropertyInt(string name, int value, in NkPropertyRange<int> range) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_propertyi(Ctx, p, range.Min, value, range.Max, range.Step, range.IncPerPixel);
		}

		/// <summary>
		/// Displays an editable double property field and returns the updated value.
		/// </summary>
		public static double PropertyDouble(string name, double value, in NkPropertyRange<double> range) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_propertyd(Ctx, p, range.Min, value, range.Max, range.Step, range.IncPerPixel);
		}

		/// <summary>
		/// Displays a float knob widget and returns the updated value.
		/// Uses NK_UP zero direction and no dead zone.
		/// </summary>
		public static float KnobFloat(float min, float value, float max, float step) {
			Nuklear.nk_knob_float(Ctx, min, &value, max, step, nk_heading.NK_UP, 0);
			return value;
		}

		/// <summary>
		/// Displays an int knob widget and returns the updated value.
		/// Uses NK_UP zero direction and no dead zone.
		/// </summary>
		public static int KnobInt(int min, int value, int max, int step) {
			Nuklear.nk_knob_int(Ctx, min, &value, max, step, nk_heading.NK_UP, 0);
			return value;
		}

		/// <summary>
		/// Displays a colored button and returns whether it was clicked.
		/// </summary>
		public static bool ButtonColor(NkColor color) {
			return Nuklear.nk_button_color(Ctx, color) != 0;
		}

		/// <summary>
		/// Displays a symbol button and returns whether it was clicked.
		/// </summary>
		public static bool ButtonSymbol(nk_symbol_type symbol) {
			return Nuklear.nk_button_symbol(Ctx, symbol) != 0;
		}

		/// <summary>
		/// Displays a selectable label and returns the updated selection state.
		/// </summary>
		public static bool SelectLabel(string label, NkTextAlign align, bool selected) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_select_label(Ctx, p, (uint)align, selected ? 1 : 0) != 0;
		}

		/// <summary>
		/// Displays an empty spacer widget.
		/// </summary>
		public static void Spacer() {
			Nuklear.nk_spacer(Ctx);
		}

		/// <summary>
		/// Displays a horizontal rule (separator line).
		/// </summary>
		public static void RuleHorizontal(NkColor color, int rounding = 0) {
			Nuklear.nk_rule_horizontal(Ctx, color, rounding);
		}

		/// <summary>
		/// Begins a line chart, invokes the callback to push values, then ends the chart.
		/// </summary>
		public static bool ChartLines(int count, float min, float max, Action<int> pushValues) {
			return ChartImpl(nk_chart_type.NK_CHART_LINES, count, min, max, pushValues);
		}

		/// <summary>
		/// Begins a column chart, invokes the callback to push values, then ends the chart.
		/// </summary>
		public static bool ChartColumns(int count, float min, float max, Action<int> pushValues) {
			return ChartImpl(nk_chart_type.NK_CHART_COLUMN, count, min, max, pushValues);
		}

		static bool ChartImpl(nk_chart_type type, int count, float min, float max, Action<int> pushValues) {
			if (Nuklear.nk_chart_begin(Ctx, type, count, min, max) != 0) {
				pushValues(count);
				Nuklear.nk_chart_end(Ctx);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Pushes a value to the current chart being built.
		/// </summary>
		public static void ChartPush(float value) {
			Nuklear.nk_chart_push(Ctx, value);
		}

		/// <summary>
		/// Displays a simple plot chart from a span of float values.
		/// </summary>
		public static void Plot(nk_chart_type type, ReadOnlySpan<float> values) {
			fixed (float* p = values)
				Nuklear.nk_plot(Ctx, type, p, values.Length, 0);
		}

		/// <summary>
		/// Displays a simple text tooltip at the current widget position.
		/// </summary>
		public static void Tooltip(string text) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(text)];
			NkStringHelper.GetUtf8(text, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_tooltip(Ctx, p);
		}

		/// <summary>
		/// Begins a custom tooltip with the given width, invokes the content callback, then ends it.
		/// </summary>
		public static void Tooltip(float width, Action content) {
			if (Nuklear.nk_tooltip_begin(Ctx, width) != 0) {
				content();
				Nuklear.nk_tooltip_end(Ctx);
			}
		}

		/// <summary>
		/// Displays a static popup window, invoking the content callback if open.
		/// </summary>
		public static bool PopupStatic(string title, NkPanelFlags flags, NkRect bounds, Action content) {
			return PopupImpl(nk_popup_type.NK_POPUP_STATIC, title, flags, bounds, content);
		}

		/// <summary>
		/// Displays a dynamic popup window, invoking the content callback if open.
		/// </summary>
		public static bool PopupDynamic(string title, NkPanelFlags flags, NkRect bounds, Action content) {
			return PopupImpl(nk_popup_type.NK_POPUP_DYNAMIC, title, flags, bounds, content);
		}

		static bool PopupImpl(nk_popup_type type, string title, NkPanelFlags flags, NkRect bounds, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(title)];
			NkStringHelper.GetUtf8(title, utf8);
			fixed (byte* p = utf8) {
				if (Nuklear.nk_popup_begin(Ctx, type, p, (uint)flags, bounds) != 0) {
					content();
					Nuklear.nk_popup_end(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Closes the currently open popup.
		/// </summary>
		public static void PopupClose() {
			Nuklear.nk_popup_close(Ctx);
		}

		/// <summary>
		/// Displays a combo box from a string array and returns the updated selected index.
		/// </summary>
		public static int Combo(string[] items, int selected, int itemHeight, nk_vec2 size) {
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
				return Nuklear.nk_combo(Ctx, ptrs, count, selected, itemHeight, size);
			}
		}

		/// <summary>
		/// Begins a custom combo box with a label, invokes the content callback, then ends it.
		/// </summary>
		public static bool ComboBeginLabel(string label, nk_vec2 size, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8) {
				if (Nuklear.nk_combo_begin_label(Ctx, p, size) != 0) {
					content();
					Nuklear.nk_combo_end(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Displays a contextual (right-click) menu, invoking the content callback if open.
		/// </summary>
		public static bool Contextual(NkPanelFlags flags, nk_vec2 size, NkRect triggerBounds, Action content) {
			if (Nuklear.nk_contextual_begin(Ctx, (uint)flags, size, triggerBounds) != 0) {
				content();
				Nuklear.nk_contextual_end(Ctx);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Displays a labeled item in a contextual menu and returns whether it was clicked.
		/// </summary>
		public static bool ContextualItemLabel(string label, NkTextAlign align = (NkTextAlign)NkTextAlignment.NK_TEXT_LEFT) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_contextual_item_label(Ctx, p, (uint)align) != 0;
		}

		/// <summary>
		/// Displays a collapsible tree node, invoking the content callback if expanded.
		/// Uses caller file path and line number for unique hash generation.
		/// </summary>
		public static bool TreePush(nk_tree_type type, string title, nk_collapse_states initialState, Action content,
			[CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
			int titleLen = NkStringHelper.GetUtf8ByteCount(title);
			Span<byte> titleUtf8 = titleLen <= 512 ? stackalloc byte[titleLen] : new byte[titleLen];
			NkStringHelper.GetUtf8(title, titleUtf8);

			int hashLen = NkStringHelper.GetUtf8ByteCount(file);
			Span<byte> hashUtf8 = hashLen <= 512 ? stackalloc byte[hashLen] : new byte[hashLen];
			NkStringHelper.GetUtf8(file, hashUtf8);

			fixed (byte* pTitle = titleUtf8)
			fixed (byte* pHash = hashUtf8) {
				if (Nuklear.nk_tree_push_hashed(Ctx, type, pTitle, initialState, pHash, hashLen, line) != 0) {
					content();
					Nuklear.nk_tree_pop(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Wraps content in a menu bar (nk_menubar_begin/end).
		/// </summary>
		public static void MenuBar(Action content) {
			Nuklear.nk_menubar_begin(Ctx);
			content();
			Nuklear.nk_menubar_end(Ctx);
		}

		/// <summary>
		/// Begins a menu with a label, invokes the content callback if open, then ends it.
		/// </summary>
		public static bool MenuBeginLabel(string label, NkTextAlign align, nk_vec2 size, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8) {
				if (Nuklear.nk_menu_begin_label(Ctx, p, (uint)align, size) != 0) {
					content();
					Nuklear.nk_menu_end(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Displays a menu item with a label and returns whether it was clicked.
		/// </summary>
		public static bool MenuItemLabel(string label, NkTextAlign align = (NkTextAlign)NkTextAlignment.NK_TEXT_LEFT) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_menu_item_label(Ctx, p, (uint)align) != 0;
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

		public static void SetClipboardCallback(Action<string> CopyFunc, Func<string> PasteFunc) {
			s_clipboardCopyFunc = CopyFunc;
			s_clipboardPasteFunc = PasteFunc;

			Ctx->clip.copy = &NkClipboardCopy;
			Ctx->clip.paste = &NkClipboardPaste;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public readonly struct NkVector2f {
		public readonly float X, Y;

		public NkVector2f(float X, float Y) {
			this.X = X;
			this.Y = Y;
		}

		public override string ToString() {
			return $"({X}, {Y})";
		}

		public static implicit operator Vector2(NkVector2f V) {
			return new Vector2(V.X, V.Y);
		}

		public static implicit operator NkVector2f(Vector2 V) {
			return new NkVector2f(V.X, V.Y);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public readonly struct NkVertex {
		public readonly NkVector2f Position;
		public readonly NkVector2f UV;
		public readonly NkColor Color;

		public override string ToString() {
			return $"Position: {Position}; UV: {UV}; Color: {Color}";
		}
	}

	public readonly struct NuklearEvent {
		public enum EventType {
			MouseButton,
			MouseMove,
			Scroll,
			Text,
			KeyboardKey,
			ForceUpdate
		}

		public enum MouseButton {
			Left, Middle, Right
		}

		public EventType EvtType { get; init; }
		public MouseButton MButton { get; init; }
		public NkKeys Key { get; init; }
		public int X { get; init; }
		public int Y { get; init; }
		public bool Down { get; init; }
		public float ScrollX { get; init; }
		public float ScrollY { get; init; }
		public string Text { get; init; }
	}

	public interface IFrameBuffered {
		void BeginBuffering();
		void EndBuffering();
		void RenderFinal();
	}

	public unsafe abstract class NuklearDevice {
		private readonly Queue<NuklearEvent> _events = new();

		internal bool HasPendingEvents => _events.Count > 0;
		internal NuklearEvent DequeueEvent() => _events.Dequeue();

		public abstract void SetBuffer(ReadOnlySpan<NkVertex> VertexBuffer, ReadOnlySpan<ushort> IndexBuffer);
		public abstract void Render(NkHandle Userdata, int Texture, NkRect ClipRect, uint Offset, uint Count);
		public abstract int CreateTextureHandle(int W, int H, IntPtr Data);

		protected NuklearDevice() {
			ForceUpdate();
		}

		public void OnMouseButton(NuklearEvent.MouseButton MouseButton, int X, int Y, bool Down) {
			_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.MouseButton, MButton = MouseButton, X = X, Y = Y, Down = Down });
		}

		public void OnMouseMove(int X, int Y) {
			_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.MouseMove, X = X, Y = Y });
		}

		public void OnScroll(float ScrollX, float ScrollY) {
			_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.Scroll, ScrollX = ScrollX, ScrollY = ScrollY });
		}

		public void OnText(string Txt) {
			_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.Text, Text = Txt });
		}

		public void OnKey(NkKeys Key, bool Down) {
			_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.KeyboardKey, Key = Key, Down = Down });
		}

		public void ForceUpdate() {
			_events.Enqueue(new NuklearEvent { EvtType = NuklearEvent.EventType.ForceUpdate });
		}
	}

	public unsafe abstract class NuklearDeviceTex<T> : NuklearDevice where T : notnull {
		List<T?> Textures;

		protected NuklearDeviceTex() {
			Textures = new List<T?>();
			Textures.Add(default);
		}

		public int CreateTextureHandle(T Tex) {
			Textures.Add(Tex);
			return Textures.Count - 1;
		}

		public T GetTexture(int Handle) {
			return Textures[Handle] ?? throw new InvalidOperationException($"Texture handle {Handle} is null");
		}

		public sealed override int CreateTextureHandle(int W, int H, IntPtr Data) {
			T Tex = CreateTexture(W, H, Data);
			return CreateTextureHandle(Tex);
		}

		public sealed override void Render(NkHandle Userdata, int Texture, NkRect ClipRect, uint Offset, uint Count) {
			Render(Userdata, GetTexture(Texture), ClipRect, Offset, Count);
		}

		public abstract T CreateTexture(int W, int H, IntPtr Data);

		public abstract void Render(NkHandle Userdata, T Texture, NkRect ClipRect, uint Offset, uint Count);
	}
}
