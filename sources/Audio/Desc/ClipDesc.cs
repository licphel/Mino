namespace Mino.Audio.Desc;

/// <summary>
///     Describes an audio clip.
/// </summary>
public record struct ClipDesc {
	public DataLine? Line;

	public ClipDesc() {
	}

	/// <summary>
	///     Creates a clip desc from a present data line.
	/// </summary>
	/// <param name="line">Source data line.</param>
	/// <returns>A clip desc.</returns>
	public static ClipDesc FromDataLine(DataLine line) {
		return new ClipDesc {
			Line = line
		};
	}
}
