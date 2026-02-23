using System;

namespace NuklearDotNet {
	public static unsafe partial class NuklearAPI {
		/// <summary>
		/// Gets the position of the current window.
		/// </summary>
		public static nk_vec2 WindowGetPosition() {
			return Nuklear.nk_window_get_position(Ctx);
		}

		/// <summary>
		/// Gets the size of the current window.
		/// </summary>
		public static nk_vec2 WindowGetSize() {
			return Nuklear.nk_window_get_size(Ctx);
		}

		/// <summary>
		/// Gets the width of the current window.
		/// </summary>
		public static float WindowGetWidth() {
			return Nuklear.nk_window_get_width(Ctx);
		}

		/// <summary>
		/// Gets the height of the current window.
		/// </summary>
		public static float WindowGetHeight() {
			return Nuklear.nk_window_get_height(Ctx);
		}

		/// <summary>
		/// Gets the content region rectangle of the current window.
		/// </summary>
		public static NkRect WindowGetContentRegion() {
			return Nuklear.nk_window_get_content_region(Ctx);
		}

		/// <summary>
		/// Gets the minimum position of the content region of the current window.
		/// </summary>
		public static nk_vec2 WindowGetContentRegionMin() {
			return Nuklear.nk_window_get_content_region_min(Ctx);
		}

		/// <summary>
		/// Gets the maximum position of the content region of the current window.
		/// </summary>
		public static nk_vec2 WindowGetContentRegionMax() {
			return Nuklear.nk_window_get_content_region_max(Ctx);
		}

		/// <summary>
		/// Gets the size of the content region of the current window.
		/// </summary>
		public static nk_vec2 WindowGetContentRegionSize() {
			return Nuklear.nk_window_get_content_region_size(Ctx);
		}

		/// <summary>
		/// Gets the command buffer (canvas) for the current window.
		/// </summary>
		public static nk_command_buffer* WindowGetCanvas() {
			return Nuklear.nk_window_get_canvas(Ctx);
		}

		/// <summary>
		/// Gets the panel for the current window.
		/// </summary>
		public static nk_panel* WindowGetPanel() {
			return Nuklear.nk_window_get_panel(Ctx);
		}

		/// <summary>
		/// Returns true if the current window has input focus.
		/// </summary>
		public static bool WindowHasFocus() {
			return Nuklear.nk_window_has_focus(Ctx) != 0;
		}

		/// <summary>
		/// Returns true if the named window is currently active (focused).
		/// </summary>
		public static bool WindowIsActive(string name) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_window_is_active(Ctx, p) != 0;
		}

		/// <summary>
		/// Returns true if the current window is being hovered by the mouse.
		/// </summary>
		public static bool WindowIsHovered() {
			return Nuklear.nk_window_is_hovered(Ctx) != 0;
		}

		/// <summary>
		/// Returns true if any window is being hovered by the mouse.
		/// </summary>
		public static bool WindowIsAnyHovered() {
			return Nuklear.nk_window_is_any_hovered(Ctx) != 0;
		}

		/// <summary>
		/// Returns true if any item (widget) is currently active.
		/// </summary>
		public static bool ItemIsAnyActive() {
			return Nuklear.nk_item_is_any_active(Ctx) != 0;
		}

		/// <summary>
		/// Sets the bounds rectangle of the named window.
		/// </summary>
		public static void WindowSetBounds(string name, NkRect bounds) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_window_set_bounds(Ctx, p, bounds);
		}

		/// <summary>
		/// Sets the position of the named window.
		/// </summary>
		public static void WindowSetPosition(string name, nk_vec2 position) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_window_set_position(Ctx, p, position);
		}

		/// <summary>
		/// Sets the size of the named window.
		/// </summary>
		public static void WindowSetSize(string name, nk_vec2 size) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_window_set_size(Ctx, p, size);
		}

		/// <summary>
		/// Sets input focus to the named window.
		/// </summary>
		public static void WindowSetFocus(string name) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_window_set_focus(Ctx, p);
		}

		/// <summary>
		/// Collapses or expands the named window.
		/// </summary>
		public static void WindowCollapse(string name, nk_collapse_states state) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_window_collapse(Ctx, p, state);
		}

		/// <summary>
		/// Shows or hides the named window.
		/// </summary>
		public static void WindowShow(string name, nk_show_states state) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_window_show(Ctx, p, state);
		}

		/// <summary>
		/// Collapses or expands the named window if the condition is true.
		/// </summary>
		public static void WindowCollapseIf(string name, nk_collapse_states state, bool condition) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_window_collapse_if(Ctx, p, state, condition ? 1 : 0);
		}

		/// <summary>
		/// Shows or hides the named window if the condition is true.
		/// </summary>
		public static void WindowShowIf(string name, nk_show_states state, bool condition) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(name)];
			NkStringHelper.GetUtf8(name, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_window_show_if(Ctx, p, state, condition ? 1 : 0);
		}

		/// <summary>
		/// Gets the scroll offsets of the current window.
		/// </summary>
		public static void WindowGetScroll(out uint offsetX, out uint offsetY) {
			uint x, y;
			Nuklear.nk_window_get_scroll(Ctx, &x, &y);
			offsetX = x;
			offsetY = y;
		}

		/// <summary>
		/// Sets the scroll offsets of the current window.
		/// </summary>
		public static void WindowSetScroll(uint offsetX, uint offsetY) {
			Nuklear.nk_window_set_scroll(Ctx, offsetX, offsetY);
		}
	}
}
