#region
using Mino.Audio.Enum;
#endregion

namespace Mino.Audio;

/// <summary>
///     Sound emitter, manages audios' lifecycle and volume.
/// </summary>
public class Emitter : IDisposable {
	/// <summary>
	///     Guard strategy when an emitter reaches its capacity.
	/// </summary>
	public enum GuardStrategy {
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

	private readonly LinkedList<Clip> _activeClips = new LinkedList<Clip>();
	private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	private List<Emitter> _children = new List<Emitter>();
	private float _extendedFactor = 1.0F;
	private float _volume = 1.0F;
	private bool _disposed;

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
	public GuardStrategy Strategy { get; set; } = GuardStrategy.StopNew;

	/// <summary>
	///     Volume of this emitter.
	/// </summary>
	public float Volume {
		set {
			// Set every clip's gain in the emitter.
			_lock.EnterReadLock();
			spreadDown(value);
			_lock.ExitReadLock();
			_volume = value;
		}
		get => _volume;
	}

	/// <summary>
	///     Stops and dispose all clips.
	/// </summary>
	public void HardGc() {
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
		Emitter emitter = new Emitter(name);
		emitter._extendedFactor = _volume * _extendedFactor;
		_children.Add(emitter);
		return emitter;
	}

	private void spreadDown(float volume) {
		foreach (Clip clip in _activeClips) {
			clip.Volume = volume * _extendedFactor;
		}
		foreach (Emitter child in _children) {
			child._extendedFactor = _extendedFactor * volume;
			child.spreadDown(child._volume);
		}
	}

	private void gc(LinkedListNode<Clip>? node) {
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
		_lock.EnterWriteLock();
		Gc();

		if (_activeClips.Count >= Capacity) {
			if (Strategy == GuardStrategy.StopNew) {
				gc(_activeClips.Last);
			} else if (Strategy == GuardStrategy.StopOld) {
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

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		HardGc();
		foreach (Emitter child in _children) {
			child.Dispose();
		}
	}
}
