#region
using Mino.Graphics.Enum;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Desc;

/// <summary>
///     Describes a render pass.
/// </summary>
public struct RenderPassDesc {
	public Color ClearColor;
	public double ClearDepth;
	public int ClearStencil;
	public ClearMask Clear;

	public RenderPassDesc() {
		ClearColor = Color.PureBlack;
		ClearDepth = 1.0;
		ClearStencil = 0;
		Clear = ClearMask.Color | ClearMask.Depth | ClearMask.Stencil;
	}

	public static readonly RenderPassDesc Default = new RenderPassDesc();
}
