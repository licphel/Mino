#region
using Mino.Graphics.Hardware.Enum;
#endregion

namespace Mino.Graphics.Hardware.Desc;

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
