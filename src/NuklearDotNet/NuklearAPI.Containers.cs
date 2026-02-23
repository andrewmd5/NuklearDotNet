using System;
using System.Runtime.CompilerServices;

namespace NuklearDotNet {
	public static unsafe partial class NuklearAPI {
		#region Group

		/// <summary>
		/// Displays a scrollable group using a scroll state struct.
		/// Invokes the content callback if the group is visible.
		/// </summary>
		public static bool GroupScrolled(ref nk_scroll scroll, string title, NkPanelFlags flags, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(title)];
			NkStringHelper.GetUtf8(title, utf8);
			fixed (nk_scroll* pScroll = &scroll)
			fixed (byte* p = utf8) {
				if (Nuklear.nk_group_scrolled_begin(Ctx, pScroll, p, (uint)flags) != 0) {
					content();
					Nuklear.nk_group_scrolled_end(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the scroll offsets of the named group.
		/// </summary>
		public static void GroupGetScroll(string id, out uint offsetX, out uint offsetY) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(id)];
			NkStringHelper.GetUtf8(id, utf8);
			uint x, y;
			fixed (byte* p = utf8)
				Nuklear.nk_group_get_scroll(Ctx, p, &x, &y);
			offsetX = x;
			offsetY = y;
		}

		/// <summary>
		/// Sets the scroll offsets of the named group.
		/// </summary>
		public static void GroupSetScroll(string id, uint offsetX, uint offsetY) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(id)];
			NkStringHelper.GetUtf8(id, utf8);
			fixed (byte* p = utf8)
				Nuklear.nk_group_set_scroll(Ctx, p, offsetX, offsetY);
		}

