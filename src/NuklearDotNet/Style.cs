using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NuklearDotNet {
	public enum nk_style_item_type {
		NK_STYLE_ITEM_COLOR,
		NK_STYLE_ITEM_IMAGE,
		NK_STYLE_ITEM_NINE_SLICE
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct nk_style_item_data {
		[FieldOffset(0)]
		public NkColor color;

		[FieldOffset(0)]
		public nk_image image;

		[FieldOffset(0)]
		public nk_nine_slice slice;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_style_item {
		public nk_style_item_type type;
		public nk_style_item_data data;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_style_text {
		public NkColor color;
		public nk_vec2 padding;
		public float color_factor;
		public float disabled_factor;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_style_button {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item active;
		public NkColor border_color;
		public float color_factor_background;

		public NkColor text_background;
		public NkColor text_normal;
		public NkColor text_hover;
		public NkColor text_active;
		public uint text_alignment_nkflags;
		public float color_factor_text;

		public float border;
		public float rounding;
		public nk_vec2 padding;
		public nk_vec2 image_padding;
		public nk_vec2 touch_padding;
		public float disabled_factor;

		public NkHandle userdata;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_begin;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_end;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_style_toggle {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item active;
		public NkColor border_color;

		public nk_style_item cursor_normal;
		public nk_style_item cursor_hover;

		public NkColor text_normal;
		public NkColor text_hover;
		public NkColor text_active;
		public NkColor text_background;
		public uint text_alignment_nkflags;

		public nk_vec2 padding;
		public nk_vec2 touch_padding;
		public float spacing;
		public float border;
		public float color_factor;
		public float disabled_factor;

		public NkHandle userdata;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_begin;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_end;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_style_selectable {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item pressed;

		public nk_style_item normal_active;
		public nk_style_item hover_active;
		public nk_style_item pressed_active;

		public NkColor text_normal;
		public NkColor text_hover;
		public NkColor text_pressed;

		public NkColor text_normal_active;
		public NkColor text_hover_active;
		public NkColor text_pressed_active;
		public NkColor text_background;
		public uint text_alignment_nkflags;

		public float rounding;
		public nk_vec2 padding;
		public nk_vec2 touch_padding;
		public nk_vec2 image_padding;
		public float color_factor;
		public float disabled_factor;

		public NkHandle userdata;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_begin;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_end;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_style_slider {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item active;
		public NkColor border_color;

		public NkColor bar_normal;
		public NkColor bar_hover;
		public NkColor bar_active;
		public NkColor bar_filled;

		public nk_style_item cursor_normal;
		public nk_style_item cursor_hover;
		public nk_style_item cursor_active;

		public float border;
		public float rounding;
		public float bar_height;
		public nk_vec2 padding;
		public nk_vec2 spacing;
		public nk_vec2 cursor_size;
		public float color_factor;
		public float disabled_factor;

		public int show_buttons;
		public nk_style_button inc_button;
		public nk_style_button dec_button;
		public nk_symbol_type inc_symbol;
		public nk_symbol_type dec_symbol;

		public NkHandle userdata;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_begin;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_end;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_style_knob {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item active;
		public NkColor border_color;

		public NkColor knob_normal;
		public NkColor knob_hover;
		public NkColor knob_active;
		public NkColor knob_border_color;

		public NkColor cursor_normal;
		public NkColor cursor_hover;
		public NkColor cursor_active;

		public float border;
		public float knob_border;
		public nk_vec2 padding;
		public nk_vec2 spacing;
		public float cursor_width;
		public float color_factor;
		public float disabled_factor;

		public NkHandle userdata;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_begin;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_end;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_style_progress {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item active;
		public NkColor border_color;

		public nk_style_item cursor_normal;
		public nk_style_item cursor_hover;
		public nk_style_item cursor_active;
		public NkColor cursor_border_color;

		public float rounding;
		public float border;
		public float cursor_border;
		public float cursor_rounding;
		public nk_vec2 padding;
		public float color_factor;
		public float disabled_factor;

		public NkHandle userdata;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_begin;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_end;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_style_scrollbar {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item active;
		public NkColor border_color;

		public nk_style_item cursor_normal;
		public nk_style_item cursor_hover;
		public nk_style_item cursor_active;
		public NkColor cursor_border_color;

		public float border;
		public float rounding;
		public float border_cursor;
		public float rounding_cursor;
		public nk_vec2 padding;
		public float color_factor;
		public float disabled_factor;

		public int show_buttons;
		public nk_style_button inc_button;
		public nk_style_button dec_button;
		public nk_symbol_type inc_symbol;
		public nk_symbol_type dec_symbol;

		public NkHandle userdata;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_begin;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_end;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_style_edit {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item active;
		public NkColor border_color;
		public nk_style_scrollbar scrollbar;

		public NkColor cursor_normal;
		public NkColor cursor_hover;
		public NkColor cursor_text_normal;
		public NkColor cursor_text_hover;

		public NkColor text_normal;
		public NkColor text_hover;
		public NkColor text_active;

		public NkColor selected_normal;
		public NkColor selected_hover;
		public NkColor selected_text_normal;
		public NkColor selected_text_hover;

		public float border;
		public float rounding;
		public float cursor_size;
		public nk_vec2 scrollbar_size;
		public nk_vec2 padding;
		public float row_padding;
		public float color_factor;
		public float disabled_factor;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_style_property {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item active;
		public NkColor border_color;

		public NkColor label_normal;
		public NkColor label_hover;
		public NkColor label_active;

		public nk_symbol_type sym_left;
		public nk_symbol_type sym_right;

		public float border;
		public float rounding;
		public nk_vec2 padding;
		public float color_factor;
		public float disabled_factor;

		public nk_style_edit edit;
		public nk_style_button inc_button;
		public nk_style_button dec_button;

		public NkHandle userdata;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_begin;
		public delegate* unmanaged[Cdecl]<nk_command_buffer*, NkHandle, void> draw_end;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_style_chart {
		public nk_style_item background;
		public NkColor border_color;
		public NkColor selected_color;
		public NkColor color;

		public float border;
		public float rounding;
		public nk_vec2 padding;
		public float color_factor;
		public float disabled_factor;
		public int show_markers;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_style_combo {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item active;
		public NkColor border_color;

		public NkColor label_normal;
		public NkColor label_hover;
		public NkColor label_active;

		public NkColor symbol_normal;
		public NkColor symbol_hover;
		public NkColor symbol_active;

		public nk_style_button button;
		public nk_symbol_type sym_normal;
		public nk_symbol_type sym_hover;
		public nk_symbol_type sym_active;

		public float border;
		public float rounding;
		public nk_vec2 content_padding;
		public nk_vec2 button_padding;
		public nk_vec2 spacing;
		public float color_factor;
		public float disabled_factor;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_style_tab {
		public nk_style_item background;
		public NkColor border_color;
		public NkColor text;

		public nk_style_button tab_maximize_button;
		public nk_style_button tab_minimize_button;
		public nk_style_button node_maximize_button;
		public nk_style_button node_minimize_button;
		public nk_symbol_type sym_minimize;
		public nk_symbol_type sym_maximize;

		public float border;
		public float rounding;
		public float indent;
		public nk_vec2 padding;
		public nk_vec2 spacing;
		public float color_factor;
		public float disabled_factor;
	}

	public enum nk_style_header_align {
		NK_HEADER_LEFT,
		NK_HEADER_RIGHT
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_style_window_header {
		public nk_style_item normal;
		public nk_style_item hover;
		public nk_style_item active;

		public nk_style_button close_button;
		public nk_style_button minimize_button;
		public nk_symbol_type close_symbol;
		public nk_symbol_type minimize_symbol;
		public nk_symbol_type maximize_symbol;

		public NkColor label_normal;
		public NkColor label_hover;
		public NkColor label_active;

		public nk_style_header_align align;
		public nk_vec2 padding;
		public nk_vec2 label_padding;
		public nk_vec2 spacing;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_style_window {
		public nk_style_window_header header;
		public nk_style_item fixed_background;
		public NkColor background;

		public NkColor border_color;
		public NkColor popup_border_color;
		public NkColor combo_border_color;
		public NkColor contextual_border_color;
		public NkColor menu_border_color;
		public NkColor group_border_color;
		public NkColor tooltip_border_color;
		public nk_style_item scaler;

		public float border;
		public float combo_border;
		public float contextual_border;
		public float menu_border;
		public float group_border;
		public float tooltip_border;
		public float popup_border;
		public float min_row_height_padding;

		public float rounding;
		public nk_vec2 spacing;
		public nk_vec2 scrollbar_size;
		public nk_vec2 min_size;

		public nk_vec2 padding;
		public nk_vec2 group_padding;
		public nk_vec2 popup_padding;
		public nk_vec2 combo_padding;
		public nk_vec2 contextual_padding;
		public nk_vec2 menu_padding;
		public nk_vec2 tooltip_padding;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_style {
		public nk_user_font* font;

		public nk_cursor* cursorArrow;
		public nk_cursor* cursorText;
		public nk_cursor* cursorMove;
		public nk_cursor* cursorResizeV;
		public nk_cursor* cursorResizeH;
		public nk_cursor* cursorResizeTLDR;
		public nk_cursor* cursorResizeTRDL;

		public nk_cursor* cursor_active;
		public nk_cursor* cursor_last;
		public int cursor_visible;

		public nk_style_text text;
		public nk_style_button button;
		public nk_style_button contextual_button;
		public nk_style_button menu_button;
		public nk_style_toggle option;
		public nk_style_toggle checkbox;
		public nk_style_selectable selectable;
		public nk_style_slider slider;
		public nk_style_knob knob;
		public nk_style_progress progress;
		public nk_style_property property;
		public nk_style_edit edit;
		public nk_style_chart chart;
		public nk_style_scrollbar scrollh;
		public nk_style_scrollbar scrollv;
		public nk_style_tab tab;
		public nk_style_combo combo;
		public nk_style_window window;
	}

	public enum nk_style_colors {
		NK_COLOR_TEXT,
		NK_COLOR_WINDOW,
		NK_COLOR_HEADER,
		NK_COLOR_BORDER,
		NK_COLOR_BUTTON,
		NK_COLOR_BUTTON_HOVER,
		NK_COLOR_BUTTON_ACTIVE,
		NK_COLOR_TOGGLE,
		NK_COLOR_TOGGLE_HOVER,
		NK_COLOR_TOGGLE_CURSOR,
		NK_COLOR_SELECT,
		NK_COLOR_SELECT_ACTIVE,
		NK_COLOR_SLIDER,
		NK_COLOR_SLIDER_CURSOR,
		NK_COLOR_SLIDER_CURSOR_HOVER,
		NK_COLOR_SLIDER_CURSOR_ACTIVE,
		NK_COLOR_PROPERTY,
		NK_COLOR_EDIT,
		NK_COLOR_EDIT_CURSOR,
		NK_COLOR_COMBO,
		NK_COLOR_CHART,
		NK_COLOR_CHART_COLOR,
		NK_COLOR_CHART_COLOR_HIGHLIGHT,
		NK_COLOR_SCROLLBAR,
		NK_COLOR_SCROLLBAR_CURSOR,
		NK_COLOR_SCROLLBAR_CURSOR_HOVER,
		NK_COLOR_SCROLLBAR_CURSOR_ACTIVE,
		NK_COLOR_TAB_HEADER,
		NK_COLOR_COUNT
	}

	public enum nk_style_cursor {
		NK_CURSOR_ARROW,
		NK_CURSOR_TEXT,
		NK_CURSOR_MOVE,
		NK_CURSOR_RESIZE_VERTICAL,
		NK_CURSOR_RESIZE_HORIZONTAL,
		NK_CURSOR_RESIZE_TOP_LEFT_DOWN_RIGHT,
		NK_CURSOR_RESIZE_TOP_RIGHT_DOWN_LEFT,
		NK_CURSOR_COUNT
	}

	public static unsafe partial class Nuklear {
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_style_default(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_style_from_table(nk_context* ctx, NkColor* color);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_style_load_cursor(nk_context* ctx, nk_style_cursor scur, nk_cursor* cursor);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_style_load_all_cursors(nk_context* ctx, nk_cursor* cursor);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial byte* nk_style_get_color_by_name(nk_style_colors scol);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_style_set_font(nk_context* ctx, nk_user_font* userfont);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_set_cursor(nk_context* ctx, nk_style_cursor scur);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_style_show_cursor(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_style_hide_cursor(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_push_font(nk_context* ctx, nk_user_font* userfont);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_push_float(nk_context* ctx, float* f, float g);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_push_vec2(nk_context* ctx, nk_vec2* a, nk_vec2 b);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_push_style_item(nk_context* ctx, nk_style_item* sitem, nk_style_item sitem2);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_push_flags(nk_context* ctx, uint* a_nkflags, uint b_nkflags);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_push_color(nk_context* ctx, NkColor* a, NkColor b);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_pop_font(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_pop_float(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_pop_vec2(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_pop_style_item(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_pop_flags(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_style_pop_color(nk_context* ctx);

		// Color creation
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgb(int r, int g, int b);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgb_iv(int* rgb);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgb_bv(byte* rgb);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgb_f(float r, float g, float b);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgb_fv(float* rgb);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgb_hex(byte* rgb);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgba(int r, int g, int b, int a);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgba_u32(uint rgba);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgba_iv(int* rgba);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgba_bv(byte* rgba);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgba_f(float r, float g, float b, float a);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgba_fv(float* rgba);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgba_hex(float* hsv);

		// HSV/HSVA
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_hsva(int h, int s, int v, int a);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_hsva_iv(int* hsva);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_hsva_bv(byte* hsva);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_hsva_f(float h, float s, float v, float a);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_hsva_fv(float* hsva);

		// Color decomposition (out → pointer)
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_f(float* r, float* g, float* b, float* a, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_fv(float* rgba_out, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_d(double* r, double* g, double* b, double* a, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_dv(double* rgba_out, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial uint nk_color_u32(NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hex_rgba(byte* output, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hex_rgb(byte* output, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsv_i(int* h, int* s, int* v, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsv_b(byte* h, byte* s, byte* v, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsv_iv(int* hsv_out, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsv_bv(byte* hsv_out, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsv_f(float* h, float* s, float* v, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsv_fv(float* hsv_out, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsva_i(int* h, int* s, int* v, int* a, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsva_b(byte* h, byte* s, byte* v, byte* a, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsva_iv(int* hsva_out, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsva_bv(byte* hsva_out, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsva_f(float* h, float* s, float* v, float* a, NkColor src);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_color_hsva_fv(float* hsva_out, NkColor src);

		// Style item creation
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_style_item nk_style_item_image(nk_image img);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_style_item nk_style_item_color(NkColor col);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_style_item nk_style_item_hide();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_style_item nk_style_item_nine_slice(nk_nine_slice slice);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkColor nk_rgb_factor(NkColor col, float factor);
	}
}
