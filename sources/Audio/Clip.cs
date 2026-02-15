#region
using Mino.Audio.AHI;
using Mino.Audio.AHI.Desc;
using Mino.Audio.AHI.Enum;
using Mino.Framework;
#endregion

namespace Mino.Audio;

/// <summary>
///     A re-playable audio clip.
/// </summary>
public class Clip : IDisposable {
	public const float MAX_PITCH = 1024.0F;
	public const float MAX_VOLUME = 1024.0F;

	private AudioBackend _backend;
	public readonly HandleRef _handle;
	private bool _disposed;

	private bool _loop = false;
	private float _pan = 0.0F;
	private float _pitch = 1.0F;
	private int _totalFrames;
	private float _volume = 1.0F;

	/// <summary>
	///     Creates a clip from a clip desc.
	/// </summary>
	/// <param name="desc">The desc of the clip.</param>
	/// <exception cref="Error">If the desc has null data line.</exception>
	public Clip(in ClipDesc desc) {
		Desc = desc;
		if (desc.Line == null) {
			throw new Error("no data line");
		}
		// Generates a native source.
		_backend = AudioSystem.GetBackend();
		_handle = new HandleRef(_backend.ClipGen());
		_backend.ClipData(_handle, desc);

		// Get total frames.
		LineDesc lineDesc = desc.Line.Desc;
		_totalFrames = lineDesc.Data?.Length / lineDesc.FrameBytes ?? 0;

		// Set initial args.
		Volume = 1.0F;
		Pitch = 1.0F;
		Pan = 0.0F;
	}

	/// <summary>
	///     The current status of the clip.
	/// </summary>
	public ClipPlayback Playback {
		get {
			assert();
			_backend.ClipGetProperty(_handle, ClipProperty.Playback, out int playback);
			return (ClipPlayback) playback;
		}
	}

	/// <summary>
	///     The clip desc.
	/// </summary>
	public ClipDesc Desc { get; set; }

	/// <summary>
	///     Controls whether the clip loops.
	/// </summary>
	public bool Looping {
		get => _loop;
		set {
			_loop = value;
			_backend.ClipSetProperty(_handle, ClipProperty.Looping, _loop);
		}
	}

	/// <summary>
	///     The volume of the clip, in [0.0, MAX_VOLUME].
	/// </summary>
	public float Volume {
		get => _volume;
		set {
			assert();
			_volume = Math.Clamp(value, 0.0F, MAX_VOLUME);
			_backend.ClipSetProperty(_handle, ClipProperty.Gain, _volume);
		}
	}

	/// <summary>
	///     The pitch of the clip, in [0.0, MAX_PITCH].
	/// </summary>
	public float Pitch {
		get => _pitch;
		set {
			assert();
			_pitch = Math.Clamp(value, 0.0F, MAX_PITCH);
			_backend.ClipSetProperty(_handle, ClipProperty.Pitch, _pitch);
		}
	}

	/// <summary>
	///     The pan of the clip, in [-1.0, 1.0].
	/// </summary>
	public float Pan {
		get => _pan;
		set {
			assert();
			_pan = Math.Clamp(value, -1.0F, 1.0F);
			_backend.ClipSetProperty(_handle, ClipProperty.Pan, _pan);
		}
	}

	/// <summary>
	///     The playing position of the clip.
	/// </summary>
	public TimeSpan Position {
		get {
			assert();
			_backend.ClipGetProperty(_handle, ClipProperty.FramePosition, out int frames);
			return (double) frames / _totalFrames * Duration;
		}
		set {
			assert();
			double percentage = value.TotalSeconds / Duration.TotalSeconds;
			// Here we clamp the position
			// Do not let it overflow.
			percentage = Math.Clamp(percentage, 0.0, 1.0);
			int frames = (int) (percentage * _totalFrames);
			_backend.ClipSetProperty(_handle, ClipProperty.FramePosition, frames);
		}
	}

	/// <summary>
	///     The real duration of the clip relying on pitch.
	/// </summary>
	public TimeSpan Duration {
		get {
			float pitch = Pitch;
			if (pitch <= 0) {
				throw new DivideByZeroException("Pitch is not positive.");
			}
			if (Desc.Line != null) {
				return Desc.Line.Duration / pitch;
			}
			return TimeSpan.Zero;
		}
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.ClipDelete(_handle);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Plays the clip,
	/// </summary>
	public void Play() {
		assert(ClipPlayback.Inactive);
		// No play end callback.
		// See Emitter.join.
		_backend.ClipPlay(_handle);
	}

	/// <summary>
	///     Stops the clip. This won't dispose it.
	/// </summary>
	public void Stop() {
		assert(ClipPlayback.Active);
		_backend.ClipStop(_handle);
	}

	private void assert(ClipPlayback? playback = null) {
		if (_disposed) {
			throw new Error("disposed");
		}
		if (playback != null && Playback != playback) {
			throw new Error($"playback error: {playback} expected");
		}
	}

	// Implicit cast to native handle.
	public static implicit operator uint(Clip obj) {
		return obj._handle;
	}
}
