namespace Mino.Audio;

/// <summary>
///     Guard strategy when an emitter reaches its capacity.
/// </summary>
public enum EmitterGuard {
	/// <summary>
	///     Stop the oldest clip and let the joining one in.
	/// </summary>
	StopOld,
	/// <summary>
	///     Stop the newest clip and let the joining one in.
	/// </summary>
	StopNew,
	/// <summary>
	///     Stop the joining clip.
	/// </summary>
	StopJoin
}
