namespace Mino.Graphics.RHI.Enum;

/// <summary>
///     Identifies a clearing operation type.
/// </summary>
[Flags]
public enum ClearMask {
	Color = 1 << 1,
	Depth = 1 << 2,
	Stencil = 1 << 3
}
