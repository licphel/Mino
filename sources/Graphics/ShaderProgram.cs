using Mino.Framework;
using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;

namespace Mino.Graphics;

/// <summary>
///     Represents a compiled shader program.
/// </summary>
public class ShaderProgram : IDisposable {
	private RenderBackend _backend;
	public readonly HandleRef _handle;
	private bool _disposed;

	public ShaderProgram(in ShaderProgramDesc desc) {
		// Set userdata.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = new HandleRef(_backend.ShaderProgramGen());
		// Compile the module.
		_backend.ShaderProgramLink(_handle, desc);
	}

	/// <summary>
	///     The shader program desc.
	/// </summary>
	public ShaderProgramDesc Desc { get; set; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.ShaderProgramDelete(_handle);
		GC.SuppressFinalize(this);
	}

	// Implicit cast to native handle.
	public static implicit operator uint(ShaderProgram obj) {
		return obj._handle;
	}

	/// <summary>
	///		Compiles a default vertex-fragment shader program.
	/// </summary>
	/// <param name="vert">Vert shader code.</param>
	/// <param name="frag">Frag shader code.</param>
	/// <returns>A linked program.</returns>
	public static ShaderProgram FragVert(string vert, string frag) {
		ShaderModule vModule = new ShaderModule(
			new ShaderModuleDesc {
				Type = ShaderType.Vertex,
				Code = vert
				// Output: gl_Position
			});
		ShaderModule fModule = new ShaderModule(
			new ShaderModuleDesc {
				Type = ShaderType.Fragment,
				Code = frag
				// Output: gl_FragColor
			});
		return new ShaderProgram(
			new ShaderProgramDesc {
				Modules = [vModule, fModule]
			});
	}
}
