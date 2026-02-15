#region
using Mino.Framework;
using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Represents a compiled shader module.
/// </summary>
public class ShaderModule : IDisposable {
	private RenderBackend _backend;
	public readonly HandleRef _handle;
	private bool _disposed;

	public ShaderModule(in ShaderModuleDesc desc) {
		// Set userdata.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = new HandleRef(_backend.ShaderModuleGen());
		// Compile the module.
		_backend.ShaderModuleCompile(_handle, desc);
	}

	/// <summary>
	///     The shader module desc.
	/// </summary>
	public ShaderModuleDesc Desc { get; set; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.ShaderModuleDelete(_handle);
		GC.SuppressFinalize(this);
	}

	// Implicit cast to native handle.
	public static implicit operator uint(ShaderModule obj) {
		return obj._handle;
	}
}
