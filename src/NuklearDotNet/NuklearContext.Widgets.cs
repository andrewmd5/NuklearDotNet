using System;

namespace NuklearDotNet;

public sealed unsafe partial class NuklearContext {
	#region Widget Utilities

	/// <summary>
	/// Returns the bounding rectangle of the current widget.
	/// </summary>
	public NkRect WidgetBounds() {
		return Nuklear.nk_widget_bounds(_ctx);
	}

	/// <summary>
	/// Returns the position of the current widget.
	/// </summary>
	public nk_vec2 WidgetPosition() {
		return Nuklear.nk_widget_position(_ctx);
	}

	/// <summary>
	/// Returns the size of the current widget.
	/// </summary>
	public nk_vec2 WidgetSize() {
		return Nuklear.nk_widget_size(_ctx);
	}

	/// <summary>
	/// Returns the width of the current widget.
	/// </summary>
	public float WidgetWidth() {
		return Nuklear.nk_widget_width(_ctx);
	}

	/// <summary>
	/// Returns the height of the current widget.
	/// </summary>
	public float WidgetHeight() {
		return Nuklear.nk_widget_height(_ctx);
	}

	/// <summary>
	/// Returns true if the current widget is being hovered by the mouse.
	/// </summary>
	public bool WidgetIsHovered() {
		return Nuklear.nk_widget_is_hovered(_ctx) != 0;
	}

	/// <summary>
	/// Returns true if the current widget was clicked with the specified mouse button.
	/// </summary>
	public bool WidgetIsMouseClicked(nk_buttons button) {
		return Nuklear.nk_widget_is_mouse_clicked(_ctx, button) != 0;
	}

	/// <summary>
	/// Adds spacing columns to the current row layout.
	/// </summary>
	public void Spacing(int cols) {
		Nuklear.nk_spacing(_ctx, cols);
	}

	/// <summary>
	/// Disables all widgets within the callback, making them non-interactive and visually dimmed.
	/// </summary>
	public void WidgetDisable(Action content) {
		Nuklear.nk_widget_disable_begin(_ctx);
		content();
		Nuklear.nk_widget_disable_end(_ctx);
	}

	#endregion

	#region Text/Label

	/// <summary>
	/// Displays length-counted text with the specified alignment.
	/// </summary>
	public void Text(string text, NkTextAlign align) {
		int byteCount = NkStringHelper.GetUtf8ByteCount(text);
		Span<byte> utf8 = byteCount <= 512 ? stackalloc byte[byteCount] : new byte[byteCount];
		int written = NkStringHelper.GetUtf8(text, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_text(_ctx, p, written, (uint)align);
	}

	/// <summary>
	/// Displays length-counted colored text with the specified alignment.
	/// </summary>
	public void TextColored(string text, NkTextAlign align, NkColor color) {
		int byteCount = NkStringHelper.GetUtf8ByteCount(text);
		Span<byte> utf8 = byteCount <= 512 ? stackalloc byte[byteCount] : new byte[byteCount];
		int written = NkStringHelper.GetUtf8(text, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_text_colored(_ctx, p, written, (uint)align, color);
	}

	/// <summary>
	/// Displays length-counted wrapping text.
	/// </summary>
	public void TextWrap(string text) {
		int byteCount = NkStringHelper.GetUtf8ByteCount(text);
		Span<byte> utf8 = byteCount <= 512 ? stackalloc byte[byteCount] : new byte[byteCount];
		int written = NkStringHelper.GetUtf8(text, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_text_wrap(_ctx, p, written);
	}

	/// <summary>
	/// Displays length-counted wrapping text with a custom color.
	/// </summary>
	public void TextWrapColored(string text, NkColor color) {
		int byteCount = NkStringHelper.GetUtf8ByteCount(text);
		Span<byte> utf8 = byteCount <= 512 ? stackalloc byte[byteCount] : new byte[byteCount];
		int written = NkStringHelper.GetUtf8(text, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_text_wrap_colored(_ctx, p, written, color);
	}

	/// <summary>
	/// Displays an image widget.
	/// </summary>
	public void Image(nk_image img) {
		Nuklear.nk_image(_ctx, img);
	}

	/// <summary>
	/// Displays an image widget with a color tint.
	/// </summary>
	public void ImageColor(nk_image img, NkColor color) {
		Nuklear.nk_image_color(_ctx, img, color);
	}

	#endregion

	#region Button

	/// <summary>
	/// Displays an image button and returns whether it was clicked.
	/// </summary>
	public bool ButtonImage(nk_image img) {
		return Nuklear.nk_button_image(_ctx, img) != 0;
	}

	/// <summary>
	/// Displays a button with a symbol and label, and returns whether it was clicked.
	/// </summary>
	public bool ButtonSymbolLabel(nk_symbol_type symbol, string label, NkTextAlign align) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_button_symbol_label(_ctx, symbol, p, (uint)align) != 0;
	}

	/// <summary>
	/// Displays a button with an image and label, and returns whether it was clicked.
	/// </summary>
	public bool ButtonImageLabel(nk_image img, string label, NkTextAlign align) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_button_image_label(_ctx, img, p, (uint)align) != 0;
	}

	/// <summary>
	/// Sets the default button behavior (default click vs repeater).
	/// </summary>
	public void ButtonSetBehavior(nk_button_behavior behavior) {
		Nuklear.nk_button_set_behavior(_ctx, behavior);
	}

	#endregion

	#region Check/Radio

	/// <summary>
	/// Displays a flag-based checkbox and returns the updated flags value.
	/// The checkbox is checked if (flags &amp; value) == value.
	/// </summary>
	public uint CheckFlagsLabel(string label, uint flags, uint value) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_check_flags_label(_ctx, p, flags, value);
	}

	/// <summary>
	/// Displays a flag-based checkbox that mutates flags by reference.
	/// Returns true if the value was toggled.
	/// </summary>
	public bool CheckboxFlagsLabel(string label, ref uint flags, uint value) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		bool toggled;
		fixed (byte* p = utf8)
		fixed (uint* pFlags = &flags)
			toggled = Nuklear.nk_checkbox_flags_label(_ctx, p, pFlags, value) != 0;
		return toggled;
	}

