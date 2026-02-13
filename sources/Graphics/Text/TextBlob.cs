using Mino.Mathematics;

namespace Mino.Graphics.Text;

/// <summary>
///     A baked text instance, used for rendering and testing.
/// </summary>
public class TextBlob {
	private readonly Font _font;
	private readonly float _lineH;
	private readonly string _text;

	internal TextBlob(string text, Font font, float lineH, float maxWidth, TextNextLine nextLine) {
		_text = text;
		_font = font;
		_lineH = lineH;

		float cursorX = 0;
		float cursorY = 0;
		float descender = font.Info.Descender;
		float lineGap = font.Info.LineGap;
		int currentLine = 0;
		int lastSpaceIndex = -1;
		float maxLineWidth = 0;

		for (int i = 0; i < text.Length; i++) {
			char c = text[i];

			// Invisible chars.
			if (c is '\r' or '\t') {
				continue;
			}

			if (c == '\n') {
				cursorX = 0;
				cursorY += lineGap + lineH;
				currentLine++;
				lastSpaceIndex = -1;
				continue;
			}

			Glyph glyph = font.GetGlyph(c);

			if (char.IsWhiteSpace(c) && !char.IsControl(c)) {
				lastSpaceIndex = i;
			}

			if (cursorX + glyph.Advance > maxWidth && cursorX > 0) {
				if (lastSpaceIndex != -1 && lastSpaceIndex > 0) {
					int charsToRemove = i - lastSpaceIndex;
					for (int j = 0; j < charsToRemove; j++) {
						if (GlyphRunList.Count > 0) {
							GlyphInstance lastGlyph = GlyphRunList[^1];
							cursorX -= lastGlyph.Glyph.Advance;
							GlyphRunList.RemoveAt(GlyphRunList.Count - 1);
						}
					}
					i = lastSpaceIndex;
					lastSpaceIndex = -1;
				}

				cursorX = 0;
				cursorY += lineGap + lineH;
				currentLine++;
				// Roll back to this char.
				i--;
				continue;
			}

			float x = cursorX + glyph.BearingX;
			float y = cursorY - glyph.BearingY;

			float top = y;
			float bottom = y + glyph.Height;
			float left = x;
			float right = x + glyph.Width;

			Box2 bounds = Box2.CreateByPoints(left, top, right, bottom);

			GlyphRunList.Add(new GlyphInstance(glyph, new Vector2(x, y), bounds, i, currentLine));

			cursorX += glyph.Advance;
			maxLineWidth = Math.Max(maxLineWidth, cursorX);

			if (nextLine == TextNextLine.Other) {
				lastSpaceIndex = i;
			}
		}

		Width = maxLineWidth;

		float maxDescender = 0;
		foreach (GlyphInstance gi in GlyphRunList) {
			if (gi.Line == currentLine) {
				float desc = gi.Position.Y + gi.Glyph.Height - cursorY;
				maxDescender = Math.Max(maxDescender, desc);
			}
		}

		Height = cursorY + Math.Max(0, -descender) + lineH;
	}

	/// <summary>
	///     Max width.
	/// </summary>
	public float Width { get; private set; }

	/// <summary>
	///     Max height.
	/// </summary>
	public float Height { get; private set; }

	/// <summary>
	///     Character count of the blob.
	/// </summary>
	public int Length {
		get => GlyphRunList.Count;
	}

	/// <summary>
	///     Glyph instance list.
	/// </summary>
	public List<GlyphInstance> GlyphRunList { get; } = new List<GlyphInstance>();

	/// <summary>
	///     Gets the glyph instance by a position.
	/// </summary>
	/// <param name="position">Pointing position.</param>
	/// <param name="instance">The correspondent glyph instance.</param>
	/// <returns>True if found, otherwise false.</returns>
	public bool GetGlyphInstance(in Vector2 position, out GlyphInstance instance) {
		for (int i = 0; i < GlyphRunList.Count; i++) {
			if (GlyphRunList[i].Bounds.Contains(position)) {
				instance = GlyphRunList[i];
				return true;
			}
		}
		instance = new GlyphInstance();
		return false;
	}
}
