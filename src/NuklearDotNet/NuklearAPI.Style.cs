using System;

namespace NuklearDotNet {
	public static unsafe partial class NuklearAPI {
		/// <summary>
		/// Resets all style properties to the built-in default theme.
		/// </summary>
		public static void StyleDefault() {
			Nuklear.nk_style_default(Ctx);
		}

		/// <summary>
		/// Sets the style theme from a color table.
		/// The span must contain exactly 28 colors (NK_COLOR_COUNT).
		/// </summary>
		public static void StyleFromTable(ReadOnlySpan<NkColor> colors) {
			if (colors.Length < (int)nk_style_colors.NK_COLOR_COUNT)
				throw new ArgumentException(
					$"Color table must have at least {(int)nk_style_colors.NK_COLOR_COUNT} entries",
					nameof(colors));

			fixed (NkColor* p = colors)
				Nuklear.nk_style_from_table(Ctx, p);
		}

		/// <summary>
		/// Sets the active cursor style.
		/// </summary>
		public static void StyleSetCursor(nk_style_cursor cursor) {
			Nuklear.nk_style_set_cursor(Ctx, cursor);
		}

		/// <summary>
		/// Makes the cursor visible.
		/// </summary>
		public static void StyleShowCursor() {
			Nuklear.nk_style_show_cursor(Ctx);
		}

		/// <summary>
		/// Hides the cursor.
		/// </summary>
		public static void StyleHideCursor() {
			Nuklear.nk_style_hide_cursor(Ctx);
		}
	}
}
