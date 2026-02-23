using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NuklearDotNet {
	public static unsafe partial class Nuklear {
		const string DllName = "Nuklear2";

		internal const int NK_INPUT_MAX = 512; // 16 by default

		// Debug sizeof helpers - compare native vs C# struct sizes
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_context();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_buffer();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_convert_config();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_draw_null_texture();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_allocator();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_handle();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_draw_list();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_style();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_input();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_font_atlas();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_offset_draw_list();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_draw_command();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_draw_vertex_layout_element();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_debug_dump_draw_commands(nk_context* ctx, nk_buffer* cmds, int max_cmds);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_cursor();
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_image();
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_font();
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_user_font();
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_baked_font();
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_font_glyph();
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_sizeof_font_config();
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_offset_atlas_default_font();
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_debug_offset_font_handle();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_font* nk_font_atlas_add_default_ex(nk_font_atlas* atlas, float height);
	}
}
