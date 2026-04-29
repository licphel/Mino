using System.Collections.Concurrent;
using Mino.Graphics;

namespace Mino.Input;

/// <summary>
///		A recommended way of handling input.
///		Input snapshot records a frame's input list.
/// </summary>
public readonly struct InputSnapshot {
	private static readonly ConcurrentBag<Thread> _listenerThreads = new ConcurrentBag<Thread>();
	private static readonly ConcurrentDictionary<Thread, bool[]> _threadStamps =
		new ConcurrentDictionary<Thread, bool[]>();
	private static bool _isEventHooked;
	
	private readonly bool[] _keyMap;
	
	public InputSnapshot() {
		if (!_isEventHooked) {
			RenderSystem.GetWindow().KeyEvent += keyCallback;
			_isEventHooked = true;
		}
		
		Thread curt = Thread.CurrentThread;
		if (!_listenerThreads.Contains(curt)) {
			throw new SnapshotFailedException($"Strange thread: {curt.Name}");
		}

		// Put if absent.
		if (_threadStamps.TryGetValue(curt, out bool[]? map)) {
			_keyMap = map;
		} else {
			_threadStamps[curt] = _keyMap = new bool[1024];
		}
	}

	/// <summary>
	///		Returns whether the key is active.
	/// </summary>
	/// <param name="keycode">Keycode of the key.</param>
	/// <returns>Whether the key is active.</returns>
	public bool IsActive(uint keycode) {
		return _keyMap[keycode];
	}
	
	/// <summary>
	///     Enable a thread to listener managed key events.
	/// </summary>
	/// <param name="thread">Listener thread.</param>
	public static void AddListeningThread(Thread thread) {
		_listenerThreads.Add(thread);
	}

	/// <summary>
	///     Ends a listening roll.
	/// </summary>
	public static void NextListeningRoll() {
		foreach(bool[] map in _threadStamps.Values) {
			Array.Fill(map, false);
		}
	}

	// Window key event callback:
	// Notify current thread keys.
	internal static void keyCallback(uint keycode, uint modifier, KeyStatus status) {
		if (status == KeyStatus.Press) {
			foreach (Thread thread in _listenerThreads) {
				if (!_threadStamps.TryGetValue(thread, out bool[]? value)) {
					value = new bool[1024];
					_threadStamps[thread] = value;
				}
				value[(int) keycode] = true;
			}
		}
	}
}
