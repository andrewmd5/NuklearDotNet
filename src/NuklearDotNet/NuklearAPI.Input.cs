namespace NuklearDotNet {
	public static unsafe partial class NuklearAPI {
		/// <summary>
		/// Returns true if the specified key was released this frame.
		/// </summary>
		public static bool IsKeyReleased(NkKeys key) {
			return Nuklear.nk_input_is_key_released(&Ctx->input, key) != 0;
		}

		/// <summary>
		/// Returns true if the specified key is currently held down.
		/// </summary>
		public static bool IsKeyDown(NkKeys key) {
			return Nuklear.nk_input_is_key_down(&Ctx->input, key) != 0;
		}

		/// <summary>
		/// Returns true if the specified mouse button is currently held down.
		/// </summary>
		public static bool IsMouseDown(nk_buttons button) {
			return Nuklear.nk_input_is_mouse_down(&Ctx->input, button) != 0;
		}

		/// <summary>
		/// Returns true if the specified mouse button was pressed this frame.
		/// </summary>
		public static bool IsMousePressed(nk_buttons button) {
			return Nuklear.nk_input_is_mouse_pressed(&Ctx->input, button) != 0;
		}

		/// <summary>
		/// Returns true if the specified mouse button was released this frame.
		/// </summary>
		public static bool IsMouseReleased(nk_buttons button) {
			return Nuklear.nk_input_is_mouse_released(&Ctx->input, button) != 0;
		}

		/// <summary>
		/// Returns true if the mouse is hovering over the specified rectangle.
		/// </summary>
		public static bool IsMouseHoveringRect(NkRect rect) {
			return Nuklear.nk_input_is_mouse_hovering_rect(&Ctx->input, rect) != 0;
		}

		/// <summary>
		/// Returns true if the specified mouse button was clicked within the rectangle.
		/// </summary>
		public static bool IsMouseClickInRect(nk_buttons button, NkRect rect) {
			return Nuklear.nk_input_is_mouse_click_in_rect(&Ctx->input, button, rect) != 0;
		}

		/// <summary>
		/// Returns true if any mouse button was clicked within the specified rectangle.
		/// </summary>
		public static bool AnyMouseClickInRect(NkRect rect) {
			return Nuklear.nk_input_any_mouse_click_in_rect(&Ctx->input, rect) != 0;
		}
	}
}
