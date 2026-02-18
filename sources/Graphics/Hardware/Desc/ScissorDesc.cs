namespace Mino.Graphics.Hardware.Desc;

/// <summary>
///     Describes a scissor test.
/// </summary>
public struct ScissorDesc {
	public bool Enable;
	public int X;
	public int Y;
	public int Width;
	public int Height;

	public ScissorDesc() {
		Enable = false;
		X = 0;
		Y = 0;
		Width = 0;
		Height = 0;
	}

	public static readonly ScissorDesc Disabled = new ScissorDesc();
}
