using Mino.Graphics.RHI.Enum;
using Mino.Mathematics;

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     Describes a render pass.
/// </summary>
public struct RenderPassDesc {
	public Color4f ClearColor;
	public double ClearDepth;
	public int ClearStencil;
	public ClearMask Clear;

	public RenderPassDesc() {
		ClearColor = Color4f.PureBlack;
		ClearDepth = 0.0;
		ClearStencil = 0;
		Clear = ClearMask.Color | ClearMask.Depth | ClearMask.Stencil;
	}
}
