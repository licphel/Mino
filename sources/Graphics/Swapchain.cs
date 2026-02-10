using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;

namespace Mino.Graphics;

public class Swapchain {
	private static Swapchain? _directSwapchain = null;
	private static Lock _lock = new Lock();

	public static Swapchain GetDirectSwapchain() {
		if (_directSwapchain == null) {
			lock (_lock) {
				_directSwapchain ??= new Swapchain(new RenderPassDesc());
			}
		}
		return _directSwapchain;
	}
	
	private RenderBackend _backend;
	private uint _renderTarget;
	private bool _inPass;

	private Swapchain(in RenderPassDesc desc, uint? renderTarget = null) {
		_backend = RenderSystem.GetBackend();
		_renderTarget = renderTarget ?? _backend.GetUltimateRenderTarget();
		Desc = desc;
	}
	
	public RenderPassDesc Desc { get; }

	/// <summary>
	///		Acquires next frame.
	/// </summary>
	public void Acquire() {
		if (_inPass) {
			throw new Error("duplicated acquire");
		}
		_inPass = true;
		if (Direct) {
			_backend.FrameBegin();
		}
		_backend.RenderPassBegin(_renderTarget, Desc);
	}

	/// <summary>
	///		Presents the rendered content.
	/// </summary>
	public void Present() {
		if (!_inPass) {
			throw new Error("duplicated present");
		}
		_inPass = false;
		_backend.RenderPassEnd();
		if (Direct) {
			_backend.FrameEnd();
		}
	}

	/// <summary>
	///		If the swapchain is a window swapchain.
	/// </summary>
	public bool Direct {
		get => _renderTarget == _backend.GetUltimateRenderTarget();
	}
}
