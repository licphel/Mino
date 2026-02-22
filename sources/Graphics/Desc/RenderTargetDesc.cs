#region
using Mino.Graphics.Enum;
#endregion

namespace Mino.Graphics.Desc;

/// <summary>
///     Describes a render target.
/// </summary>
public struct RenderTargetDesc {
	public bool IsUltimate;
	public int Width;
	public int Height;
	public TextureFormat[] ColorAttachments;
	public TextureFormat? DepthStencilAttachment;
	public int Samples;

	public RenderTargetDesc() {
		IsUltimate = false;
		Width = 0;
		Height = 0;
		ColorAttachments = [];
		DepthStencilAttachment = null;
		Samples = 1;
	}
}
