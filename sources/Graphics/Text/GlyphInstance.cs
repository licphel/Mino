using Mino.Mathematics;

namespace Mino.Graphics.Text;

/// <summary>
///     Baked glyph instance.
/// </summary>
public readonly struct GlyphInstance {
	public readonly Glyph Glyph;
	public readonly Vector2 Position;
	public readonly Box2 Bounds;
	public readonly int Index;
	public readonly int Line;

	public GlyphInstance(Glyph glyph, Vector2 position, Box2 bounds, int index, int line) {
		Glyph = glyph;
		Position = position;
		Bounds = bounds;
		Index = index;
		Line = line;
	}
}
