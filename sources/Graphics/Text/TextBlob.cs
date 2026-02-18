#region
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Text;

/// <summary>
///     A baked text instance, used for rendering and testing.
/// </summary>
public class TextBlob {
	public readonly FontInfo Info;
	
	internal TextBlob(Font font, string text, float maxWidth, float lineH, FontStyle style) {
		float cursorX = 0;
		float cursorY = 0;
		int currentLine = 0;
		int lastSpaceIndex = -1;
		float maxLineWidth = 0;
		float scale = lineH / Font.BasicLineHeight;
		float descender = font.Info.Descender * scale;
		float lineGap = font.Info.LineGap * scale;

		for (int i = 0; i < text.Length; i++) {
			char ch = text[i];

			// Invisible chars.
			if (ch is '\r' or '\t') {
				continue;
			}

			if (ch == '\n') {
				cursorX = 0;
				cursorY += lineGap;
				currentLine++;
				lastSpaceIndex = -1;
				continue;
			}
			
			if (IsCJK(ch)) {
				lastSpaceIndex = i;
			}

			Glyph glyph = font.GetGlyph(ch, style).Scale(scale);

			if (char.IsWhiteSpace(ch) && !char.IsControl(ch)) {
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
				cursorY += lineGap;
				currentLine++;
				// Roll back to this char.
				i--;
				continue;
			}

			float x = cursorX + glyph.BearingX;
			float y = cursorY + lineH - glyph.BearingY;
			
			float bottom = y + glyph.Height;
			float right = x + glyph.Width;

			Box2 bounds = Box2.CreateByPoints(x, y, right, bottom);
			GlyphRunList.Add(new GlyphInstance(glyph, bounds, i, currentLine));

			cursorX += glyph.Advance;
			maxLineWidth = Math.Max(maxLineWidth, cursorX);
		}

		Width = maxLineWidth;

		float maxDescender = 0;
		foreach (GlyphInstance gi in GlyphRunList) {
			if (gi.Line == currentLine) {
				float desc = gi.Bounds.MaxY - cursorY;
				maxDescender = Math.Max(maxDescender, desc);
			}
		}

		Height = cursorY + Math.Max(0, -descender) + lineH;

		Info = new FontInfo(
			font.Info.Ascender * scale,
			font.Info.Descender * scale,
			font.Info.LineGap * scale,
			font.Info.UnderlinePos * scale,
			font.Info.UnderlineThickness * scale,
			font.Info.StrikeoutPos * scale,
			font.Info.StrikeoutThickness * scale
		);
		
		return;

		static bool IsCJK(char ch) {
			// TODO: Add correct next line logic.
			return true;
			/*
			return ch >= 0x4E00 && ch <= 0x9FFF ||
				ch >= 0x3400 && ch <= 0x4DBF;
			*/
		}
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
