using System;

namespace NuklearDotNet {
	public static unsafe partial class NuklearAPI {
		/// <summary>
		/// Sets the minimum row height for subsequent layout rows.
		/// </summary>
		public static void LayoutSetMinRowHeight(float height) {
			Nuklear.nk_layout_set_min_row_height(Ctx, height);
		}

		/// <summary>
		/// Resets the minimum row height to the default (font height).
		/// </summary>
		public static void LayoutResetMinRowHeight() {
			Nuklear.nk_layout_reset_min_row_height(Ctx);
		}

		/// <summary>
		/// Returns the bounding rectangle of the next widget to be allocated.
		/// </summary>
		public static NkRect LayoutWidgetBounds() {
			return Nuklear.nk_layout_widget_bounds(Ctx);
		}

		/// <summary>
		/// Converts a pixel width to a layout ratio for the current row.
		/// </summary>
		public static float LayoutRatioFromPixel(float pixelWidth) {
			return Nuklear.nk_layout_ratio_from_pixel(Ctx, pixelWidth);
		}

		/// <summary>
		/// Defines a row layout with column ratios or fixed widths provided as a span.
		/// </summary>
		public static void LayoutRow(nk_layout_format format, float height, ReadOnlySpan<float> ratios) {
			fixed (float* p = ratios)
				Nuklear.nk_layout_row(Ctx, format, height, ratios.Length, p);
		}

		/// <summary>
		/// Begins a custom row layout where each column width is pushed individually.
		/// </summary>
		public static void LayoutRowBegin(nk_layout_format format, float rowHeight, int cols) {
			Nuklear.nk_layout_row_begin(Ctx, format, rowHeight, cols);
		}

		/// <summary>
		/// Pushes the width of the next column in a custom row layout.
		/// </summary>
		public static void LayoutRowPush(float value) {
			Nuklear.nk_layout_row_push(Ctx, value);
		}

		/// <summary>
		/// Ends a custom row layout started with LayoutRowBegin.
		/// </summary>
		public static void LayoutRowEnd() {
			Nuklear.nk_layout_row_end(Ctx);
		}

		/// <summary>
		/// Defines a template-based row layout. Use the callback to push dynamic, variable, and static columns.
		/// </summary>
		public static void LayoutRowTemplate(float height, Action content) {
			Nuklear.nk_layout_row_template_begin(Ctx, height);
			content();
			Nuklear.nk_layout_row_template_end(Ctx);
		}

		/// <summary>
		/// Pushes a dynamically-sized column in a template row layout.
		/// </summary>
		public static void LayoutRowTemplatePushDynamic() {
			Nuklear.nk_layout_row_template_push_dynamic(Ctx);
		}

		/// <summary>
		/// Pushes a variable-sized column with a minimum width in a template row layout.
		/// </summary>
		public static void LayoutRowTemplatePushVariable(float minWidth) {
			Nuklear.nk_layout_row_template_push_variable(Ctx, minWidth);
		}

		/// <summary>
		/// Pushes a fixed-width column in a template row layout.
		/// </summary>
		public static void LayoutRowTemplatePushStatic(float width) {
			Nuklear.nk_layout_row_template_push_static(Ctx, width);
		}

		/// <summary>
		/// Defines a space-based layout for absolute or relative widget positioning.
		/// Use the callback to push widget bounds with LayoutSpacePush.
		/// </summary>
		public static void LayoutSpace(nk_layout_format format, float height, int widgetCount, Action content) {
			Nuklear.nk_layout_space_begin(Ctx, format, height, widgetCount);
			content();
			Nuklear.nk_layout_space_end(Ctx);
		}

		/// <summary>
		/// Pushes the bounding rectangle for the next widget in a space layout.
		/// </summary>
		public static void LayoutSpacePush(NkRect rect) {
			Nuklear.nk_layout_space_push(Ctx, rect);
		}

		/// <summary>
		/// Returns the bounding rectangle of the current layout space.
		/// </summary>
		public static NkRect LayoutSpaceBounds() {
			return Nuklear.nk_layout_space_bounds(Ctx);
		}

		/// <summary>
		/// Converts a local-space position to screen-space within a space layout.
		/// </summary>
		public static nk_vec2 LayoutSpaceToScreen(nk_vec2 localPos) {
			return Nuklear.nk_layout_space_to_screen(Ctx, localPos);
		}

		/// <summary>
		/// Converts a screen-space position to local-space within a space layout.
		/// </summary>
		public static nk_vec2 LayoutSpaceToLocal(nk_vec2 screenPos) {
			return Nuklear.nk_layout_space_to_local(Ctx, screenPos);
		}

		/// <summary>
		/// Converts a local-space rectangle to screen-space within a space layout.
		/// </summary>
		public static NkRect LayoutSpaceRectToScreen(NkRect localRect) {
			return Nuklear.nk_layout_space_rect_to_screen(Ctx, localRect);
		}

		/// <summary>
		/// Converts a screen-space rectangle to local-space within a space layout.
		/// </summary>
		public static NkRect LayoutSpaceRectToLocal(NkRect screenRect) {
			return Nuklear.nk_layout_space_rect_to_local(Ctx, screenRect);
		}
	}
}
