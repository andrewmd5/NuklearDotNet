using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace NuklearDotNet {
	internal static class NkStringHelper {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetUtf8(ReadOnlySpan<char> source, Span<byte> destination) {
			int written = Encoding.UTF8.GetBytes(source, destination);
			destination[written] = 0; // null-terminate
			return written;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetUtf8ByteCount(string s) {
			return Encoding.UTF8.GetByteCount(s) + 1; // +1 for null terminator
		}
	}
}
