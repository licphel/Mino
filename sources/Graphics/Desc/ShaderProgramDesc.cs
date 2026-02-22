namespace Mino.Graphics.Desc;

/// <summary>
///     Describes a shader program.
/// </summary>
public struct ShaderProgramDesc {
	public required ShaderModule[] Modules;

	public ShaderProgramDesc() {
	}
}
