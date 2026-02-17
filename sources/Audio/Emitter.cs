#region
using Mino.Audio.Hardware.Enum;
#endregion

namespace Mino.Audio;

/// <summary>
///     Sound emitter, manages audios' lifecycle and volume.
/// </summary>
public class Emitter : IDisposable {
	private readonly LinkedList<Clip> _activeClips = new LinkedList<Clip>();
	private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	private List<Emitter> _children = new List<Emitter>();
	private bool _disposed;
	private float _extendedFactor = 1.0F;
	private float _volume = 1.0F;

	public Emitter(string name) {
		Name = name;
	}

	public string Name { get; }

	/// <summary>
	///     Max capacity of the emitter.
	/// </summary>
	public int Capacity { get; set; } = 64;

	/// <summary>
	///     The guard strategy of the emitter.
	/// </summary>
	public EmitterGuard Strategy { get; set; } = EmitterGuard.StopNew;

	/// <summary>
	///     Volume of this emitter.
	/// </summary>
	public float Volume {
		set {
			assert();
			// Set every clip's gain in the emitter.
			_lock.EnterReadLock();
			spreadDown(value);
			_lock.ExitReadLock();
			_volume = value;
		}
		get => _volume;
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		HardGc();
		foreach (Emitter child in _children) {
			child.Dispose();
		}
		GC.SuppressFinalize(this);
	}

	// Finalizer in case.
	~Emitter() {
		Dispose();
	}

	/// <summary>
	///     Stops and dispose all clips.
	/// </summary>
	public void HardGc() {
		assert();
		_lock.EnterWriteLock();
		while (_activeClips.Count > 0) {
			gc(_activeClips.First);
		}
		_lock.ExitWriteLock();
	}

	/// <summary>
	///     Tries to dispose ended clips.
	/// </summary>
	public void Gc() {
		assert();
		bool hasLocked = !_lock.IsWriteLockHeld;
		if (hasLocked) {
			_lock.EnterWriteLock();
		}
		LinkedListNode<Clip>? it = _activeClips.First;
		while (it != null) {
			Clip clip = it.Value;
			if (clip.Playback == ClipPlayback.Inactive) {
				_activeClips.Remove(it);
				// Here it naturally ends.
				clip.Dispose();
			}
			it = it.Next;
		}
		if (hasLocked) {
			_lock.ExitWriteLock();
		}
	}

	/// <summary>
	///     Plays a clip and automatically dispose it.
	/// </summary>
	/// <param name="clip">The clip to play.</param>
	/// <param name="loop">Whether to loop the clip.</param>
	public void Play(Clip clip, bool loop = false) {
		if (loop) {
			clip.Looping = true;
		}
		clip.Play();
		// Join later to avoid this clip dies.
		join(clip);
	}

	/// <summary>
	///     Derives a sub emitter that extends the volume factor.
	/// </summary>
	/// <param name="name">Sub emitter name.</param>
	/// <returns>A sub emitter.</returns>
	public Emitter Derive(string name) {
		assert();
		Emitter emitter = new Emitter(name);
		emitter._extendedFactor = _volume * _extendedFactor;
		_children.Add(emitter);
		return emitter;
	}

	private void spreadDown(float volume) {
		assert();
		foreach (Clip clip in _activeClips) {
			clip.Volume = volume * _extendedFactor;
		}
		foreach (Emitter child in _children) {
			child._extendedFactor = _extendedFactor * volume;
			child.spreadDown(child._volume);
		}
	}

	private void gc(LinkedListNode<Clip>? node) {
		assert();
		Clip? clip = node?.Value;
		if (clip != null) {
			clip.Stop();
			clip.Dispose();
		}
		if (node != null) {
			_activeClips.Remove(node);
		}
	}

	// Reminds the 'gc' of the given clip.
	private void join(Clip clip) {
		assert();
		_lock.EnterWriteLock();
		Gc();

		if (_activeClips.Count >= Capacity) {
			if (Strategy == EmitterGuard.StopNew) {
				gc(_activeClips.Last);
			} else if (Strategy == EmitterGuard.StopOld) {
				gc(_activeClips.First);
			} else {
				// Discard to-play clip.
				return;
			}
		}
		clip.Volume = _volume * _extendedFactor;
		_activeClips.AddLast(clip);
		_lock.ExitWriteLock();
	}

	private void assert() {
		if (_disposed) {
			throw new Error("disposed");
		}
	}
}
