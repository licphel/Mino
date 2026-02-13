namespace Mino.Graphics.Text;

/// <summary>
///     Font information struct.
/// </summary>
public readonly struct FontInfo {
	public readonly float Ascender;
	public readonly float Descender;
	public readonly float LineGap;
	public readonly float Height;

	public FontInfo(float ascender, float descender, float lineGap, float height) {
		Ascender = ascender;
		Descender = descender;
		LineGap = lineGap;
		Height = height;
	}
}
