namespace Mino.Graphics.Hardware.Desc;

/// <summary>
///     A packed stencil test state.
/// </summary>
public struct StencilDesc {
	public bool StencilTest;
	public byte StencilReadMask;
	public byte StencilWriteMask;
	public StencilFuncDesc Front;
	public StencilFuncDesc Back;

	public StencilDesc() {
		StencilTest = false;
		StencilReadMask = 0;
		StencilWriteMask = 0;
		Front = new StencilFuncDesc();
		Back = new StencilFuncDesc();
	}
}
