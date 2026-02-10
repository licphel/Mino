namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     Describes a shader program.
/// </summary>
public struct ShaderProgramDesc {
	public uint[] Modules;

	public ShaderProgramDesc() {
		Modules = [];
	}
}
