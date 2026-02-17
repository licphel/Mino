namespace Mino.Graphics.Hardware.Enum;

/// <summary>
///     Basic shader module types.
/// </summary>
[Flags]
public enum ShaderType {
	Vertex = 1 << 0,
	Fragment = 1 << 1,
	Geometry = 1 << 2,
	Compute = 1 << 3
}
