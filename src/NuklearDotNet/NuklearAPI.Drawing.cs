namespace NuklearDotNet {
	public static unsafe partial class NuklearAPI {
		/// <summary>
		/// Draws a line on the current window's canvas.
		/// </summary>
		public static void StrokeLine(nk_vec2 a, nk_vec2 b, float thickness, NkColor color) {
			nk_command_buffer* canvas = Nuklear.nk_window_get_canvas(Ctx);
			Nuklear.nk_stroke_line(canvas, a.x, a.y, b.x, b.y, thickness, color);
		}

		/// <summary>
		/// Draws a stroked rectangle on the current window's canvas.
		/// </summary>
		public static void StrokeRect(NkRect r, float rounding, float thickness, NkColor color) {
			nk_command_buffer* canvas = Nuklear.nk_window_get_canvas(Ctx);
			Nuklear.nk_stroke_rect(canvas, r, rounding, thickness, color);
		}

		/// <summary>
		/// Draws a stroked circle on the current window's canvas.
		/// </summary>
		public static void StrokeCircle(NkRect r, float thickness, NkColor color) {
			nk_command_buffer* canvas = Nuklear.nk_window_get_canvas(Ctx);
			Nuklear.nk_stroke_circle(canvas, r, thickness, color);
		}

		/// <summary>
		/// Draws a stroked triangle on the current window's canvas with a fixed line thickness of 1.0.
		/// </summary>
		public static void StrokeTriangle(nk_vec2 a, nk_vec2 b, nk_vec2 c, NkColor color) {
			nk_command_buffer* canvas = Nuklear.nk_window_get_canvas(Ctx);
			Nuklear.nk_stroke_triangle(canvas, a.x, a.y, b.x, b.y, c.x, c.y, 1.0f, color);
		}

		/// <summary>
		/// Draws a filled rectangle on the current window's canvas.
		/// </summary>
		public static void FillRect(NkRect r, float rounding, NkColor color) {
			nk_command_buffer* canvas = Nuklear.nk_window_get_canvas(Ctx);
			Nuklear.nk_fill_rect(canvas, r, rounding, color);
		}

		/// <summary>
		/// Draws a filled circle on the current window's canvas.
		/// </summary>
		public static void FillCircle(NkRect r, NkColor color) {
			nk_command_buffer* canvas = Nuklear.nk_window_get_canvas(Ctx);
			Nuklear.nk_fill_circle(canvas, r, color);
		}

		/// <summary>
		/// Draws a filled triangle on the current window's canvas.
		/// </summary>
		public static void FillTriangle(nk_vec2 a, nk_vec2 b, nk_vec2 c, NkColor color) {
			nk_command_buffer* canvas = Nuklear.nk_window_get_canvas(Ctx);
			Nuklear.nk_fill_triangle(canvas, a.x, a.y, b.x, b.y, c.x, c.y, color);
		}

		/// <summary>
		/// Draws an image on the current window's canvas.
		/// </summary>
		public static void DrawImage(NkRect r, nk_image img, NkColor color) {
			nk_command_buffer* canvas = Nuklear.nk_window_get_canvas(Ctx);
			Nuklear.nk_draw_image(canvas, r, &img, color);
		}

		/// <summary>
		/// Sets the scissor clipping rectangle on the current window's canvas.
		/// </summary>
		public static void PushScissor(NkRect r) {
			nk_command_buffer* canvas = Nuklear.nk_window_get_canvas(Ctx);
			Nuklear.nk_push_scissor(canvas, r);
		}

		/// <summary>
		/// Draws a nine-slice image on the current window's canvas.
		/// </summary>
		public static void DrawNineSlice(NkRect r, nk_nine_slice slice, NkColor color) {
			nk_command_buffer* canvas = Nuklear.nk_window_get_canvas(Ctx);
			Nuklear.nk_draw_nine_slice(canvas, r, &slice, color);
		}
	}
}
