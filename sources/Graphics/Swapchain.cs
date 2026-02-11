using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;

namespace Mino.Graphics;

/// <summary>
///     Swapchain abstraction.
/// </summary>
public class Swapchain : IDisposable {
	private RenderBackend _backend;
	private bool _disposed;
	private bool _inPass;
	private uint _renderTarget;

	public Swapchain(in RenderPassDesc desc, RenderTarget rt) {
		_backend = RenderSystem.GetBackend();
		_renderTarget = rt;
		Desc = desc;
	}

	/// <summary>
	///     The render pass desc.
	/// </summary>
	public RenderPassDesc Desc { get; }

	/// <summary>
	///     If the swapchain is a window swapchain.
	/// </summary>
	public bool IsUltimate {
		get => _renderTarget == _backend.GetUltimateRenderTarget();
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		// Nothing to do.
		// We are building a cross-backend solution
		// So swapchain is not a native object.
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Acquires next frame.
	/// </summary>
	public void Acquire() {
		if (_inPass) {
			throw new Error("duplicated acquire");
		}
		_inPass = true;
		if (IsUltimate) {
			_backend.FrameBegin();
		}
		_backend.RenderPassBegin(_renderTarget, Desc);
	}

	/// <summary>
	///     Presents the rendered content.
	/// </summary>
	public void Present() {
		if (!_inPass) {
			throw new Error("duplicated present");
		}
		_inPass = false;
		_backend.RenderPassEnd();
		if (IsUltimate) {
			_backend.FrameEnd();
		}
	}

	// Finalizer in case.
	~Swapchain() {
		Dispose();
	}
}
