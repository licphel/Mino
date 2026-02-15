#region
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Text;

/// <summary>
///     Baked glyph instance.
/// </summary>
public struct GlyphInstance {
	public Glyph Glyph;
	public Box2 Bounds;
	public int Index;
	public int Line;

	public GlyphInstance(Glyph glyph, Box2 bounds, int index, int line) {
		Glyph = glyph;
		Bounds = bounds;
		Index = index;
		Line = line;
	}
}
