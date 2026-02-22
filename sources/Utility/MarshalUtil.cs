#region
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mino.Nio;
#endregion

namespace Mino.Utility;

/// <summary>
///     Fast endianness careful marshal utils.
/// </summary>
public unsafe class MarshalUtil {
	/// <summary>
	///     Convert a struct to bytes.
	/// </summary>
	/// <param name="value">Conversion target.</param>
	/// <param name="tmp">Temp array.</param>
	/// <param name="endianness">Used endianness.</param>
	/// <typeparam name="T">Target type.</typeparam>
	/// <returns>A span containing the bytes.</returns>
	public static ReadOnlySpan<byte> Write<T>(in T value, byte* tmp = null,
		Endianness endianness = Endianness.Little) where T : unmanaged {
		int size = Unsafe.SizeOf<T>();
		Span<byte> span;
		if (tmp == null) {
			span = new byte[size];
		} else {
			span = new Span<byte>(tmp, size);
		}
		MemoryMarshal.Write(span, value);

		if (endianness != Endianness.Native) {
			if (endianness == Endianness.Little ^ BitConverter.IsLittleEndian) {
				span.Reverse();
			}
		}
		return span;
	}

	/// <summary>
	///     Convert a byte span to a specified struct.
	/// </summary>
	/// <param name="span">Byte span to convert.</param>
	/// <param name="endianness">Used endianness.</param>
	/// <typeparam name="T">Struct type.</typeparam>
	/// <returns>A converted struct.</returns>
	/// <exception cref="Error">Thrown if the span is not long enough.</exception>
	public static T Read<T>(ReadOnlySpan<byte> span, Endianness endianness = Endianness.Little)
		where T : unmanaged {
		int size = Unsafe.SizeOf<T>();
		if (span.Length < size) {
			throw new Error("Span length is not enough.");
		}

		if (endianness != Endianness.Native) {
			if (endianness == Endianness.Little ^ BitConverter.IsLittleEndian) {
				Span<byte> temp = stackalloc byte[size];
				span.CopyTo(temp);
				temp.Reverse();
				return MemoryMarshal.Read<T>(temp);
			}
		}
		return MemoryMarshal.Read<T>(span);
	}
}
