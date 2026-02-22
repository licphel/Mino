#region
using Mino.Audio.Desc;
using Mino.Framework.Resource;
#endregion

namespace Mino.Audio;

/// <summary>
///     Represents an audio data line.
/// </summary>
public interface DataLine : ThreadContextHolder, IDisposable {
	/// <summary>
	///     The data line desc.
	/// </summary>
	public DataLineDesc Desc { get; }

	/// <summary>
	///     The derived clip duration when pitch is normal (1.0F).
	/// </summary>
	public TimeSpan Duration {
		get => Desc.Duration;
	}

	/// <summary>
	///     Source data of the line.
	/// </summary>
	public byte[] Data {
		get => Desc.Data ?? Array.Empty<byte>();
	}
}
