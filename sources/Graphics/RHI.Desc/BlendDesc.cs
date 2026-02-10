using Mino.Graphics.RHI.Enum;
using Mino.Mathematics;

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     A packed blend state, allows separate control of RGB/Alpha.
/// </summary>
public struct BlendDesc {
	public bool Enable;
	public BlendFactor SrcColor;
	public BlendFactor DstColor;
	public BlendFunc ColorFunc;
	public BlendFactor SrcAlpha;
	public BlendFactor DstAlpha;
	public BlendFunc AlphaFunc;
	public Color4f Constant;

	public BlendDesc() {
		Enable = false;
		SrcColor = default;
		DstColor = default;
		ColorFunc = default;
		SrcAlpha = default;
		DstAlpha = default;
		AlphaFunc = default;
		Constant = Color4f.PureWhite;
	}

	/// <summary>
	///     Disabled blend.
	/// </summary>
	public static readonly BlendDesc Disabled = new BlendDesc {
		Enable = false
	};

	/// <summary>
	///     Standard alpha-mix blend state.
	/// </summary>
	public static readonly BlendDesc AlphaMix = new BlendDesc {
		Enable = true,
		SrcColor = BlendFactor.SrcAlpha,
		DstColor = BlendFactor.OneMinusSrcAlpha,
		ColorFunc = BlendFunc.Add,
		SrcAlpha = BlendFactor.One,
		DstAlpha = BlendFactor.OneMinusSrcAlpha,
		AlphaFunc = BlendFunc.Add
	};

	/// <summary>
	///     Standard additive blend state.
	/// </summary>
	public static readonly BlendDesc Additive = new BlendDesc {
		Enable = true,
		SrcColor = BlendFactor.SrcAlpha,
		DstColor = BlendFactor.One,
		ColorFunc = BlendFunc.Add,
		SrcAlpha = BlendFactor.One,
		DstAlpha = BlendFactor.One,
		AlphaFunc = BlendFunc.Add
	};
}
