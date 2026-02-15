#region
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#endregion

namespace Mino.Nio;

/// <summary>
///     Endianness-careful byte buffer.
/// </summary>
public class ByteBuffer {
	public const int DefaultCapacity = 64;
	private Endianness _endianness;

	private bool _littleEndian;
	public byte[] BufferArray;
	public int Capacity;
	public int ReadIndex;
	public int WriteIndex;

	public ByteBuffer(int cap = DefaultCapacity, Endianness endianness = Endianness.Unsure) {
		BufferArray = new byte[cap];
		Capacity = cap;
		ReadIndex = 0;
		WriteIndex = 0;
		Endianness = endianness;
	}

	public ByteBuffer(byte[] bytes, Endianness endianness = Endianness.Unsure) {
		BufferArray = new byte[bytes.Length];
		Array.Copy(bytes, 0, BufferArray, 0, BufferArray.Length);
		Capacity = BufferArray.Length;
		ReadIndex = 0;
		WriteIndex = bytes.Length;
		Endianness = endianness;
	}

	/// <summary>
	///     The used buffer endianness.
	/// </summary>
	public Endianness Endianness {
		get => _endianness;
		set {
			_endianness = value;
			_littleEndian = value switch {
				Endianness.Little => true,
				Endianness.Big => false,
				Endianness.Native => BitConverter.IsLittleEndian,
				_ => _littleEndian
			};
		}
	}

	/// <summary>
	///     Free bytes to write.
	/// </summary>
	public int FreeBytes {
		get => Capacity - WriteIndex;
	}

	/// <summary>
	///     Remaining bytes to read.
	/// </summary>
	public int ReadableBytes {
		get => WriteIndex - ReadIndex;
	}

	/// <summary>
	///     Converts the written buffer to a span.
	/// </summary>
	/// <returns>A span containing the written bytes.</returns>
	public Span<byte> AsSpan() {
		return new Span<byte>(BufferArray, 0, WriteIndex);
	}

	/// <summary>
	///     Clears all bytes but keeps the capacity.
	/// </summary>
	public void Clear() {
		ReadIndex = 0;
		WriteIndex = 0;
		Capacity = BufferArray.Length;
	}

	/// <summary>
	///     Compacts the buffer, discarding read data.
	/// </summary>
	public void Compact() {
		if (ReadIndex == 0) {
			return;
		}
		int remaining = ReadableBytes;
		if (remaining > 0) {
			Buffer.BlockCopy(BufferArray, ReadIndex, BufferArray, 0, remaining);
		}
		ReadIndex = 0;
		WriteIndex = remaining;
	}

	/// <summary>
	///     Ensures the remaining capacity enough to write the given bytes.
	/// </summary>
	/// <param name="additional">Ensured additional bytes.</param>
	public void Ensure(int additional) {
		fixSizeAndReset(WriteIndex + additional);
	}

	/// <summary>
	///     Writes a byte span into the buffer.
	/// </summary>
	/// <param name="bytes">Data to write.</param>
	public unsafe void WriteBytes(ReadOnlySpan<byte> bytes) {
		int len = bytes.Length;
		if (len <= 0) {
			return;
		}
		int total = len + WriteIndex;
		fixSizeAndReset(total);

		fixed (byte* src = bytes) {
			fixed (byte* dst = BufferArray) {
				Buffer.MemoryCopy(src, dst + WriteIndex, len, len);
			}
		}
		WriteIndex = total;
	}

	/// <summary>
	///     Writes a primitive value into the buffer.
	/// </summary>
	/// <param name="value">The value to write.</param>
	/// <typeparam name="T">Value type.</typeparam>
	public void Write<T>(in T value) where T : unmanaged {
		int size = Unsafe.SizeOf<T>();

		if (size > 1 && Endianness == Endianness.Unsure) {
			throw new Error("unsure endianness for >1 bytes read");
		}

		Ensure(size);

		Span<byte> span = BufferArray.AsSpan(WriteIndex, size);
		MemoryMarshal.Write(span, value);

		if (_littleEndian ^ BitConverter.IsLittleEndian) {
			span.Reverse();
		}
		WriteIndex += size;
	}

	/// <summary>
	///     Writes a span of primitive values into the buffer.
	/// </summary>
	/// <param name="span">The value span to write.</param>
	/// <typeparam name="T">Value type.</typeparam>
	public void Write<T>(ReadOnlySpan<T> span) where T : unmanaged {
		foreach (ref readonly T t in span) {
			Write(t);
		}
	}

	/// <summary>
	///     Write a string into the buffer.
	/// </summary>
	/// <param name="value">The string to write.</param>
	public void WriteString(string value) {
		Write(value.Length);
		for (int i = 0; i < value.Length; i++) {
			Write(value[i]);
		}
	}

	/// <summary>
	///     Reads a byte span from the buffer.
	/// </summary>
	/// <param name="bytes">Data destination.</param>
	/// <param name="len">Read length, by default is the length of the span.</param>
	public unsafe void ReadBytes(Span<byte> bytes, int len = -1) {
		if (len == -1) {
			len = bytes.Length;
		}

		fixed (byte* src = BufferArray) {
			fixed (byte* dst = bytes) {
				Buffer.MemoryCopy(src + ReadIndex, dst, len, len);
			}
		}
		ReadIndex += len;
	}

	/// <summary>
	///     Reads a primitive value from the buffer.
	/// </summary>
	/// <typeparam name="T">Value type.</typeparam>
	/// <returns>A primitive value.</returns>
	public T Read<T>() where T : unmanaged {
		int size = Unsafe.SizeOf<T>();

		if (ReadableBytes < size) {
			throw new Error("nothing to read");
		}

		if (size > 1 && Endianness == Endianness.Unsure) {
			throw new Error("unsure endianness for >1 bytes read");
		}

		Span<byte> span = BufferArray.AsSpan(ReadIndex, size);
		ReadIndex += size;

		if (_littleEndian ^ BitConverter.IsLittleEndian) {
			Span<byte> temp = stackalloc byte[size];
			span.CopyTo(temp);
			temp.Reverse();
			return MemoryMarshal.Read<T>(temp);
		}
		return MemoryMarshal.Read<T>(span);
	}

	/// <summary>
	///     Reads a string from the buffer.
	/// </summary>
	/// <returns>The read string.</returns>
	public string ReadString() {
		int len = Read<int>();
		char[] chars = new char[len];
		for (int i = 0; i < len; i++) {
			chars[i] = Read<char>();
		}
		return new string(chars);
	}

	/// <summary>
	///     Reads a ascii string (byte as char) from the buffer.
	/// </summary>
	/// <param name="len">The length.</param>
	/// <returns>The read ascii string.</returns>
	public string ReadAscii(int len) {
		char[] chars = new char[len];
		for (int i = 0; i < len; i++) {
			chars[i] = (char) Read<byte>();
		}
		return new string(chars);
	}

	protected static int nextLength(int value) {
		if (value == 0) {
			return 1;
		}
		value--;
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		return value + 1;
	}

	protected void fixSizeAndReset(int futureLen) {
		int currLen = Capacity;
		if (futureLen > currLen) {
			int size = nextLength(currLen) * 2;
			if (futureLen > size) {
				size = nextLength(futureLen) * 2;
			}
			byte[] newArray = new byte[size];
			Array.Copy(BufferArray, 0, newArray, 0, currLen);
			BufferArray = newArray;
			Capacity = size;
		}
	}
}
