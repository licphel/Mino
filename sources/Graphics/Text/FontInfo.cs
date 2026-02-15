namespace Mino.Graphics.Text;

/// <summary>
///     Font information struct.
/// </summary>
public readonly struct FontInfo {
	public readonly float Ascender;
	public readonly float Descender;
	public readonly float LineGap;

	public FontInfo(float ascender, float descender, float lineGap) {
		Ascender = ascender;
		Descender = descender;
		LineGap = lineGap;
	}
}
