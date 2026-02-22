#region
using Mino.Graphics.Enum;
#endregion

namespace Mino.Graphics.Desc;

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
