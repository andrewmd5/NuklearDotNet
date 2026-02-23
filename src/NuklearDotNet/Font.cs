using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NuklearDotNet {
	[StructLayout(LayoutKind.Sequential)]
	public struct nk_user_font_glyph {
		public nk_vec2 u;
		public nk_vec2 v;
		public nk_vec2 offset;
		public float width;
		public float height;
		public float xadvance;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_user_font {
		public NkHandle userdata;
		public float height;
		public delegate* unmanaged[Cdecl]<NkHandle, float, byte*, int, float> width;
		public delegate* unmanaged[Cdecl]<NkHandle, float, nk_user_font_glyph*, uint, uint, void> query;
		public NkHandle texture;
	}

	public enum nk_font_coord_type {
		NK_COORD_UV,
		NK_COORD_PIXEL
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_baked_font {
		public float height;
		public float ascent;
		public float descent;
		public uint glyph_offset;
		public uint glyph_count;
		public uint* ranges;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_font_config {
		public nk_font_config* next;
		public IntPtr ttf_blob;
		public nuint ttf_size;
		public byte ttf_data_owned_by_atlas;
		public byte merge_mode;
		public byte pixel_snap;
		public byte oversample_v;
		public byte oversample_h;
		fixed byte padding[3];
		public float size;
		public nk_font_coord_type coord_type;
		public nk_vec2 spacing;
		public uint* range;
		public nk_baked_font* font;
		public uint fallback_glyph;

		public nk_font_config* n;
		public nk_font_config* p;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_font_glyph {
		public uint codepoint;
		public float xadvance;
		public float x0;
		public float y0;
		public float x1;
		public float y1;
		public float w;
		public float h;
		public float u0;
		public float v0;
		public float u1;
		public float v1;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_font {
		public nk_font* next;
		public nk_user_font handle;
		public nk_baked_font info;
		public float scale;
		public nk_font_glyph* glyphs;
		public nk_font_glyph* fallback;
		public uint fallback_codepoint;
		public NkHandle texture;
		public nk_font_config* config;
	}

	public enum nk_font_atlas_format {
		NK_FONT_ATLAS_ALPHA8,
		NK_FONT_ATLAS_RGBA32
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_font_atlas {
		public IntPtr pixel;
		public int tex_width;
		public int tex_height;

		public nk_allocator permanent;
		public nk_allocator temporary;

		public nk_recti custom;
		public nk_cursor cursorArrow;
		public nk_cursor cursorText;
		public nk_cursor cursorMove;
		public nk_cursor cursorResizeV;
		public nk_cursor cursorResizeH;
		public nk_cursor cursorResizeTLDR;
		public nk_cursor cursorResizeTRDL;

		public int glyph_count;
		public nk_font_glyph* glyphs;
		public nk_font* default_font;
		public nk_font* fonts;
		public nk_font_config* config;
		public int font_num;
	}

	public static unsafe partial class Nuklear {
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial uint* nk_font_default_glyph_ranges();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial uint* nk_font_chinese_glyph_ranges();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial uint* nk_font_cyrillic_glyph_ranges();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial uint* nk_font_korean_glyph_ranges();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_font_atlas_init(nk_font_atlas* atlas, nk_allocator* alloc);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_font_atlas_init_default(nk_font_atlas* atlas);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_font_atlas_init_custom(nk_font_atlas* atlas, nk_allocator* persistent, nk_allocator* transient_alloc);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_font_atlas_begin(nk_font_atlas* atlas);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_font_config nk_font_config(float pixel_height);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_font* nk_font_atlas_add(nk_font_atlas* atlas, nk_font_config* fconfig);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_font* nk_font_atlas_add_default(nk_font_atlas* atlas, float height, nk_font_config* fconfig);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_font* nk_font_atlas_add_from_memory(nk_font_atlas* atlas, void* memory, nuint size, float height, nk_font_config* fconfig);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_font* nk_font_atlas_add_compressed(nk_font_atlas* atlas, void* memory, nuint size, float height, nk_font_config* fconfig);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_font* nk_font_atlas_add_compressed_base85(nk_font_atlas* atlas, byte* data, float height, nk_font_config* fconfig);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial IntPtr nk_font_atlas_bake(nk_font_atlas* atlas, int* width, int* height, nk_font_atlas_format afmt);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_font_atlas_end(nk_font_atlas* atlas, NkHandle tex, nk_draw_null_texture* drawnulltex);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_font_glyph* nk_font_find_glyph(nk_font* font, uint unicode);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_font_atlas_cleanup(nk_font_atlas* atlas);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_font_atlas_clear(nk_font_atlas* atlas);
	}
}
