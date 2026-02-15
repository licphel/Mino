#region
using Mino.Graphics.RHI.Enum;
#endregion

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     A packed stencil operation state.
/// </summary>
public struct StencilFuncDesc {
	public StencilFunc FailFunc;
	public StencilFunc PassFunc;
	public StencilFunc DepthFailFunc;
	public CompareOp CompareOp;

	public StencilFuncDesc() {
		FailFunc = default;
		PassFunc = default;
		DepthFailFunc = default;
		CompareOp = default;
	}
}
