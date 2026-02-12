using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;

namespace Mino.Graphics;

/// <summary>
///     An immutable list of render states.
/// </summary>
public class RenderPipe : IDisposable {
	private RenderBackend _backend;
	private bool _disposed;
	private uint _handle;

	public RenderPipe(in RenderPipeDesc desc) {
		// Set userdata.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = _backend.RenderPipeGen();
		// Compile the pipe.
		_backend.RenderPipeCompile(_handle, desc);
	}

	/// <summary>
	///     The pipe desc.
	/// </summary>
	public RenderPipeDesc Desc { get; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.RenderPipeDelete(_handle);
		GC.SuppressFinalize(this);
	}

	// Finalizer in case.
	~RenderPipe() {
		Dispose();
	}

	// Implicit cast to native handle.
	public static implicit operator uint(RenderPipe obj) {
		return obj._handle;
	}
}
