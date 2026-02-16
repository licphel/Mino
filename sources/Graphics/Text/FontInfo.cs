namespace Mino.Graphics.Text;

/// <summary>
///     Font information struct.
/// </summary>
public readonly struct FontInfo {
	public readonly float Ascender;
	public readonly float Descender;
	public readonly float LineGap;
	public readonly float UnderlinePos;
	public readonly float UnderlineThickness;
	public readonly float StrikeoutPos;
	public readonly float StrikeoutThickness;

	public FontInfo(float ascender, float descender, float lineGap, float underlinePos, float underlineThickness,
		float strikeoutPos, float strikeoutThickness) {
		Ascender = ascender;
		Descender = descender;
		LineGap = lineGap;
		UnderlinePos = underlinePos;
		UnderlineThickness = underlineThickness;
		StrikeoutPos = strikeoutPos;
		StrikeoutThickness = strikeoutThickness;
	}
}
