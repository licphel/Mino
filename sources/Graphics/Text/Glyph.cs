namespace Mino.Graphics.Text;

/// <summary>
///     A glyph of a font.
/// </summary>
public readonly struct Glyph {
	public readonly TexturePart TexPart;
	public readonly float Width;
	public readonly float Height;
	public readonly float Advance;
	public readonly float BearingX;
	public readonly float BearingY;

	public Glyph(TexturePart texPart, float width, float height, float advance, float bearingX, float bearingY) {
		TexPart = texPart;
		Width = width;
		Height = height;
		Advance = advance;
		BearingX = bearingX;
		BearingY = bearingY;
	}

	/// <summary>
	///     Scales the glyph by the specified scalar.
	/// </summary>
	/// <param name="scalar">Scale factor.</param>
	/// <returns>A scaled new glyph.</returns>
	public Glyph Scale(float scalar) {
		return new Glyph(
			TexPart,
			Width * scalar,
			Height * scalar,
			Advance * scalar,
			BearingX * scalar,
			BearingY * scalar
		);
	}
}
