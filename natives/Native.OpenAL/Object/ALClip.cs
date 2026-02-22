#region
using Mino.Audio;
using Mino.Audio.Desc;
using Mino.Audio.Enum;
using Mino.Framework.Resource;
using Silk.NET.OpenAL;
#endregion

namespace Mino.Native.OpenAL.Object;

public sealed class ALClip : Clip {
	public AL _al = null!;
	public ALContext _ctx = null!;
	public uint _handle;
	public bool _disposed;
	
	public ClipDesc _desc;
	public volatile float _volume = 1.0F;
	public volatile float _pan = 0.0F;
	public volatile float _pitch = 1.0F;
	public volatile bool _loop = false;
	
	/*
	 * Tracked properties
	 */
	public volatile int _alRawPlayback = (int) SourceState.Initial;
	public volatile float _alSecOffset = 0.0F;

	[ResourceCreation]
	public ALClip(in ClipDesc desc) {
		_desc = desc;
	}

	public ClipPlayback Playback {
		get => ALEnumC.Cast((SourceState) _alRawPlayback);
	}

	public ClipDesc Desc {
		get => _desc;
	}

	public bool Looping {
		get => _loop;
		set {
			_loop = value;
			_ctx.Pend(() => {
				_al.SetSourceProperty(_handle, SourceBoolean.Looping, value);
			});
		}
	}

	public float Volume {
		get => _volume;
		set {
			_volume = Math.Clamp(value, 0.0F, Clip.MaxVolume);
			_ctx.Pend(() => {
				_al.SetSourceProperty(_handle, SourceFloat.Gain, _volume);
			});
		}
	}
	
	public float Pitch {
		get => _pitch;
		set {
			_pitch = Math.Clamp(value, 0.0F, Clip.MaxPitch);
			_ctx.Pend(() => {
				_al.SetSourceProperty(_handle, SourceFloat.Pitch, _pitch);
			});
		}
	}
	
	public float Pan {
		get => _pan;
		set {
			_pan = Math.Clamp(value, 0.0F, Clip.MaxVolume);
			_ctx.Pend(() => {
				// Try 'Stereo Panning' extension.
				if (_ctx._ext_StereoPanning) {
					_al.SetSourceProperty(_handle, (SourceFloat) 0x1005, _pan);
				} else {
					// Simulate by position.
					// This won't work with stereo sounds.
					float panZ = -MathF.Sqrt(1.0F - _pan * _pan);
					_al.SetSourceProperty(_handle, SourceVector3.Position, _pan, 0.0F, panZ);
				}
			});
		}
	}

	public TimeSpan Position {
		get => TimeSpan.FromSeconds(_alSecOffset);
		set {
			float secs = Math.Clamp((float) value.TotalSeconds, 0.0F, (float) Duration.TotalSeconds);
			_ctx.Pend(() => {
				_al.SetSourceProperty(_handle, SourceFloat.SecOffset, secs);
			});
		}
	}

	public TimeSpan Duration {
		get {
			if (_pitch <= 1E-5F) {
				return TimeSpan.MaxValue;
			}
			return _desc.Line?.Duration ?? TimeSpan.Zero / _pitch;
		}
	}

	public void Play() {
		if (Playback != ClipPlayback.Inactive) {
			return;
		}
		_ctx.Pend(() => {
			_al.SourcePlay(_handle);
			_ctx._trackingList.AddLast(this);
		});
	}
	
	public void Stop() {
		if (Playback != ClipPlayback.Active) {
			return;
		}
		_ctx.Pend(() => {
			_al.SourceStop(_handle);
			_ctx._trackingList.Remove(this);
		});
	}
	
	public bool TryGetThreadContext(out ThreadContext ctx) {
		ctx = _ctx;
		return true;
	}
	
	public void Listen(ThreadContext ctx) {
		_ctx = (ALContext) ctx;
		_al = _ctx._al;
		
		_ctx.Pend(() => {
			_handle = _al.GenSource();
			
			if (_desc.Line == null) {
				return;
			}
			
			// Set buffer data.
			ALDataLine srcL = (ALDataLine) _desc.Line;
			_al.SetSourceProperty(_handle, SourceInteger.Buffer, srcL._handle);
		});
	}
	
	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		
		_ctx.Pend(() => {
			_al.DeleteSource(_handle);
			_ctx._trackingList.Remove(this);
		});
	}
}
