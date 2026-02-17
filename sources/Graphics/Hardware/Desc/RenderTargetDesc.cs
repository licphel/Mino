#region
using Mino.Graphics.Hardware.Enum;
#endregion

namespace Mino.Graphics.Hardware.Desc;

/// <summary>
///     Describes a render target.
/// </summary>
public struct RenderTargetDesc {
	public int Width;
	public int Height;
	public TextureFormat[] ColorAttachments;
	public TextureFormat? DepthStencilAttachment;
	public int Samples;

	public RenderTargetDesc() {
		Width = 0;
		Height = 0;
		ColorAttachments = [];
		DepthStencilAttachment = null;
		Samples = 1;
	}
}
