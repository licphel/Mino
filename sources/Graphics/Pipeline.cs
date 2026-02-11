using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;

namespace Mino.Graphics;

/// <summary>
///     An immutable list of render states.
/// </summary>
public class Pipeline : IDisposable {
	private RenderBackend _backend;
	private bool _disposed;
	private uint _handle;

	public Pipeline(in PipelineDesc desc) {
		// Set userdata.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = _backend.PipelineGen();
		// Compile the pipeline.
		_backend.PipelineCompile(_handle, desc);
	}

	/// <summary>
	///     The pipeline desc.
	/// </summary>
	public PipelineDesc Desc { get; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.PipelineDelete(_handle);
		GC.SuppressFinalize(this);
	}

	// Finalizer in case.
	~Pipeline() {
		Dispose();
	}

	// Implicit cast to native handle.
	public static implicit operator uint(Pipeline obj) {
		return obj._handle;
	}
}
