namespace Mino.Mathematics.TwoDim;

/// <summary>
///     To batch poses as chunks.
/// </summary>
public static class PosBatcher {
	public const int Standard = 16;

	/// <summary>
	///		Batches a position by the given group sizes.
	/// </summary>
	/// <param name="pos">Pos to batch.</param>
	/// <param name="sizeX">Group x.</param>
	/// <param name="sizeY">Group y.</param>
	/// <returns>A batched position.</returns>
	public static PosI By(in PosI pos, int sizeX = Standard, int sizeY = Standard) {
		return new PosI(
			(int) MathF.Floor(pos.X * sizeX),
			(int) MathF.Floor(pos.Y * sizeY)
		);
	}
}
