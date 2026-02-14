namespace Mino.Scene;

/// <summary>
///		Brush draw flags.
/// </summary>
[Flags]
public enum BrushFlag {
	None = 0,
	FlipX = 1 << 0,
	FlipY = 1 << 1
}
