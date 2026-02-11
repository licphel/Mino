using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;

namespace Mino.Graphics;

/// <summary>
///     Represents a compiled shader program.
/// </summary>
public class ShaderProgram : IDisposable {
	private RenderBackend _backend;
	private bool _disposed;
	private uint _handle;

	public ShaderProgram(in ShaderProgramDesc desc) {
		// Set userdata.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = _backend.ShaderProgramGen();
		// Compile the module.
		_backend.ShaderProgramLink(_handle, desc);
	}

	/// <summary>
	///     The shader program desc.
	/// </summary>
	public ShaderProgramDesc Desc { get; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.ShaderProgramDelete(_handle);
		GC.SuppressFinalize(this);
	}

	// Finalizer in case.
	~ShaderProgram() {
		Dispose();
	}

	// Implicit cast to native handle.
	public static implicit operator uint(ShaderProgram obj) {
		return obj._handle;
	}
}
