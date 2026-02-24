using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NuklearDotNet {

	/// <summary>
	/// Opaque handle to a native <c>nk_console</c> widget node. Never dereferenced in managed code —
	/// used only as a typed pointer target for P/Invoke calls.
	/// </summary>
	/// <example>
	/// <code>
	/// nk_console* root = console.Root;
	/// nk_console* btn = console.Button(root, "Play");
	/// </code>
	/// </example>
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct nk_console {
		private readonly nint _handle;
	}

	public static unsafe partial class Nuklear {

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_init(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_free(nk_console* console);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_render(nk_console* console);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_render_window(nk_console* console, byte* title, NkRect bounds, uint flags);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_get_top(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_console_get_widget_index(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_check_tooltip(nk_console* console);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_check_up_down(nk_console* widget, NkRect bounds);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_console_is_active_widget(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_active_parent(nk_console* console);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_set_active_parent(nk_console* new_parent);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_set_active_widget(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_get_active_widget(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_console_button_pushed(nk_console* console, int button);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_set_gamepads(nk_console* console, NkGamepads* gamepads);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial NkGamepads* nk_console_get_gamepads(nk_console* console);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_set_tooltip(nk_console* widget, byte* tooltip);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_set_label(nk_console* widget, byte* label, int label_length);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial byte* nk_console_get_label(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_free_children(nk_console* console);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_layout_widget(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_add_child(nk_console* parent, nk_console* child);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_set_height(nk_console* widget, int height);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_console_height(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_console_selectable(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void* nk_console_user_data(nk_console* console);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_set_user_data(nk_console* console, void* user_data);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_console_trigger_event(nk_console* widget, int type);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_add_event(nk_console* widget, int type, delegate* unmanaged[Cdecl]<nk_console*, void*, void> callback);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_add_event_handler(nk_console* widget, int type,
			delegate* unmanaged[Cdecl]<nk_console*, void*, void> callback, void* user_data,
			delegate* unmanaged[Cdecl]<nk_console*, void*, void> destructor);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_button(nk_console* parent, byte* text);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_button_onclick(nk_console* parent, byte* text,
			delegate* unmanaged[Cdecl]<nk_console*, void*, void> onclick);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_button_onclick_handler(nk_console* parent, byte* text,
			delegate* unmanaged[Cdecl]<nk_console*, void*, void> callback, void* data,
			delegate* unmanaged[Cdecl]<nk_console*, void*, void> destructor);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_console_button_get_symbol(nk_console* button);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_button_set_symbol(nk_console* button, int symbol);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_button_set_image(nk_console* button, nk_image image);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_image nk_console_button_get_image(nk_console* button);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_button_back(nk_console* button, void* user_data);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_label(nk_console* parent, byte* text);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_checkbox(nk_console* parent, byte* text, int* active);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_progress(nk_console* parent, byte* text, nuint* current, nuint max);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_property_int(nk_console* parent, byte* label, int min, int* val, int max, int step, float inc_per_pixel);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_property_float(nk_console* parent, byte* label, float min, float* val, float max, float step, float inc_per_pixel);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_slider_int(nk_console* parent, byte* label, int min, int* val, int max, int step);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_slider_float(nk_console* parent, byte* label, float min, float* val, float max, float step);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_combobox(nk_console* parent, byte* label, byte* items_separated_by_separator, int separator, int* selected);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_combobox_update(nk_console* combobox, byte* label, byte* items_separated_by_separator, int separator, int* selected);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_radio(nk_console* parent, byte* label, int* selected);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_console_radio_is_selected(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial int nk_console_radio_index(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_textedit(nk_console* parent, byte* label, byte* buffer, int buffer_size);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_textedit_text(nk_console* textedit);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_row_begin(nk_console* parent);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_row_end(nk_console* row);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_file(nk_console* parent, byte* label, byte* file_path_buffer, int file_path_buffer_size);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_dir(nk_console* parent, byte* label, byte* dir_buffer, int dir_buffer_size);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_file_add_entry(nk_console* parent, byte* path, int is_directory);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_file_refresh(nk_console* widget, void* user_data);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_file_set_file_user_data(nk_console* file, void* user_data);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void* nk_console_file_get_file_user_data(nk_console* file);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_image(nk_console* parent, nk_image image);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_image_color(nk_console* parent, nk_image image, NkColor color);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_image_set_image(nk_console* widget, nk_image image);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_image nk_console_image_get_image(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_image_set_color(nk_console* widget, NkColor color);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial NkColor nk_console_image_get_color(nk_console* widget);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_spacing(nk_console* parent, int cols);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_color(nk_console* parent, byte* label, nk_colorf* color, nk_color_format format);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_knob_int(nk_console* parent, byte* label, int min, int* val, int max, int step, float inc_per_pixel);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_knob_float(nk_console* parent, byte* label, float min, float* val, float max, float step, float inc_per_pixel);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial nk_console* nk_console_input(nk_console* parent, byte* label, int gamepad_number, int* out_gamepad_number, NkGamepadButton* out_gamepad_button);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_show_message(nk_console* console, byte* text);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_set_delta_time(float dt);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_set_visible(nk_console* widget, int visible);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial void nk_console_set_selectable(nk_console* widget, int selectable);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		internal static partial NkRect nk_console_get_active_bounds(nk_console* console);
	}
}
