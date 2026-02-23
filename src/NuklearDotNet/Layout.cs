using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NuklearDotNet {
	public static unsafe partial class Nuklear {
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_set_min_row_height(nk_context* ctx, float height);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_reset_min_row_height(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkRect nk_layout_widget_bounds(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial float nk_layout_ratio_from_pixel(nk_context* ctx, float pixel_width);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row_dynamic(nk_context* ctx, float height, int cols);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row_static(nk_context* ctx, float height, int item_width, int cols);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row_begin(nk_context* ctx, nk_layout_format fmt, float row_height, int cols);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row_push(nk_context* ctx, float val);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row_end(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row(nk_context* ctx, nk_layout_format fmt, float height, int cols, float* ratio);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row_template_begin(nk_context* ctx, float row_height);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row_template_push_dynamic(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row_template_push_variable(nk_context* ctx, float min_width);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row_template_push_static(nk_context* ctx, float width);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_row_template_end(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_space_begin(nk_context* ctx, nk_layout_format fmt, float height, int widget_count);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_space_push(nk_context* ctx, NkRect rect);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_layout_space_end(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkRect nk_layout_space_bounds(nk_context* ctx);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_vec2 nk_layout_space_to_screen(nk_context* ctx, nk_vec2 v);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_vec2 nk_layout_space_to_local(nk_context* ctx, nk_vec2 v);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkRect nk_layout_space_rect_to_screen(nk_context* ctx, NkRect r);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkRect nk_layout_space_rect_to_local(nk_context* ctx, NkRect r);
	}
}
