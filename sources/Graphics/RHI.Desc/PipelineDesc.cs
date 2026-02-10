using Mino.Graphics.RHI.Enum;

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     Describes a pipeline (an immutable list of render states).
/// </summary>
public struct PipelineDesc {
	public PipelineType Type;
	public BlendDesc Blend;
	public StencilDesc Stencil;
	public DepthDesc Depth;
	public RasterizationDesc Rasterization;
	public uint ShaderProgram;
	public VertexLayout VertexLayout;
	public ResourceSetLayout[] ResourceLayouts;

	public PipelineDesc() {
		Type = default;
		Blend = new BlendDesc();
		Stencil = new StencilDesc();
		Depth = new DepthDesc();
		Rasterization = new RasterizationDesc();
		ShaderProgram = 0;
		VertexLayout = VertexLayout.Bake();
		ResourceLayouts = [];
	}
}
