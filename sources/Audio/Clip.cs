#region
using Mino.Audio.Desc;
using Mino.Audio.Enum;
using Mino.Framework.Resource;
#endregion

namespace Mino.Audio;

/// <summary>
///     A re-playable audio clip.
/// </summary>
public interface Clip : ThreadContextHolder, IDisposable {
	public const float MaxPitch = 1024.0F;
	public const float MaxVolume = 1024.0F;

	/// <summary>
	///     The current status of the clip.
	/// </summary>
	ClipPlayback Playback { get; }

	/// <summary>
	///     The clip desc.
	/// </summary>
	ClipDesc Desc { get; }

	/// <summary>
	///     Controls whether the clip loops.
	/// </summary>
	bool Looping { get; set; }

	/// <summary>
	///     The volume of the clip, in [0.0, MAX_VOLUME].
	/// </summary>
	float Volume { get; set; }

	/// <summary>
	///     The pitch of the clip, in [0.0, MAX_PITCH].
	/// </summary>
	float Pitch { get; set; }

	/// <summary>
	///     The pan of the clip, in [-1.0, 1.0].
	/// </summary>
	float Pan { get; set; }

	/// <summary>
	///     The playing position of the clip.
	/// </summary>
	TimeSpan Position { get; set; }

	/// <summary>
	///     The real duration of the clip relying on pitch.
	/// </summary>
	TimeSpan Duration { get; }

	/// <summary>
	///     Plays the clip,
	/// </summary>
	void Play();

	/// <summary>
	///     Stops the clip. This won't dispose it.
	/// </summary>
	void Stop();
}
