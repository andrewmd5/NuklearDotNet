using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NuklearDotNet {
	public enum nk_bool {
		nk_false,
		nk_true
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NkColor {
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public NkColor(byte R, byte G, byte B, byte A = 255) {
			this.R = R;
			this.G = G;
			this.B = B;
			this.A = A;
		}

		public override string ToString() {
			return string.Format("({0}, {1}, {2}, {3})", R, G, B, A);
		}

		public static NkColor FromColorf(nk_colorf c) {
			return new NkColor(
				(byte)(c.r * 255),
				(byte)(c.g * 255),
				(byte)(c.b * 255),
				(byte)(c.a * 255));
		}

		/// <summary>
		/// Creates an NkColor from floating-point RGBA values (0.0 to 1.0).
		/// </summary>
		public static NkColor FromFloat(float r, float g, float b, float a = 1f) {
			return new NkColor(
				(byte)(Math.Clamp(r, 0f, 1f) * 255f),
				(byte)(Math.Clamp(g, 0f, 1f) * 255f),
				(byte)(Math.Clamp(b, 0f, 1f) * 255f),
				(byte)(Math.Clamp(a, 0f, 1f) * 255f));
		}

		/// <summary>
		/// Creates an NkColor from HSV values (h: 0-255, s: 0-255, v: 0-255) with full opacity.
		/// </summary>
		public static NkColor FromHsv(int h, int s, int v) {
			float hf = h / 255f;
			float sf = s / 255f;
			float vf = v / 255f;

			if (sf <= 0f)
				return FromFloat(vf, vf, vf);

			float hh = hf * 6f;
			int i = (int)hh;
			float ff = hh - i;
			float p = vf * (1f - sf);
			float q = vf * (1f - sf * ff);
			float t = vf * (1f - sf * (1f - ff));

			return i switch {
				0 => FromFloat(vf, t, p),
				1 => FromFloat(q, vf, p),
				2 => FromFloat(p, vf, t),
				3 => FromFloat(p, q, vf),
				4 => FromFloat(t, p, vf),
				_ => FromFloat(vf, p, q),
			};
		}

		/// <summary>
		/// Creates an NkColor from a hex string (e.g. "#FF0000", "FF0000FF", "#AABB").
		/// Supports 6-char RGB and 8-char RGBA formats, with or without leading '#'.
		/// </summary>
		public static NkColor FromHex(string hex) {
			ReadOnlySpan<char> span = hex.AsSpan();
			if (span.Length > 0 && span[0] == '#')
				span = span[1..];

			byte r = 0, g = 0, b = 0, a = 255;
			if (span.Length >= 6) {
				r = (byte)((ParseHexDigit(span[0]) << 4) | ParseHexDigit(span[1]));
				g = (byte)((ParseHexDigit(span[2]) << 4) | ParseHexDigit(span[3]));
				b = (byte)((ParseHexDigit(span[4]) << 4) | ParseHexDigit(span[5]));
			}
			if (span.Length >= 8) {
				a = (byte)((ParseHexDigit(span[6]) << 4) | ParseHexDigit(span[7]));
			}
			return new NkColor(r, g, b, a);
		}

		static int ParseHexDigit(char c) {
			return c switch {
				>= '0' and <= '9' => c - '0',
				>= 'a' and <= 'f' => c - 'a' + 10,
				>= 'A' and <= 'F' => c - 'A' + 10,
				_ => 0
			};
		}

		/// <summary>
		/// Converts this NkColor to a floating-point nk_colorf representation.
		/// </summary>
		public nk_colorf ToColorf() {
			return new nk_colorf { r = R / 255f, g = G / 255f, b = B / 255f, a = A / 255f };
		}

		/// <summary>
		/// Packs this NkColor into a 32-bit unsigned integer (RGBA byte order).
		/// </summary>
		public uint ToU32() {
			return (uint)(R | (G << 8) | (B << 16) | (A << 24));
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_colorf {
		public float r;
		public float g;
		public float b;
		public float a;

		public NkColor ToNkColor() {
			return NkColor.FromColorf(this);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_vec2 {
		public float x;
		public float y;

		public nk_vec2(float x, float y) {
			this.x = x;
			this.y = y;
		}

		public static implicit operator Vector2(nk_vec2 v) {
			return new Vector2(v.x, v.y);
		}

		public static implicit operator nk_vec2(Vector2 V) {
			return new nk_vec2(V.X, V.Y);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_vec2i {
		public short x;
		public short y;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NkRect {
		public float X;
		public float Y;
		public float W;
		public float H;

		public NkRect(float X, float Y, float W, float H) {
			this.X = X;
			this.Y = Y;
			this.W = W;
			this.H = H;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_recti {
		public short x;
		public short y;
		public short w;
		public short h;
	}

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct nk_glyph {
		[FieldOffset(0)]
		public fixed byte bytes[4];

		[FieldOffset(0)]
		public int glyph;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct NkHandle {
		[FieldOffset(0)]
		public int id;
		[FieldOffset(0)]
		public IntPtr ptr;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct nk_image {
		public NkHandle handle;
		public ushort w;
		public ushort h;
		public fixed ushort region[4];
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_nine_slice {
		public nk_image img;
		public ushort l;
		public ushort t;
		public ushort r;
		public ushort b;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_cursor {
		public nk_image img;
		public nk_vec2 size;
		public nk_vec2 offset;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct nk_scroll {
		public uint x;
		public uint y;
	}

	public enum nk_heading {
		NK_UP,
		NK_RIGHT,
		NK_DOWN,
		NK_LEFT
	}

	public enum nk_button_behavior {
		NK_BUTTON_DEFAULT,
		NK_BUTTON_REPEATER
	}

	public enum nk_modify {
		NK_FIXED = nk_bool.nk_false,
		NK_MODIFIABLE = nk_bool.nk_true
	}

	public enum nk_orientation {
		NK_VERTICAL,
		NK_HORIZONTAL
	}

	public enum nk_collapse_states {
		NK_MINIMIZED = nk_bool.nk_false,
		NK_MAXIMIZED = nk_bool.nk_true
	}

	public enum nk_show_states {
		NK_HIDDEN = nk_bool.nk_false,
		NK_SHOWN = nk_bool.nk_true
	}

	public enum nk_chart_type {
		NK_CHART_LINES,
		NK_CHART_COLUMN,
		NK_CHART_MAX
	}

	public enum nk_chart_event {
		NK_CHART_HOVERING = 0x01,
		NK_CHART_CLICKED = 0x02
	}

	public enum nk_color_format {
		NK_RGB,
		NK_RGBA
	}

	public enum nk_popup_type {
		NK_POPUP_STATIC,
		NK_POPUP_DYNAMIC
	}

	public enum nk_layout_format {
		NK_DYNAMIC,
		NK_STATIC
	}

	public enum nk_tree_type {
		NK_TREE_NODE,
		NK_TREE_TAB
	}

	/// <summary>
	/// Defines the value range and drag sensitivity for a Nuklear property widget.
	/// </summary>
	/// <typeparam name="T">The numeric type (float, int, or double).</typeparam>
	public readonly struct NkPropertyRange<T>(T min, T max, T step, float incPerPixel)
		where T : unmanaged, INumber<T> {
		public readonly T Min = min;
		public readonly T Max = max;
		public readonly T Step = step;
		public readonly float IncPerPixel = incPerPixel;
	}

	/// <summary>
	/// Chart slot configuration for colored chart variants.
	/// </summary>
	public readonly struct NkChartSlotConfig(int count, float min, float max, NkColor color, NkColor activeColor) {
		public readonly int Count = count;
		public readonly float Min = min;
		public readonly float Max = max;
		public readonly NkColor Color = color;
		public readonly NkColor ActiveColor = activeColor;
	}

	/// <summary>
	/// Configuration for list view row dimensions.
	/// </summary>
	public readonly struct NkListViewConfig(int rowHeight, int rowCount) {
		public readonly int RowHeight = rowHeight;
		public readonly int RowCount = rowCount;
	}

	public static unsafe partial class Nuklear {
		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkHandle nk_handle_ptr(IntPtr ptr);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkHandle nk_handle_id(int id);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_image nk_image_handle(NkHandle handle);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_image nk_image_ptr(IntPtr ptr);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_image nk_image_id(int id);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial int nk_image_is_subimage(nk_image* img);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_image nk_subimage_ptr(IntPtr ptr, ushort w, ushort h, NkRect sub_region);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_image nk_subimage_id(int id, ushort w, ushort h, NkRect sub_region);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_image nk_subimage_handle(NkHandle handle, ushort w, ushort h, NkRect sub_region);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial uint nk_murmur_hash(IntPtr key, int len, uint seed);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial void nk_triangle_from_direction(nk_vec2* result, NkRect r, float pad_x, float pad_y, nk_heading heading);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_vec2 nk_vec2i(int x, int y);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_vec2 nk_vec2v(float* xy);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_vec2 nk_vec2iv(int* xy);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkRect nk_get_null_rect();

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkRect nk_recti(int x, int y, int w, int h);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkRect nk_recta(nk_vec2 pos, nk_vec2 size);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkRect nk_rectv(float* xywh);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial NkRect nk_rectiv(int* xywh);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_vec2 nk_rect_pos(NkRect r);

		[LibraryImport(DllName)]
		[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static partial nk_vec2 nk_rect_size(NkRect r);
	}
}
