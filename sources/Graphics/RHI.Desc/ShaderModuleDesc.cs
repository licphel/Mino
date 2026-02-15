#region
using Mino.Graphics.RHI.Enum;
#endregion

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     Describes a shader module.
/// </summary>
public struct ShaderModuleDesc {
	public ShaderType Type;
	public string Code;
	public string[] Targets;

	public ShaderModuleDesc() {
		Type = default;
		Code = string.Empty;
		Targets = [];
	}
}
