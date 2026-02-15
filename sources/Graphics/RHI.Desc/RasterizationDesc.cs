#region
using Mino.Graphics.RHI.Enum;
#endregion

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     A packed rasterization state.
/// </summary>
public struct RasterizationDesc {
	public PolygonMode PolygonMode;
	public CullMode CullMode;
	public FrontFace FrontFace;
	public bool DepthBiasEnable;
	public float DepthBiasConstantFactor;
	public float DepthBiasClamp;
	public float DepthBiasSlopeFactor;

	public RasterizationDesc() {
		PolygonMode = PolygonMode.Fill;
		CullMode = CullMode.None;
		FrontFace = FrontFace.CounterClockwise;
		DepthBiasEnable = false;
		DepthBiasConstantFactor = 0.0F;
		DepthBiasClamp = 0.0F;
		DepthBiasSlopeFactor = 0.0F;
	}

	/// <summary>
	///     Default rasterizer, CCW front and filled polygon.
	/// </summary>
	public static readonly RasterizationDesc Default = new RasterizationDesc {
		PolygonMode = PolygonMode.Fill,
		CullMode = CullMode.Back,
		FrontFace = FrontFace.CounterClockwise,
		DepthBiasEnable = false
	};

	/// <summary>
	///     No-culling rasterizer, CW front and filled polygon.
	/// </summary>
	public static readonly RasterizationDesc NotCull = new RasterizationDesc {
		PolygonMode = PolygonMode.Fill,
		CullMode = CullMode.None,
		FrontFace = FrontFace.CounterClockwise,
		DepthBiasEnable = false
	};
}
