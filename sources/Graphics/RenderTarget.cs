#region
using Mino.Desktop;
using Mino.Framework;
using Mino.Graphics.Hardware;
using Mino.Graphics.Hardware.Desc;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Represents a render target (a series of textures).
/// </summary>
public class RenderTarget : IDisposable {
	private static RenderTarget? _ultRT;
	private static Lock _lock = new Lock();

	/// <summary>
	///     Gets the ultimate render target.
	/// </summary>
	/// <returns>A render target representing the window.</returns>
	public static RenderTarget GetUltimate() {
		if (_ultRT == null) {
			lock (_lock) {
				_ultRT ??= new RenderTarget();
			}
		}
		return _ultRT;
	}

	private RenderBackend _backend;
	public readonly HandleRef _handle;
	private RenderTargetDesc _desc;
	private bool _disposed;

	public RenderTarget(in RenderTargetDesc desc) {
		// Set userdata.
		_desc = desc;

		// Custom RT.
		_backend = RenderSystem.GetBackend();
		_handle = new HandleRef(_backend.RenderTargetGen());
		_backend.RenderTargetData(_handle, desc);
	}

	private RenderTarget() {
		_backend = RenderSystem.GetBackend();
		// Ult RT.
		_handle = new HandleRef(_backend.GetUltimateRenderTarget());
	}

	/// <summary>
	///     The render target desc.
	/// </summary>
	public RenderTargetDesc Desc {
		get {
			if (IsUltimate) {
				Window win = RenderSystem.GetWindow();

				return new RenderTargetDesc {
					Width = (int) win.Size.X,
					Height = (int) win.Size.Y
					// Other data is lost
					// TODO
				};
			}
			return _desc;
		}
	}

	/// <summary>
	///     Checks if this is the ultimate render target.
	/// </summary>
	public bool IsUltimate {
		get => this == _ultRT;
	}

	public void Dispose() {
		// Cannot delete ult RT.
		if (IsUltimate) {
			return;
		}
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.RenderTargetDelete(_handle);
		GC.SuppressFinalize(this);
	}

	// Implicit cast to native handle.
	public static implicit operator uint(RenderTarget obj) {
		return obj._handle;
	}
}