		/// <summary>
		/// Displays a virtualized list view for efficiently rendering large lists.
		/// The callback receives the list view state with begin/end indices.
		/// </summary>
		public static bool ListView(string id, NkPanelFlags flags, NkListViewConfig config, Action<nk_list_view> content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(id)];
			NkStringHelper.GetUtf8(id, utf8);
			nk_list_view view;
			fixed (byte* p = utf8) {
				if (Nuklear.nk_list_view_begin(Ctx, &view, p, (uint)flags, config.RowHeight, config.RowCount) != 0) {
					content(view);
					Nuklear.nk_list_view_end(&view);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Displays a scrollable group using explicit scroll offset values.
		/// Invokes the content callback if the group is visible.
		/// </summary>
		public static bool GroupScrolledOffset(ref uint offsetX, ref uint offsetY, string title, NkPanelFlags flags, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(title)];
			NkStringHelper.GetUtf8(title, utf8);
			fixed (uint* px = &offsetX)
			fixed (uint* py = &offsetY)
			fixed (byte* p = utf8) {
				if (Nuklear.nk_group_scrolled_offset_begin(Ctx, px, py, p, (uint)flags) != 0) {
					content();
					Nuklear.nk_group_scrolled_end(Ctx);
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Tree

		/// <summary>
		/// Displays a collapsible tree node with an image, invoking the content callback if expanded.
		/// Uses caller file path and line number for unique hash generation.
		/// </summary>
		public static bool TreeImagePush(nk_tree_type type, nk_image img, string title, nk_collapse_states initialState, Action content,
			[CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
			int titleLen = NkStringHelper.GetUtf8ByteCount(title);
			Span<byte> titleUtf8 = titleLen <= 512 ? stackalloc byte[titleLen] : new byte[titleLen];
			NkStringHelper.GetUtf8(title, titleUtf8);

			int hashLen = NkStringHelper.GetUtf8ByteCount(file);
			Span<byte> hashUtf8 = hashLen <= 512 ? stackalloc byte[hashLen] : new byte[hashLen];
			NkStringHelper.GetUtf8(file, hashUtf8);

			fixed (byte* pTitle = titleUtf8)
			fixed (byte* pHash = hashUtf8) {
				if (Nuklear.nk_tree_image_push_hashed(Ctx, type, img, pTitle, initialState, pHash, hashLen, line) != 0) {
					content();
					Nuklear.nk_tree_pop(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Displays a collapsible tree node with explicit state management.
		/// The collapse state is stored in the provided variable rather than internally by hash.
		/// </summary>
		public static bool TreeStatePush(nk_tree_type type, string title, ref nk_collapse_states state, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(title)];
			NkStringHelper.GetUtf8(title, utf8);
			fixed (byte* p = utf8)
			fixed (nk_collapse_states* pState = &state) {
				if (Nuklear.nk_tree_state_push(Ctx, type, p, pState) != 0) {
					content();
					Nuklear.nk_tree_state_pop(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Displays a collapsible tree node with an image and explicit state management.
		/// </summary>
		public static bool TreeStateImagePush(nk_tree_type type, nk_image img, string title, ref nk_collapse_states state, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(title)];
			NkStringHelper.GetUtf8(title, utf8);
			fixed (byte* p = utf8)
			fixed (nk_collapse_states* pState = &state) {
				if (Nuklear.nk_tree_state_image_push(Ctx, type, img, p, pState) != 0) {
					content();
					Nuklear.nk_tree_state_pop(Ctx);
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Combo

		/// <summary>
		/// Begins a combo box displaying a color, invokes the content callback if open, then ends it.
		/// </summary>
		public static bool ComboBeginColor(NkColor color, nk_vec2 size, Action content) {
			if (Nuklear.nk_combo_begin_color(Ctx, color, size) != 0) {
				content();
				Nuklear.nk_combo_end(Ctx);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Begins a combo box displaying a symbol, invokes the content callback if open, then ends it.
		/// </summary>
		public static bool ComboBeginSymbol(nk_symbol_type symbol, nk_vec2 size, Action content) {
			if (Nuklear.nk_combo_begin_symbol(Ctx, symbol, size) != 0) {
				content();
				Nuklear.nk_combo_end(Ctx);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Begins a combo box displaying an image, invokes the content callback if open, then ends it.
		/// </summary>
		public static bool ComboBeginImage(nk_image img, nk_vec2 size, Action content) {
			if (Nuklear.nk_combo_begin_image(Ctx, img, size) != 0) {
				content();
				Nuklear.nk_combo_end(Ctx);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Begins a combo box displaying a symbol and label, invokes the content callback if open, then ends it.
		/// </summary>
		public static bool ComboBeginSymbolLabel(string label, nk_symbol_type symbol, nk_vec2 size, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8) {
				if (Nuklear.nk_combo_begin_symbol_label(Ctx, p, symbol, size) != 0) {
					content();
					Nuklear.nk_combo_end(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Begins a combo box displaying an image and label, invokes the content callback if open, then ends it.
		/// </summary>
		public static bool ComboBeginImageLabel(string label, nk_image img, nk_vec2 size, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8) {
				if (Nuklear.nk_combo_begin_image_label(Ctx, p, img, size) != 0) {
					content();
					Nuklear.nk_combo_end(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Displays a labeled item inside a combo box and returns whether it was selected.
		/// </summary>
		public static bool ComboItemLabel(string label, NkTextAlign align) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_combo_item_label(Ctx, p, (uint)align) != 0;
		}

		/// <summary>
		/// Displays a combo item with an image and label, returning whether it was selected.
		/// </summary>
		public static bool ComboItemImageLabel(nk_image img, string label, NkTextAlign align) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_combo_item_image_label(Ctx, img, p, (uint)align) != 0;
		}

		/// <summary>
		/// Displays a combo item with a symbol and label, returning whether it was selected.
		/// </summary>
		public static bool ComboItemSymbolLabel(nk_symbol_type symbol, string label, NkTextAlign align) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_combo_item_symbol_label(Ctx, symbol, p, (uint)align) != 0;
		}

		/// <summary>
		/// Closes the current combo box.
		/// </summary>
		public static void ComboClose() {
			Nuklear.nk_combo_close(Ctx);
		}

		#endregion

		#region Contextual

		/// <summary>
		/// Displays a contextual menu item with an image and label, returning whether it was clicked.
		/// </summary>
		public static bool ContextualItemImageLabel(nk_image img, string label, NkTextAlign align) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_contextual_item_image_label(Ctx, img, p, (uint)align) != 0;
		}

		/// <summary>
		/// Displays a contextual menu item with a symbol and label, returning whether it was clicked.
		/// </summary>
		public static bool ContextualItemSymbolLabel(nk_symbol_type symbol, string label, NkTextAlign align) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_contextual_item_symbol_label(Ctx, symbol, p, (uint)align) != 0;
		}

		/// <summary>
		/// Closes the current contextual menu.
		/// </summary>
		public static void ContextualClose() {
			Nuklear.nk_contextual_close(Ctx);
		}

		#endregion

		#region Chart

		/// <summary>
		/// Begins a colored line chart, invokes the callback to push values, then ends the chart.
		/// </summary>
		public static bool ChartLinesColored(in NkChartSlotConfig config, Action<int> pushValues) {
			return ChartColoredImpl(nk_chart_type.NK_CHART_LINES, config, pushValues);
		}

		/// <summary>
		/// Begins a colored column chart, invokes the callback to push values, then ends the chart.
		/// </summary>
		public static bool ChartColumnsColored(in NkChartSlotConfig config, Action<int> pushValues) {
			return ChartColoredImpl(nk_chart_type.NK_CHART_COLUMN, config, pushValues);
		}

		static bool ChartColoredImpl(nk_chart_type type, in NkChartSlotConfig config, Action<int> pushValues) {
			if (Nuklear.nk_chart_begin_colored(Ctx, type, config.Color, config.ActiveColor, config.Count, config.Min, config.Max) != 0) {
				pushValues(config.Count);
				Nuklear.nk_chart_end(Ctx);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Adds a chart slot to a multi-slot chart.
		/// </summary>
		public static void ChartAddSlot(nk_chart_type type, int count, float min, float max) {
			Nuklear.nk_chart_add_slot(Ctx, type, count, min, max);
		}

		/// <summary>
		/// Adds a colored chart slot to a multi-slot chart.
		/// </summary>
		public static void ChartAddSlotColored(nk_chart_type type, in NkChartSlotConfig config) {
			Nuklear.nk_chart_add_slot_colored(Ctx, type, config.Color, config.ActiveColor, config.Count, config.Min, config.Max);
		}

		/// <summary>
		/// Pushes a value to a specific chart slot.
		/// </summary>
		public static void ChartPushSlot(float value, int slot) {
			Nuklear.nk_chart_push_slot(Ctx, value, slot);
		}

		#endregion

		#region Menu

		/// <summary>
		/// Begins a menu with an image header, invokes the content callback if open, then ends it.
		/// </summary>
		public static bool MenuBeginImage(string label, nk_image img, nk_vec2 size, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8) {
				if (Nuklear.nk_menu_begin_image(Ctx, p, img, size) != 0) {
					content();
					Nuklear.nk_menu_end(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Begins a menu with a symbol header, invokes the content callback if open, then ends it.
		/// </summary>
		public static bool MenuBeginSymbol(string label, nk_symbol_type symbol, nk_vec2 size, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8) {
				if (Nuklear.nk_menu_begin_symbol(Ctx, p, symbol, size) != 0) {
					content();
					Nuklear.nk_menu_end(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Begins a menu with an image and label header with alignment, invokes the content callback if open, then ends it.
		/// </summary>
		public static bool MenuBeginImageLabel(string label, NkTextAlign align, nk_image img, nk_vec2 size, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8) {
				if (Nuklear.nk_menu_begin_image_label(Ctx, p, (uint)align, img, size) != 0) {
					content();
					Nuklear.nk_menu_end(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Begins a menu with a symbol and label header with alignment, invokes the content callback if open, then ends it.
		/// </summary>
		public static bool MenuBeginSymbolLabel(string label, NkTextAlign align, nk_symbol_type symbol, nk_vec2 size, Action content) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8) {
				if (Nuklear.nk_menu_begin_symbol_label(Ctx, p, (uint)align, symbol, size) != 0) {
					content();
					Nuklear.nk_menu_end(Ctx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Displays a menu item with an image and label, returning whether it was clicked.
		/// </summary>
		public static bool MenuItemImageLabel(nk_image img, string label, NkTextAlign align) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_menu_item_image_label(Ctx, img, p, (uint)align) != 0;
		}

		/// <summary>
		/// Displays a menu item with a symbol and label, returning whether it was clicked.
		/// </summary>
		public static bool MenuItemSymbolLabel(nk_symbol_type symbol, string label, NkTextAlign align) {
			Span<byte> utf8 = stackalloc byte[NkStringHelper.GetUtf8ByteCount(label)];
			NkStringHelper.GetUtf8(label, utf8);
			fixed (byte* p = utf8)
				return Nuklear.nk_menu_item_symbol_label(Ctx, symbol, p, (uint)align) != 0;
		}

		/// <summary>
		/// Closes the current menu.
		/// </summary>
		public static void MenuClose() {
			Nuklear.nk_menu_close(Ctx);
		}

		#endregion

		#region Popup

		/// <summary>
		/// Gets the scroll offsets of the current popup.
		/// </summary>
		public static void PopupGetScroll(out uint offsetX, out uint offsetY) {
			uint x, y;
			Nuklear.nk_popup_get_scroll(Ctx, &x, &y);
			offsetX = x;
			offsetY = y;
		}

		/// <summary>
		/// Sets the scroll offsets of the current popup.
		/// </summary>
		public static void PopupSetScroll(uint offsetX, uint offsetY) {
			Nuklear.nk_popup_set_scroll(Ctx, offsetX, offsetY);
		}

		#endregion
	}
}
