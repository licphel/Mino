using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;

namespace Mino.Graphics;

/// <summary>
///     Represents a compiled shader module.
/// </summary>
public class ShaderModule : IDisposable {
	private RenderBackend _backend;
	private bool _disposed;
	private uint _handle;

	public ShaderModule(in ShaderModuleDesc desc) {
		// Set userdata.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = _backend.ShaderModuleGen();
		// Compile the module.
		_backend.ShaderModuleCompile(_handle, desc);
	}

	/// <summary>
	///     The shader module desc.
	/// </summary>
	public ShaderModuleDesc Desc { get; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.ShaderModuleDelete(_handle);
		GC.SuppressFinalize(this);
	}

	// Finalizer in case.
	~ShaderModule() {
		Dispose();
	}

	// Implicit cast to native handle.
	public static implicit operator uint(ShaderModule obj) {
		return obj._handle;
	}
}
