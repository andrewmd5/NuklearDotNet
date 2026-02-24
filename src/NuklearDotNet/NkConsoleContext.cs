using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NuklearDotNet {

	/// <summary>
	/// Lifecycle state for <see cref="NkConsoleContext"/> to prevent use-after-dispose
	/// and enforce correct initialization order.
	/// </summary>
	internal enum NkConsoleState {
		Uninitialized,
		Active,
		Disposed
	}

	/// <summary>
	/// Owns the native <c>nk_console*</c> and <c>nk_gamepads</c> lifetime. Provides gamepad-navigable
	/// console UI widgets over a nuklear context.
	/// <para>
	/// Call <see cref="Poll"/> once per frame before rendering, then <see cref="Render"/> inside
	/// a nuklear window or use <see cref="RenderWindow"/> for a self-contained window.
	/// </para>
	/// </summary>
	/// <remarks>
	/// The <c>nk_gamepads</c> struct is allocated in unmanaged memory and mirrored in C# for
	/// zero-cost per-frame button state reads. SDL3 gamepad subsystem is initialized if
	/// <see cref="CreateWithSdl3"/> is used, and shut down on dispose.
	/// </remarks>
	/// <example>
	/// <code>
	/// using var console = NkConsoleContext.CreateWithSdl3(nkCtx);
	/// console.Button(console.Root, "Play");
	/// console.Button(console.Root, "Settings");
	///
	/// // Per frame:
	/// console.Poll();
	/// console.Render();
	/// </code>
	/// </example>
	public sealed unsafe class NkConsoleContext : IDisposable {
		private const int MaxGamepads = 4;

		private nk_console* _console;
		private NkGamepads* _gamepads;
		private NkConsoleState _state;
		private readonly NkConsoleInputMode _inputMode;

		/// <summary>All unmanaged label allocations made by widget creation methods. Freed on dispose.</summary>
		private List<nint>? _labelAllocations;

		/// <summary>Per-widget reusable buffers for <see cref="SetLabel"/>. Key = widget pointer.</summary>
		private ConcurrentDictionary<nint, (nint Buffer, int Capacity)> _dynamicLabels;

		private NkConsoleContext(NkConsoleInputMode inputMode) {
			_inputMode = inputMode;
			_dynamicLabels = new ConcurrentDictionary<nint, (nint, int)>();
		}

		/// <summary>
		/// Creates a console context with SDL3 gamepad input. Initializes the SDL3 gamepad subsystem.
		/// </summary>
		/// <param name="ctx">A valid nuklear context pointer. Must outlive this instance.</param>
		/// <returns>An initialized console context ready for widget creation.</returns>
		/// <exception cref="InvalidOperationException">SDL3 gamepad subsystem failed to initialize.</exception>
		/// <example>
		/// <code>
		/// using var console = NkConsoleContext.CreateWithSdl3(nkCtx);
		/// </code>
		/// </example>
		public static NkConsoleContext CreateWithSdl3(nk_context* ctx) {
			if (ctx is null)
				throw new ArgumentNullException(nameof(ctx));

			var instance = new NkConsoleContext(NkConsoleInputMode.Sdl3);

			if (Nuklear.nk_sdl3_gamepad_init() == 0)
				throw new InvalidOperationException("SDL3 gamepad subsystem failed to initialize");

			instance._gamepads = (NkGamepads*)NativeMemory.AllocZeroed((nuint)sizeof(NkGamepads));
			NkGamepadInputSource source = Nuklear.nk_gamepad_sdl3_input_source(null);
			Nuklear.nk_gamepad_init_with_source(instance._gamepads, ctx, source);

			instance._console = Nuklear.nk_console_init(ctx);
			if (instance._console is null) {
				instance.DisposeCore();
				throw new InvalidOperationException("nk_console_init returned null");
			}

			Nuklear.nk_console_set_gamepads(instance._console, instance._gamepads);
			instance._state = NkConsoleState.Active;
			return instance;
		}

		/// <summary>
		/// Creates a console context with keyboard-only input. No SDL3 dependency at runtime.
		/// </summary>
		/// <param name="ctx">A valid nuklear context pointer. Must outlive this instance.</param>
		/// <returns>An initialized console context ready for widget creation.</returns>
		/// <example>
		/// <code>
		/// using var console = NkConsoleContext.CreateWithKeyboard(nkCtx);
		/// </code>
		/// </example>
		public static NkConsoleContext CreateWithKeyboard(nk_context* ctx) {
			if (ctx is null)
				throw new ArgumentNullException(nameof(ctx));

			var instance = new NkConsoleContext(NkConsoleInputMode.Keyboard);

			instance._gamepads = (NkGamepads*)NativeMemory.AllocZeroed((nuint)sizeof(NkGamepads));
			NkGamepadInputSource source = Nuklear.nk_gamepad_keyboard_input_source(null);
			Nuklear.nk_gamepad_init_with_source(instance._gamepads, ctx, source);

			instance._console = Nuklear.nk_console_init(ctx);
			if (instance._console is null) {
				instance.DisposeCore();
				throw new InvalidOperationException("nk_console_init returned null");
			}

			Nuklear.nk_console_set_gamepads(instance._console, instance._gamepads);
			instance._state = NkConsoleState.Active;
			return instance;
		}

		/// <summary>
		/// The root console widget. All top-level widgets are children of this node.
		/// </summary>
		/// <example>
		/// <code>
		/// console.Button(console.Root, "Start Game");
		/// </code>
		/// </example>
		public nk_console* Root {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				ThrowIfNotActive();
				return _console;
			}
		}

		/// <summary>
		/// Sets the frame delta time for hold-to-repeat input on sliders and properties.
		/// Call once per frame before <see cref="Poll"/>.
		/// </summary>
		/// <param name="deltaTime">Frame delta time in seconds.</param>
		public static void SetDeltaTime(float deltaTime) {
			Nuklear.nk_console_set_delta_time(deltaTime);
		}

		/// <summary>
		/// Pumps SDL3 events and updates gamepad state so that
		/// <see cref="IsButtonPressed"/>, <see cref="IsButtonDown"/>, and
		/// <see cref="IsButtonReleased"/> return fresh values.
		/// Call once per frame on the render thread — even when the overlay is hidden —
		/// so gamepad combos (e.g., overlay toggle) can be detected.
		/// </summary>
		public void Poll() {
			ThrowIfNotActive();
			if (_inputMode == NkConsoleInputMode.Sdl3)
				Nuklear.nk_sdl3_gamepad_pump_events(_gamepads);
			Nuklear.nk_gamepad_update(_gamepads);
		}

		/// <summary>
		/// Renders the console widget tree. Must be called inside an active nuklear window
		/// (between <c>nk_begin</c> and <c>nk_end</c>).
		/// </summary>
		/// <example>
		/// <code>
		/// // Inside a nuklear window
		/// console.Render();
		/// </code>
		/// </example>
		public void Render() {
			ThrowIfNotActive();
			Nuklear.nk_console_render(_console);
		}

		/// <summary>
		/// Renders the console inside a self-contained nuklear window with the given title and bounds.
		/// </summary>
		/// <param name="title">Window title (UTF-8 encoded internally).</param>
		/// <param name="bounds">Window position and size.</param>
		/// <param name="flags">Nuklear window flags (<c>nk_panel_flags</c>).</param>
		/// <example>
		/// <code>
		/// console.RenderWindow("Settings", new NkRect(100, 100, 400, 300), 0);
		/// </code>
		/// </example>
		public void RenderWindow(string title, NkRect bounds, uint flags) {
			ThrowIfNotActive();
			int byteCount = NkStringHelper.GetUtf8ByteCount(title);
			Span<byte> utf8 = byteCount <= 256 ? stackalloc byte[byteCount] : new byte[byteCount];
			NkStringHelper.GetUtf8(title, utf8);
			fixed (byte* ptr = utf8)
				Nuklear.nk_console_render_window(_console, ptr, bounds, flags);
		}

		#region Gamepad state queries

		/// <summary>
		/// Returns <see langword="true"/> if the button transitioned from released to pressed this frame.
		/// Reads directly from the mirrored <c>nk_gamepads</c> struct — zero P/Invoke cost.
		/// </summary>
		/// <param name="pad">Gamepad index (0 to 3).</param>
		/// <param name="button">The button to query.</param>
		/// <example>
		/// <code>
		/// if (console.IsButtonPressed(0, NkGamepadButton.A))
		///     ActivateWidget();
		/// </code>
		/// </example>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsButtonPressed(int pad, NkGamepadButton button) {
			Debug.Assert((uint)pad < MaxGamepads);
			ref NkGamepad gp = ref Unsafe.Add(ref _gamepads->Pad0, pad);
			uint mask = 1u << (int)button;
			return (gp.Buttons & mask) != 0 && (gp.ButtonsPrev & mask) == 0;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the button is currently held down.
		/// </summary>
		/// <param name="pad">Gamepad index (0 to 3).</param>
		/// <param name="button">The button to query.</param>
		/// <example>
		/// <code>
		/// if (console.IsButtonDown(0, NkGamepadButton.RB))
		///     ScrollFast();
		/// </code>
		/// </example>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsButtonDown(int pad, NkGamepadButton button) {
			Debug.Assert((uint)pad < MaxGamepads);
			ref NkGamepad gp = ref Unsafe.Add(ref _gamepads->Pad0, pad);
			return (gp.Buttons & (1u << (int)button)) != 0;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the button transitioned from pressed to released this frame.
		/// </summary>
		/// <param name="pad">Gamepad index (0 to 3).</param>
		/// <param name="button">The button to query.</param>
		/// <example>
		/// <code>
		/// if (console.IsButtonReleased(0, NkGamepadButton.B))
		///     GoBack();
		/// </code>
		/// </example>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsButtonReleased(int pad, NkGamepadButton button) {
			Debug.Assert((uint)pad < MaxGamepads);
			ref NkGamepad gp = ref Unsafe.Add(ref _gamepads->Pad0, pad);
			uint mask = 1u << (int)button;
			return (gp.Buttons & mask) == 0 && (gp.ButtonsPrev & mask) != 0;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the specified gamepad is connected and available.
		/// </summary>
		/// <param name="pad">Gamepad index (0 to 3).</param>
		/// <example>
		/// <code>
		/// for (int i = 0; i &lt; 4; i++)
		///     if (console.IsGamepadAvailable(i))
		///         ShowPlayerSlot(i);
		/// </code>
		/// </example>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsGamepadAvailable(int pad) {
			Debug.Assert((uint)pad < MaxGamepads);
			ref NkGamepad gp = ref Unsafe.Add(ref _gamepads->Pad0, pad);
			return gp.Available != 0;
		}

		#endregion

		#region Widget creation

		/// <summary>
		/// Adds a navigable button widget to the given parent.
		/// </summary>
		/// <param name="parent">Parent console widget (use <see cref="Root"/> for top-level).</param>
		/// <param name="text">Button label text.</param>
		/// <returns>The created button widget pointer, owned by the console tree.</returns>
		/// <example>
		/// <code>
		/// var btn = console.Button(console.Root, "Play");
		/// </code>
		/// </example>
		public nk_console* Button(nk_console* parent, ReadOnlySpan<char> text) {
			ThrowIfNotActive();
			return Nuklear.nk_console_button(parent, AllocLabel(text));
		}

		/// <summary>
		/// Adds a navigable button with a click callback.
		/// </summary>
		/// <param name="parent">Parent console widget.</param>
		/// <param name="text">Button label text.</param>
		/// <param name="onclick">Unmanaged callback invoked on click. Must be
		/// a <c>[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]</c> static method.</param>
		/// <returns>The created button widget pointer.</returns>
		/// <example>
		/// <code>
		/// [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
		/// static void OnPlay(nk_console* widget, void* userData) { /* ... */ }
		///
		/// console.ButtonOnClick(console.Root, "Play", &amp;OnPlay);
		/// </code>
		/// </example>
		public nk_console* ButtonOnClick(nk_console* parent, ReadOnlySpan<char> text,
			delegate* unmanaged[Cdecl]<nk_console*, void*, void> onclick) {
			ThrowIfNotActive();
			return Nuklear.nk_console_button_onclick(parent, AllocLabel(text), onclick);
		}

		/// <summary>
		/// Adds a non-interactive label widget.
		/// </summary>
		/// <param name="parent">Parent console widget.</param>
		/// <param name="text">Label text.</param>
		/// <returns>The created label widget pointer.</returns>
		/// <example>
		/// <code>
		/// console.Label(console.Root, "Volume");
		/// </code>
		/// </example>
		public nk_console* Label(nk_console* parent, ReadOnlySpan<char> text) {
			ThrowIfNotActive();
			return Nuklear.nk_console_label(parent, AllocLabel(text));
		}

		/// <summary>
		/// Adds a checkbox widget bound to an <see cref="int"/> value (0 = unchecked, 1 = checked).
		/// </summary>
		/// <param name="parent">Parent console widget.</param>
		/// <param name="text">Checkbox label text.</param>
		/// <param name="active">Pointer to the backing int value. Must remain valid while the console is active.</param>
		/// <returns>The created checkbox widget pointer.</returns>
		/// <example>
		/// <code>
		/// int fullscreen = 0;
		/// console.Checkbox(console.Root, "Fullscreen", &amp;fullscreen);
		/// </code>
		/// </example>
		public nk_console* Checkbox(nk_console* parent, ReadOnlySpan<char> text, int* active) {
			ThrowIfNotActive();
			return Nuklear.nk_console_checkbox(parent, AllocLabel(text), active);
		}

		/// <summary>
		/// Adds an integer slider widget.
		/// </summary>
		/// <param name="parent">Parent console widget.</param>
		/// <param name="label">Slider label text.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="val">Pointer to the backing int value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="step">Step increment.</param>
		/// <returns>The created slider widget pointer.</returns>
		/// <example>
		/// <code>
		/// int volume = 50;
		/// console.SliderInt(console.Root, "Volume", 0, &amp;volume, 100, 1);
		/// </code>
		/// </example>
		public nk_console* SliderInt(nk_console* parent, ReadOnlySpan<char> label, int min, int* val, int max, int step) {
			ThrowIfNotActive();
			return Nuklear.nk_console_slider_int(parent, AllocLabel(label), min, val, max, step);
		}

		/// <summary>
		/// Adds a float slider widget.
		/// </summary>
		/// <param name="parent">Parent console widget.</param>
		/// <param name="label">Slider label text.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="val">Pointer to the backing float value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="step">Step increment.</param>
		/// <returns>The created slider widget pointer.</returns>
		/// <example>
		/// <code>
		/// float brightness = 1.0f;
		/// console.SliderFloat(console.Root, "Brightness", 0f, &amp;brightness, 2f, 0.1f);
		/// </code>
		/// </example>
		public nk_console* SliderFloat(nk_console* parent, ReadOnlySpan<char> label, float min, float* val, float max, float step) {
			ThrowIfNotActive();
			return Nuklear.nk_console_slider_float(parent, AllocLabel(label), min, val, max, step);
		}

		/// <summary>
		/// Adds a navigable spacing widget.
		/// </summary>
		/// <param name="parent">Parent console widget.</param>
		/// <param name="cols">Number of columns the spacer occupies.</param>
		/// <returns>The created spacing widget pointer.</returns>
		public nk_console* Spacing(nk_console* parent, int cols = 1) {
			ThrowIfNotActive();
			return Nuklear.nk_console_spacing(parent, cols);
		}

		#endregion

		#region Tree manipulation

		/// <summary>
		/// Removes and frees all child widgets of the given parent.
		/// Use before rebuilding a dynamic submenu (e.g., effects list after preset change).
		/// </summary>
		/// <param name="parent">The parent widget whose children will be freed.</param>
		public void FreeChildren(nk_console* parent) {
			ThrowIfNotActive();
			Nuklear.nk_console_free_children(parent);
		}

		/// <summary>
		/// Sets the visibility of a widget. Invisible widgets are not rendered and
		/// are skipped by gamepad navigation. Use for collapsible section content.
		/// </summary>
		/// <param name="widget">The widget to show or hide.</param>
		/// <param name="visible">Whether the widget should be visible.</param>
		public void SetVisible(nk_console* widget, bool visible) {
			ThrowIfNotActive();
			Nuklear.nk_console_set_visible(widget, visible ? 1 : 0);
		}

		/// <summary>
		/// Returns the screen-space bounds of the currently active (focused) widget.
		/// Call after <see cref="RenderWindow"/> to get valid bounds for drawing a highlight.
		/// </summary>
		public NkRect GetActiveBounds() {
			ThrowIfNotActive();
			return Nuklear.nk_console_get_active_bounds(_console);
		}

		/// <summary>
		/// Sets whether a widget is selectable (navigable) by gamepad/keyboard.
		/// Non-selectable widgets are skipped during up/down navigation.
		/// </summary>
		/// <param name="widget">The widget to modify.</param>
		/// <param name="selectable">Whether the widget should be selectable.</param>
		public void SetSelectable(nk_console* widget, bool selectable) {
			ThrowIfNotActive();
			Nuklear.nk_console_set_selectable(widget, selectable ? 1 : 0);
		}

		/// <summary>
		/// Sets the symbol icon on a button widget (e.g., triangle for collapse indicator).
		/// </summary>
		/// <param name="button">The button widget.</param>
		/// <param name="symbol">The symbol to display.</param>
		public void ButtonSetSymbol(nk_console* button, nk_symbol_type symbol) {
			ThrowIfNotActive();
			Nuklear.nk_console_button_set_symbol(button, (int)symbol);
		}

		/// <summary>
		/// Updates the display text of a label or button widget.
		/// The UTF-8 bytes are copied into a persistent unmanaged buffer that is reused
		/// across calls for the same widget. Safe to call every frame.
		/// </summary>
		/// <param name="widget">The widget to update.</param>
		/// <param name="utf8">UTF-8 encoded label text.</param>
		public void SetLabel(nk_console* widget, ReadOnlySpan<byte> utf8) {
			ThrowIfNotActive();

			nint key = (nint)widget;
			int needed = utf8.Length + 1; // +1 for null terminator

			if (_dynamicLabels.TryGetValue(key, out var existing) && existing.Capacity >= needed) {
				utf8.CopyTo(new Span<byte>((byte*)existing.Buffer, existing.Capacity));
				((byte*)existing.Buffer)[utf8.Length] = 0;
				Nuklear.nk_console_set_label(widget, (byte*)existing.Buffer, utf8.Length);
			}
			else {
				int capacity = Math.Max(needed, 128);
				byte* buffer = (byte*)NativeMemory.Alloc((nuint)capacity);
				utf8.CopyTo(new Span<byte>(buffer, capacity));
				buffer[utf8.Length] = 0;

				if (existing.Buffer != nint.Zero)
					NativeMemory.Free((void*)existing.Buffer);

				_dynamicLabels[key] = ((nint)buffer, capacity);
				Nuklear.nk_console_set_label(widget, buffer, utf8.Length);
			}
		}

		/// <summary>
		/// Adds a combobox widget with items separated by a delimiter character.
		/// <para>
		/// The <paramref name="itemsSeparatedBySeparator"/> buffer must remain valid
		/// for the widget's lifetime — nuklear_console stores the pointer, not a copy.
		/// </para>
		/// </summary>
		/// <param name="parent">Parent console widget.</param>
		/// <param name="label">Combobox label text.</param>
		/// <param name="itemsSeparatedBySeparator">Null-terminated UTF-8 string with items delimited by <paramref name="separator"/>.</param>
		/// <param name="separator">ASCII code of the delimiter character (e.g., <c>';'</c> = 0x3B).</param>
		/// <param name="selected">Pointer to the backing int for the selected index. Must remain valid.</param>
		/// <returns>The created combobox widget pointer.</returns>
		public nk_console* Combobox(nk_console* parent, ReadOnlySpan<char> label,
			byte* itemsSeparatedBySeparator, int separator, int* selected) {
			ThrowIfNotActive();
			return Nuklear.nk_console_combobox(parent, AllocLabel(label), itemsSeparatedBySeparator, separator, selected);
		}

		/// <summary>
		/// Adds a "Back" button that navigates to the parent menu when pressed.
		/// Uses the built-in <c>nk_console_button_back</c> handler.
		/// </summary>
		/// <param name="parent">Parent console widget.</param>
		/// <param name="text">Button label text (typically "< Back").</param>
		/// <returns>The created back button widget pointer.</returns>
		public nk_console* BackButton(nk_console* parent, ReadOnlySpan<char> text) {
			ThrowIfNotActive();
			return Nuklear.nk_console_button_onclick(parent, AllocLabel(text), &BackButtonTrampoline);
		}

		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
		private static void BackButtonTrampoline(nk_console* widget, void* userData) {
			Nuklear.nk_console_button_back(widget, userData);
		}

		#endregion

		#region Label allocation

		/// <summary>
		/// Allocates a persistent null-terminated UTF-8 copy of <paramref name="text"/> in unmanaged memory.
		/// The pointer remains valid until this context is disposed.
		/// </summary>
		private byte* AllocLabel(ReadOnlySpan<char> text) {
			int byteCount = Encoding.UTF8.GetByteCount(text);
			byte* buffer = (byte*)NativeMemory.Alloc((nuint)(byteCount + 1));
			Encoding.UTF8.GetBytes(text, new Span<byte>(buffer, byteCount));
			buffer[byteCount] = 0;

			_labelAllocations ??= new List<nint>(32);
			_labelAllocations.Add((nint)buffer);
			return buffer;
		}

		private void FreeLabelAllocations() {
			if (_labelAllocations is null) return;
			foreach (nint ptr in _labelAllocations)
				NativeMemory.Free((void*)ptr);
			_labelAllocations.Clear();
		}

		private void FreeDynamicLabels() {
			foreach (var kvp in _dynamicLabels) {
				if (kvp.Value.Buffer != nint.Zero)
					NativeMemory.Free((void*)kvp.Value.Buffer);
			}
			_dynamicLabels.Clear();
		}

		#endregion

		#region Disposal

		/// <inheritdoc/>
		public void Dispose() {
			if (_state == NkConsoleState.Disposed)
				return;
			DisposeCore();
		}

		private void DisposeCore() {
			if (_console is not null) {
				Nuklear.nk_console_free(_console);
				_console = null;
			}

			FreeLabelAllocations();
			FreeDynamicLabels();

			if (_gamepads is not null) {
				Nuklear.nk_gamepad_free(_gamepads);
				NativeMemory.Free(_gamepads);
				_gamepads = null;
			}
			if (_inputMode == NkConsoleInputMode.Sdl3)
				Nuklear.nk_sdl3_gamepad_shutdown();
			_state = NkConsoleState.Disposed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ThrowIfNotActive() {
			if (_state != NkConsoleState.Active)
				ThrowBadState();
		}

		private void ThrowBadState() {
			throw _state switch {
				NkConsoleState.Disposed => new ObjectDisposedException(nameof(NkConsoleContext)),
				NkConsoleState.Uninitialized => new InvalidOperationException("NkConsoleContext has not been initialized — use CreateWithSdl3 or CreateWithKeyboard"),
				_ => new InvalidOperationException($"NkConsoleContext is in unexpected state: {_state}")
			};
		}

		#endregion
	}

	/// <summary>
	/// Determines the gamepad input backend used by <see cref="NkConsoleContext"/>.
	/// </summary>
	internal enum NkConsoleInputMode {
		Keyboard,
		Sdl3
	}
}
