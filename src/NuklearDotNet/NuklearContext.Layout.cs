using System;

namespace NuklearDotNet;

public sealed unsafe partial class NuklearContext {
	/// <summary>
	/// Sets the minimum row height for subsequent layout rows.
	/// </summary>
	public void LayoutSetMinRowHeight(float height) {
		Nuklear.nk_layout_set_min_row_height(_ctx, height);
	}

	/// <summary>
	/// Resets the minimum row height to the default (font height).
	/// </summary>
	public void LayoutResetMinRowHeight() {
		Nuklear.nk_layout_reset_min_row_height(_ctx);
	}

	/// <summary>
	/// Returns the bounding rectangle of the next widget to be allocated.
	/// </summary>
	public NkRect LayoutWidgetBounds() {
		return Nuklear.nk_layout_widget_bounds(_ctx);
	}

	/// <summary>
	/// Converts a pixel width to a layout ratio for the current row.
	/// </summary>
	public float LayoutRatioFromPixel(float pixelWidth) {
		return Nuklear.nk_layout_ratio_from_pixel(_ctx, pixelWidth);
	}

	/// <summary>
	/// Defines a row layout with column ratios or fixed widths provided as a span.
	/// </summary>
	public void LayoutRow(nk_layout_format format, float height, ReadOnlySpan<float> ratios) {
		fixed (float* p = ratios)
			Nuklear.nk_layout_row(_ctx, format, height, ratios.Length, p);
	}

	/// <summary>
	/// Begins a custom row layout where each column width is pushed individually.
	/// </summary>
	public void LayoutRowBegin(nk_layout_format format, float rowHeight, int cols) {
		Nuklear.nk_layout_row_begin(_ctx, format, rowHeight, cols);
	}

	/// <summary>
	/// Pushes the width of the next column in a custom row layout.
	/// </summary>
	public void LayoutRowPush(float value) {
		Nuklear.nk_layout_row_push(_ctx, value);
	}

	/// <summary>
	/// Ends a custom row layout started with LayoutRowBegin.
	/// </summary>
	public void LayoutRowEnd() {
		Nuklear.nk_layout_row_end(_ctx);
	}

	/// <summary>
	/// Defines a template-based row layout. Use the callback to push dynamic, variable, and static columns.
	/// </summary>
	public void LayoutRowTemplate(float height, Action content) {
		Nuklear.nk_layout_row_template_begin(_ctx, height);
		content();
		Nuklear.nk_layout_row_template_end(_ctx);
	}

	/// <summary>
	/// Pushes a dynamically-sized column in a template row layout.
	/// </summary>
	public void LayoutRowTemplatePushDynamic() {
		Nuklear.nk_layout_row_template_push_dynamic(_ctx);
	}

	/// <summary>
	/// Pushes a variable-sized column with a minimum width in a template row layout.
	/// </summary>
	public void LayoutRowTemplatePushVariable(float minWidth) {
		Nuklear.nk_layout_row_template_push_variable(_ctx, minWidth);
	}

	/// <summary>
	/// Pushes a fixed-width column in a template row layout.
	/// </summary>
	public void LayoutRowTemplatePushStatic(float width) {
		Nuklear.nk_layout_row_template_push_static(_ctx, width);
	}

	/// <summary>
	/// Defines a space-based layout for absolute or relative widget positioning.
	/// Use the callback to push widget bounds with LayoutSpacePush.
	/// </summary>
	public void LayoutSpace(nk_layout_format format, float height, int widgetCount, Action content) {
		Nuklear.nk_layout_space_begin(_ctx, format, height, widgetCount);
		content();
		Nuklear.nk_layout_space_end(_ctx);
	}

	/// <summary>
	/// Pushes the bounding rectangle for the next widget in a space layout.
	/// </summary>
	public void LayoutSpacePush(NkRect rect) {
		Nuklear.nk_layout_space_push(_ctx, rect);
	}

	/// <summary>
	/// Returns the bounding rectangle of the current layout space.
	/// </summary>
	public NkRect LayoutSpaceBounds() {
		return Nuklear.nk_layout_space_bounds(_ctx);
	}

	/// <summary>
	/// Converts a local-space position to screen-space within a space layout.
	/// </summary>
	public nk_vec2 LayoutSpaceToScreen(nk_vec2 localPos) {
		return Nuklear.nk_layout_space_to_screen(_ctx, localPos);
	}

	/// <summary>
	/// Converts a screen-space position to local-space within a space layout.
	/// </summary>
	public nk_vec2 LayoutSpaceToLocal(nk_vec2 screenPos) {
		return Nuklear.nk_layout_space_to_local(_ctx, screenPos);
	}

	/// <summary>
	/// Converts a local-space rectangle to screen-space within a space layout.
	/// </summary>
	public NkRect LayoutSpaceRectToScreen(NkRect localRect) {
		return Nuklear.nk_layout_space_rect_to_screen(_ctx, localRect);
	}

	/// <summary>
	/// Converts a screen-space rectangle to local-space within a space layout.
	/// </summary>
	public NkRect LayoutSpaceRectToLocal(NkRect screenRect) {
		return Nuklear.nk_layout_space_rect_to_local(_ctx, screenRect);
	}
}
