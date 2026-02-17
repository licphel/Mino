#region
using Mino.Graphics.Hardware.Enum;
#endregion

namespace Mino.Graphics.Hardware.Desc;

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
