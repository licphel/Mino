#region
using Mino.Graphics.RHI.Enum;
#endregion

namespace Mino.Graphics.RHI.Desc;

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
