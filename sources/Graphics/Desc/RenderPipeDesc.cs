#region
using Mino.Graphics.Enum;
#endregion

namespace Mino.Graphics.Desc;

/// <summary>
///     Describes a pipe (an immutable list of render states).
/// </summary>
public struct RenderPipeDesc {
	public RenderPipeUsage Usage;
	public BlendDesc Blend;
	public StencilDesc Stencil;
	public DepthDesc Depth;
	public RasterizationDesc Rasterization;
	public ShaderProgram? ShaderProgram;
	public VertexLayout VertexLayout;
	public ResourceSetLayout[] ResourceLayouts;

	public RenderPipeDesc() {
		Usage = default;
		Blend = new BlendDesc();
		Stencil = new StencilDesc();
		Depth = new DepthDesc();
		Rasterization = new RasterizationDesc();
		ShaderProgram = null;
		VertexLayout = VertexLayout.Bake();
		ResourceLayouts = [];
	}
}
