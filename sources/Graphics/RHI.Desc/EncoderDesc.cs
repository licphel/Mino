#region
using Mino.Graphics.RHI.Enum;
#endregion

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     Describes a command encoder.
/// </summary>
public struct EncoderDesc {
	public bool IsExtended;
	public EncoderUsage Usage;

	public EncoderDesc() {
		IsExtended = false;
		Usage = default;
	}
}
