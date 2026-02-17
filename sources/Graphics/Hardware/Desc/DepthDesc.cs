#region
using Mino.Graphics.Hardware.Enum;
#endregion

namespace Mino.Graphics.Hardware.Desc;

/// <summary>
///     A packed depth test state.
/// </summary>
public struct DepthDesc {
	public bool DepthTest;
	public bool DepthWrite;
	public CompareOp DepthCompare;

	public DepthDesc() {
		DepthTest = false;
		DepthWrite = false;
		DepthCompare = CompareOp.Never;
	}


	/// <summary>
	///     DepthTest = true, DepthWrite = true, 'LEQ' Comparison.
	/// </summary>
	public static readonly DepthDesc Leq = new DepthDesc {
		DepthTest = true,
		DepthWrite = true,
		DepthCompare = CompareOp.LessOrEqual
	};

	/// <summary>
	///     DepthTest = true, DepthWrite = true, 'GEQ' Comparison.
	/// </summary>
	public static readonly DepthDesc Geq = new DepthDesc {
		DepthTest = true,
		DepthWrite = true,
		DepthCompare = CompareOp.GreaterOrEqual
	};

	/// <summary>
	///     DepthTest = false, DepthWrite = false.
	/// </summary>
	public static readonly DepthDesc Disabled = new DepthDesc {
		DepthTest = false,
		DepthWrite = false
	};
}