	/// <summary>
	/// Displays a radio button that mutates the active state by reference.
	/// Returns true if the radio button was toggled this frame.
	/// </summary>
	public bool RadioLabel(string label, ref bool active) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		int val = active ? 1 : 0;
		bool toggled;
		fixed (byte* p = utf8)
			toggled = Nuklear.nk_radio_label(_ctx, p, &val) != 0;
		active = val != 0;
		return toggled;
	}

	#endregion

	#region Selectable

	/// <summary>
	/// Displays a selectable label that mutates its selection state by reference.
	/// Returns true if the selection state changed.
	/// </summary>
	public bool SelectableLabel(string label, NkTextAlign align, ref bool selected) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		int val = selected ? 1 : 0;
		bool changed;
		fixed (byte* p = utf8)
			changed = Nuklear.nk_selectable_label(_ctx, p, (uint)align, &val) != 0;
		selected = val != 0;
		return changed;
	}

	/// <summary>
	/// Displays a selectable label with an image that mutates its selection state by reference.
	/// </summary>
	public bool SelectableImageLabel(nk_image img, string label, NkTextAlign align, ref bool selected) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		int val = selected ? 1 : 0;
		bool changed;
		fixed (byte* p = utf8)
			changed = Nuklear.nk_selectable_image_label(_ctx, img, p, (uint)align, &val) != 0;
		selected = val != 0;
		return changed;
	}

	/// <summary>
	/// Displays a selectable label with a symbol that mutates its selection state by reference.
	/// </summary>
	public bool SelectableSymbolLabel(nk_symbol_type symbol, string label, NkTextAlign align, ref bool selected) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		int val = selected ? 1 : 0;
		bool changed;
		fixed (byte* p = utf8)
			changed = Nuklear.nk_selectable_symbol_label(_ctx, symbol, p, (uint)align, &val) != 0;
		selected = val != 0;
		return changed;
	}

	/// <summary>
	/// Displays a selectable label with an image and returns the updated selection state.
	/// </summary>
	public bool SelectImageLabel(nk_image img, string label, NkTextAlign align, bool selected) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_select_image_label(_ctx, img, p, (uint)align, selected ? 1 : 0) != 0;
	}

	/// <summary>
	/// Displays a selectable label with a symbol and returns the updated selection state.
	/// </summary>
	public bool SelectSymbolLabel(nk_symbol_type symbol, string label, NkTextAlign align, bool selected) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
		NkStringHelper.GetUtf8(label, utf8);
		fixed (byte* p = utf8)
			return Nuklear.nk_select_symbol_label(_ctx, symbol, p, (uint)align, selected ? 1 : 0) != 0;
	}

	#endregion

	#region Value Display

	/// <summary>
	/// Displays a labeled boolean value.
	/// </summary>
	public void ValueBool(string prefix, bool value) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(prefix)];
		NkStringHelper.GetUtf8(prefix, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_value_bool(_ctx, p, value ? 1 : 0);
	}

	/// <summary>
	/// Displays a labeled integer value.
	/// </summary>
	public void ValueInt(string prefix, int value) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(prefix)];
		NkStringHelper.GetUtf8(prefix, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_value_int(_ctx, p, value);
	}

	/// <summary>
	/// Displays a labeled unsigned integer value.
	/// </summary>
	public void ValueUInt(string prefix, uint value) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(prefix)];
		NkStringHelper.GetUtf8(prefix, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_value_uint(_ctx, p, value);
	}

	/// <summary>
	/// Displays a labeled floating-point value.
	/// </summary>
	public void ValueFloat(string prefix, float value) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(prefix)];
		NkStringHelper.GetUtf8(prefix, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_value_float(_ctx, p, value);
	}

	/// <summary>
	/// Displays a labeled color value as byte components (0-255).
	/// </summary>
	public void ValueColorByte(string prefix, NkColor color) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(prefix)];
		NkStringHelper.GetUtf8(prefix, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_value_color_byte(_ctx, p, color);
	}

	/// <summary>
	/// Displays a labeled color value as float components (0.0-1.0).
	/// </summary>
	public void ValueColorFloat(string prefix, NkColor color) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(prefix)];
		NkStringHelper.GetUtf8(prefix, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_value_color_float(_ctx, p, color);
	}

	/// <summary>
	/// Displays a labeled color value as a hex string.
	/// </summary>
	public void ValueColorHex(string prefix, NkColor color) {
		Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(prefix)];
		NkStringHelper.GetUtf8(prefix, utf8);
		fixed (byte* p = utf8)
			Nuklear.nk_value_color_hex(_ctx, p, color);
	}

	#endregion

	#region Edit

	/// <summary>
	/// Sets keyboard focus to the current edit widget with the specified flags.
	/// </summary>
	public void EditFocus(NkEditFlags flags) {
		Nuklear.nk_edit_focus(_ctx, (uint)flags);
	}

	/// <summary>
	/// Removes keyboard focus from the current edit widget.
	/// </summary>
	public void EditUnfocus() {
		Nuklear.nk_edit_unfocus(_ctx);
	}

	#endregion
}
