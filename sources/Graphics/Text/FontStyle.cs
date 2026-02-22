namespace Mino.Graphics.Text;

/// <summary>
///     Optional font styles.
/// </summary>
[Flags]
public enum FontStyle {
	Regular = 0,
	Bold = 1 << 0,
	Italic = 1 << 1,
	Underline = 1 << 2,
	Strikethrough = 1 << 3
}
