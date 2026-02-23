using System;

namespace NuklearDotNet;

public sealed unsafe partial class NuklearContext {
	/// <summary>
	/// Resets all style properties to the built-in default theme.
	/// </summary>
	public void StyleDefault() {
		Nuklear.nk_style_default(_ctx);
	}

	/// <summary>
	/// Sets the style theme from a color table.
	/// The span must contain exactly 28 colors (NK_COLOR_COUNT).
	/// </summary>
	public void StyleFromTable(ReadOnlySpan<NkColor> colors) {
		if (colors.Length < (int)nk_style_colors.NK_COLOR_COUNT)
			throw new ArgumentException(
				$"Color table must have at least {(int)nk_style_colors.NK_COLOR_COUNT} entries",
				nameof(colors));

		fixed (NkColor* p = colors)
			Nuklear.nk_style_from_table(_ctx, p);
	}

	/// <summary>
	/// Sets the active cursor style.
	/// </summary>
	public void StyleSetCursor(nk_style_cursor cursor) {
		Nuklear.nk_style_set_cursor(_ctx, cursor);
	}

	/// <summary>
	/// Makes the cursor visible.
	/// </summary>
	public void StyleShowCursor() {
		Nuklear.nk_style_show_cursor(_ctx);
	}

	/// <summary>
	/// Hides the cursor.
	/// </summary>
	public void StyleHideCursor() {
		Nuklear.nk_style_hide_cursor(_ctx);
	}
}
