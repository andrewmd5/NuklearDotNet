using System;

namespace NuklearDotNet;

public sealed unsafe partial class NuklearContext {
	/// <summary>
	/// Gets the position of the current window.
	/// </summary>
	public nk_vec2 WindowGetPosition() {
		return Nuklear.nk_window_get_position(_ctx);
	}

	/// <summary>
	/// Gets the size of the current window.
	/// </summary>
	public nk_vec2 WindowGetSize() {
		return Nuklear.nk_window_get_size(_ctx);
	}

	/// <summary>
	/// Gets the width of the current window.
	/// </summary>
	public float WindowGetWidth() {
		return Nuklear.nk_window_get_width(_ctx);
	}

	/// <summary>
	/// Gets the height of the current window.
	/// </summary>
	public float WindowGetHeight() {
		return Nuklear.nk_window_get_height(_ctx);
	}

	/// <summary>
	/// Gets the content region rectangle of the current window.
	/// </summary>
	public NkRect WindowGetContentRegion() {
		return Nuklear.nk_window_get_content_region(_ctx);
	}

	/// <summary>
	/// Gets the minimum position of the content region of the current window.
	/// </summary>
	public nk_vec2 WindowGetContentRegionMin() {
		return Nuklear.nk_window_get_content_region_min(_ctx);
	}

	/// <summary>
	/// Gets the maximum position of the content region of the current window.
	/// </summary>
	public nk_vec2 WindowGetContentRegionMax() {
		return Nuklear.nk_window_get_content_region_max(_ctx);
	}

	/// <summary>
	/// Gets the size of the content region of the current window.
	/// </summary>
	public nk_vec2 WindowGetContentRegionSize() {
		return Nuklear.nk_window_get_content_region_size(_ctx);
	}

	/// <summary>
	/// Gets the command buffer (canvas) for the current window.
	/// </summary>
	public nk_command_buffer* WindowGetCanvas() {
		return Nuklear.nk_window_get_canvas(_ctx);
	}

	/// <summary>
	/// Gets the panel for the current window.
	/// </summary>
	public nk_panel* WindowGetPanel() {
		return Nuklear.nk_window_get_panel(_ctx);
	}

	/// <summary>
	/// Returns true if the current window has input focus.
	/// </summary>
	public bool WindowHasFocus() {
		return Nuklear.nk_window_has_focus(_ctx) != 0;
	}

	/// <summary>
	/// Returns true if the named window is currently active (focused).
	/// </summary>
	public bool WindowIsActive(string name) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_window_is_active(_ctx, p) != 0;
	}

	/// <summary>
	/// Returns true if the current window is being hovered by the mouse.
	/// </summary>
	public bool WindowIsHovered() {
		return Nuklear.nk_window_is_hovered(_ctx) != 0;
	}

	/// <summary>
	/// Returns true if any window is being hovered by the mouse.
	/// </summary>
	public bool WindowIsAnyHovered() {
		return Nuklear.nk_window_is_any_hovered(_ctx) != 0;
	}

	/// <summary>
	/// Returns true if any item (widget) is currently active.
	/// </summary>
	public bool ItemIsAnyActive() {
		return Nuklear.nk_item_is_any_active(_ctx) != 0;
	}

	/// <summary>
	/// Sets the bounds rectangle of the named window.
	/// </summary>
	public void WindowSetBounds(string name, NkRect bounds) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_window_set_bounds(_ctx, p, bounds);
	}

	/// <summary>
	/// Sets the position of the named window.
	/// </summary>
	public void WindowSetPosition(string name, nk_vec2 position) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_window_set_position(_ctx, p, position);
	}

	/// <summary>
	/// Sets the size of the named window.
	/// </summary>
	public void WindowSetSize(string name, nk_vec2 size) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_window_set_size(_ctx, p, size);
	}

	/// <summary>
	/// Sets input focus to the named window.
	/// </summary>
	public void WindowSetFocus(string name) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_window_set_focus(_ctx, p);
	}

	/// <summary>
	/// Collapses or expands the named window.
	/// </summary>
	public void WindowCollapse(string name, nk_collapse_states state) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_window_collapse(_ctx, p, state);
	}

	/// <summary>
	/// Shows or hides the named window.
	/// </summary>
	public void WindowShow(string name, nk_show_states state) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_window_show(_ctx, p, state);
	}

	/// <summary>
	/// Collapses or expands the named window if the condition is true.
	/// </summary>
	public void WindowCollapseIf(string name, nk_collapse_states state, bool condition) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_window_collapse_if(_ctx, p, state, condition ? 1 : 0);
	}

	/// <summary>
	/// Shows or hides the named window if the condition is true.
	/// </summary>
	public void WindowShowIf(string name, nk_show_states state, bool condition) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
		NkStringHelper.GetUtf8(name, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_window_show_if(_ctx, p, state, condition ? 1 : 0);
	}

	/// <summary>
	/// Gets the scroll offsets of the current window.
	/// </summary>
	public void WindowGetScroll(out uint offsetX, out uint offsetY) {
		uint x, y;
		Nuklear.nk_window_get_scroll(_ctx, &x, &y);
		offsetX = x;
		offsetY = y;
	}

	/// <summary>
	/// Sets the scroll offsets of the current window.
	/// </summary>
	public void WindowSetScroll(uint offsetX, uint offsetY) {
		Nuklear.nk_window_set_scroll(_ctx, offsetX, offsetY);
	}
}
