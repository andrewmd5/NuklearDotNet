namespace NuklearDotNet;

public sealed unsafe partial class NuklearContext {
	/// <summary>
	/// Returns true if the specified key was released this frame.
	/// </summary>
	public bool IsKeyReleased(NkKeys key) {
		return Nuklear.nk_input_is_key_released(&_ctx->input, key) != 0;
	}

	/// <summary>
	/// Returns true if the specified key is currently held down.
	/// </summary>
	public bool IsKeyDown(NkKeys key) {
		return Nuklear.nk_input_is_key_down(&_ctx->input, key) != 0;
	}

	/// <summary>
	/// Returns true if the specified mouse button is currently held down.
	/// </summary>
	public bool IsMouseDown(nk_buttons button) {
		return Nuklear.nk_input_is_mouse_down(&_ctx->input, button) != 0;
	}

	/// <summary>
	/// Returns true if the specified mouse button was pressed this frame.
	/// </summary>
	public bool IsMousePressed(nk_buttons button) {
		return Nuklear.nk_input_is_mouse_pressed(&_ctx->input, button) != 0;
	}

	/// <summary>
	/// Returns true if the specified mouse button was released this frame.
	/// </summary>
	public bool IsMouseReleased(nk_buttons button) {
		return Nuklear.nk_input_is_mouse_released(&_ctx->input, button) != 0;
	}

	/// <summary>
	/// Returns true if the mouse is hovering over the specified rectangle.
	/// </summary>
	public bool IsMouseHoveringRect(NkRect rect) {
		return Nuklear.nk_input_is_mouse_hovering_rect(&_ctx->input, rect) != 0;
	}

	/// <summary>
	/// Returns true if the specified mouse button was clicked within the rectangle.
	/// </summary>
	public bool IsMouseClickInRect(nk_buttons button, NkRect rect) {
		return Nuklear.nk_input_is_mouse_click_in_rect(&_ctx->input, button, rect) != 0;
	}

	/// <summary>
	/// Returns true if any mouse button was clicked within the specified rectangle.
	/// </summary>
	public bool AnyMouseClickInRect(NkRect rect) {
		return Nuklear.nk_input_any_mouse_click_in_rect(&_ctx->input, rect) != 0;
	}
}
