#region
using Mino.Framework.Resource;
using Mino.Graphics.Desc;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Represents a texture sampler.
/// </summary>
public interface Sampler : ThreadContextHolder, IDisposable {
	/// <summary>
	///     The sampler desc.
	/// </summary>
	SamplerDesc Desc { get; }
}
