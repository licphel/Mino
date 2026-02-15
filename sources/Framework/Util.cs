#region
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mino.Nio;
#endregion

namespace Mino.Framework;

/// <summary>
///     Some utilities used in the engine.
/// </summary>
public unsafe class Util {
	/// <summary>
	///     Compares the object arrays by index and
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	/// <exception cref="Error"></exception>
	public static int ChainCompare(object[] a, object[] b) {
		if (a.Length != b.Length) {
			throw new Error("different chain length");
		}
		for (int i = 0; i < a.Length; i++) {
			if (!a[i].Equals(b[i])) {
				return a[i].GetHashCode().CompareTo(b[i].GetHashCode());
			}
		}
		return 0;
	}

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
	/// <exception cref="IndexOutOfRangeException">Thrown if the span is not long enough.</exception>
	public static T Read<T>(ReadOnlySpan<byte> span, Endianness endianness = Endianness.Little)
		where T : unmanaged {
		int size = Unsafe.SizeOf<T>();
		if (span.Length < size) {
			throw new IndexOutOfRangeException("Span length is not enough.");
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

	public static T? AsNullable<T, R>(in R? o) {
		return (T?) Convert.ChangeType(o, typeof(T));
	}

	public static T As<T, R>(in R? o) {
		return AsNullable<T, R>(o) ?? throw new Error($"cast {typeof(R)} -> {typeof(T)} invalid");
	}

	public static T As<T>(in object? o) {
		return (T?) o ?? throw new Error($"cast {o?.GetType()} -> {typeof(T)} invalid");
	}
}
