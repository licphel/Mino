#region
using System.Runtime.CompilerServices;
#endregion

namespace Mino.Utility;

/// <summary>
///     A 2D measure^2 byte array with extensible metadata.
/// </summary>
public sealed class ByteArena2D {
	public readonly byte[] Bytes;
	public readonly int Measure;
	public readonly int BPE;
	private readonly int _logM;

	private ByteArena2D(int measure, int bpe) {
		Measure = measure;
		_logM = (int) Math.Log2(Measure);
		BPE = bpe;
		Bytes = new byte[measure * measure * BPE];
	}

	/// <summary>
	///     Gets an arena of the given measure.
	/// </summary>
	/// <param name="measure">The measure of the arena, must be in the form of 2^k.</param>
	/// <param name="extraBPE">Extra bytes per element, not including the meta finder 8 bytes.</param>
	/// <returns>A new byte arena.</returns>
	public static ByteArena2D OfMeasure(int measure, int extraBPE) {
		if ((measure & measure - 1) != 0) {
			throw new Crash($"Measure {measure} is not a power of 2.");
		}
		return new ByteArena2D(measure, extraBPE);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int index(int x, int y) {
		x &= Measure - 1;
		y &= Measure - 1;
		return (y << _logM) + x;
	}

	/// <summary>
	///     Gets a memory block of extra data at (x, y).
	/// </summary>
	/// <param name="x">X coordinate, which can exceed the arena.</param>
	/// <param name="y">Y coordinate, which can exceed the arena.</param>
	/// <returns>A target memory block [indexOf(x, y), ExtraBPE].</returns>
	public Memory<byte> At(int x, int y) {
		return new Memory<byte>(Bytes, index(x, y) * BPE, BPE);
	}
}
