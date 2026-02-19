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
	public Box2 AdjacentBounds;
	public int Index;
	public int Line;
	public bool Visible;

	public GlyphInstance(Glyph glyph, Box2 bounds, Box2 adjacentBounds, int index, int line, bool visible = true) {
		Glyph = glyph;
		Bounds = bounds;
		Index = index;
		Line = line;
		AdjacentBounds = adjacentBounds;
		Visible = visible;
	}

	public static GlyphInstance Invisible(int index, int line) {
		return new GlyphInstance(default, default, default, index, line, false);
	}
}
