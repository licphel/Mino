#region
using Mino.Framework.Resource;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Represents a render target (a series of textures).
/// </summary>
public interface RenderTarget : ThreadContextHolder, IDisposable {
	private static RenderTarget? _ultimate;
	private static Lock _lock = new Lock();

	/// <summary>
	///     Gets the ultimate render target.
	/// </summary>
	/// <returns>A render target representing the window.</returns>
	static RenderTarget GetUltimate() {
		if (_ultimate == null) {
			lock (_lock) {
				RenderTargetDesc rtDesc = new RenderTargetDesc {
					IsUltimate = true
				};
				_ultimate ??= RenderSystem.Create<RenderTarget>(rtDesc);
			}
		}
		return _ultimate;
	}

	/// <summary>
	///     The render target desc.
	/// </summary>
	RenderTargetDesc Desc { get; }

	/// <summary>
	///     Checks if this is the ultimate render target.
	/// </summary>
	bool IsUltimate {
		get => this == _ultimate;
	}

	/// <summary>
	///     Acquires next frame.
	/// </summary>
	void Acquire(in RenderPassDesc? desc = null);

	/// <summary>
	///     Presents the rendered content.
	/// </summary>
	void Present();

	/// <summary>
	///     Blits the render target.
	/// </summary>
	/// <param name="to">Dst render target.</param>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	/// <param name="srcX">Src x.</param>
	/// <param name="srcY">Src y.</param>
	/// <param name="srcW">Src width.</param>
	/// <param name="srcH">Src height.</param>
	/// <param name="filter">Blit filter.</param>
	void Blit(RenderTarget to, int x, int y, int w, int h, int srcX, int srcY, int srcW, int srcH,
		TextureFilter filter = TextureFilter.Nearest);
}
