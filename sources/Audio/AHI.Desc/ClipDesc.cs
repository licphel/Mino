namespace Mino.Audio.AHI.Desc;

/// <summary>
///     Describes an audio clip.
/// </summary>
public record struct ClipDesc {
	public Line Line;

	public ClipDesc() {
		// Actually we will assert line != null at clip init
		// So null check is useless.
		Line = null!;
	}
}
