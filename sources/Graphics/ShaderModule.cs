#region
using Mino.Framework.Resource;
using Mino.Graphics.Desc;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Represents a compiled shader module.
/// </summary>
public interface ShaderModule : ThreadContextHolder, IDisposable {
	/// <summary>
	///     The shader module desc.
	/// </summary>
	ShaderModuleDesc Desc { get; }
}
