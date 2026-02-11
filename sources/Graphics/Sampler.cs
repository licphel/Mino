using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;

namespace Mino.Graphics;

/// <summary>
///		Represents a texture sampler.
/// </summary>
public class Sampler : IDisposable {
	private RenderBackend _backend;
	private bool _disposed;
	private uint _handle;

	public Sampler(in SamplerDesc desc) {
		// Set userdata.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = _backend.SamplerGen();
		// Compile the sampler.
		_backend.SamplerData(_handle, desc);
	}

	/// <summary>
	///     The sampler desc.
	/// </summary>
	public SamplerDesc Desc { get; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.SamplerDelete(_handle);
		GC.SuppressFinalize(this);
	}

	// Finalizer in case.
	~Sampler() {
		Dispose();
	}

	// Implicit cast to native handle.
	public static implicit operator uint(Sampler obj) {
		return obj._handle;
	}
}
