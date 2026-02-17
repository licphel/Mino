namespace Mino.Graphics.Hardware.Enum;

/// <summary>
///     Basic texture usages.
/// </summary>
[Flags]
public enum TextureUsage {
	Sampled = 1 << 0,
	Storage = 1 << 1,
	ColorAttachment = 1 << 2,
	DepthStencilAttachment = 1 << 3,
	TransientAttachment = 1 << 4,
	InputAttachment = 1 << 5
}
