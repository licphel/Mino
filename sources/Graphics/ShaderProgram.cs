#region
using Mino.Framework.Resource;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
using Mino.Nio;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Represents a compiled shader program.
/// </summary>
public interface ShaderProgram : ThreadContextHolder, IDisposable {
	/// <summary>
	///     The shader program desc.
	/// </summary>
	ShaderProgramDesc Desc { get; }

	/// <summary>
	///     Compiles a default vertex-fragment shader program.
	/// </summary>
	/// <param name="vert">Vert shader code.</param>
	/// <param name="frag">Frag shader code.</param>
	/// <returns>A linked program.</returns>
	public static ShaderProgram CreateRender(TextAccess vert, TextAccess frag) {
		ShaderModule vModule = RenderSystem.Create<ShaderModule>(
			new ShaderModuleDesc {
				Type = ShaderType.Vertex,
				Code = vert
				// Output: gl_Position
			});
		ShaderModule fModule = RenderSystem.Create<ShaderModule>(
			new ShaderModuleDesc {
				Type = ShaderType.Fragment,
				Code = frag
				// Output: gl_FragColor
			});
		return RenderSystem.Create<ShaderProgram>(
			new ShaderProgramDesc {
				Modules = [vModule, fModule]
			});
	}
}
