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
	public static void Write<T>(in T value, Memory<byte> tmp,
		Endianness endianness = Endianness.Little) where T : unmanaged {
		Span<byte> span = tmp.Span;
		
		MemoryMarshal.Write(span, value);

		if (endianness != Endianness.Native) {
			if (endianness == Endianness.Little ^ BitConverter.IsLittleEndian) {
				span.Reverse();
			}
		}
	}

	/// <summary>
	///     Convert a byte span to a specified struct.
	/// </summary>
	/// <param name="span">Byte span to convert.</param>
	/// <param name="endianness">Used endianness.</param>
	/// <typeparam name="T">Struct type.</typeparam>
	/// <returns>A converted struct.</returns>
	/// <exception cref="Crash">Thrown if the span is not long enough.</exception>
	public static T Read<T>(ReadOnlySpan<byte> span, Endianness endianness = Endianness.Little)
		where T : unmanaged {
		int size = Unsafe.SizeOf<T>();
		if (span.Length < size) {
			throw new Crash("Span length is not enough.");
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
