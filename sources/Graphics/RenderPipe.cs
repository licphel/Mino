#region
using Mino.Framework.Resource;
using Mino.Graphics.Desc;
#endregion

namespace Mino.Graphics;

/// <summary>
///     An immutable list of render states.
/// </summary>
public interface RenderPipe : ThreadContextHolder, IDisposable {
	/// <summary>
	///     The pipe desc.
	/// </summary>
	RenderPipeDesc Desc { get; }
}
