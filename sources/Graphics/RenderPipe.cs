using Mino.Framework;
using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;

namespace Mino.Graphics;

/// <summary>
///     An immutable list of render states.
/// </summary>
public class RenderPipe : IDisposable {
	private RenderBackend _backend;
	public readonly HandleRef _handle;
	private bool _disposed;

	public RenderPipe(in RenderPipeDesc desc) {
		// Set userdata.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = new HandleRef(_backend.RenderPipeGen());
		// Compile the pipe.
		_backend.RenderPipeCompile(_handle, desc);
	}

	/// <summary>
	///     The pipe desc.
	/// </summary>
	public RenderPipeDesc Desc { get; set; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.RenderPipeDelete(_handle);
		GC.SuppressFinalize(this);
	}

	// Implicit cast to native handle.
	public static implicit operator uint(RenderPipe obj) {
		return obj._handle;
	}
}
