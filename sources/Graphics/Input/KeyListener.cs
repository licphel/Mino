using System.Collections.Concurrent;
using Mino.Graphics.Desktop;
using Mino.Graphics.RHI;

namespace Mino.Graphics.Input;

/// <summary>
///     Provides key input observation.
/// </summary>
public class KeyListener {
	private static readonly ConcurrentBag<Thread> _LISTENER_THREADS = new ConcurrentBag<Thread>();
	private static readonly ConcurrentDictionary<Thread, bool[]> _THREAD_STAMPS =
		new ConcurrentDictionary<Thread, bool[]>();
	private static readonly ConcurrentDictionary<KeyCode, KeyListener> _ACTIVE_LISTENERS =
		new ConcurrentDictionary<KeyCode, KeyListener>();
	private static bool _isEventHooked;

	private Window _window;

	// Private ctor - we use key listeners for better performance.
	private KeyListener(KeyCode code) {
		_window = RenderSystem.GetWindow();
		Code = code;

		if (!_isEventHooked) {
			_window.KeyEvent += keyCallback;
			_isEventHooked = true;
		}
	}

	/// <summary>
	///     Key code of the listener.
	/// </summary>
	public KeyCode Code { get; }

	/// <summary>
	///     The key is just pressed.
	/// </summary>
	public bool Press {
		get => isThisRollActivated() && Hold;
	}

	/// <summary>
	///     The key is being held.
	/// </summary>
	public bool Hold {
		get => _window.GetStatus(Code) != KeyStatus.Release;
	}

	/// <summary>
	///     The key is repeating.
	/// </summary>
	public bool Repeat {
		get => _window.GetStatus(Code) == KeyStatus.Repeat;
	}

	/// <summary>
	///     The key is just pressed or repeating.
	/// </summary>
	private bool React {
		get => Press || Repeat;
	}

	/// <summary>
	///     Returns if the key is pressed with given modifiers.
	/// </summary>
	/// <param name="mod">Modifiers, can be 'Any'.</param>
	/// <returns>True if the modifier combination is applied.</returns>
	public bool With(KeyModifier mod) {
		if (mod == KeyModifier.Any) {
			return true;
		}
		return (_window.GetModifiers(Code) & mod) == mod;
	}

	private bool isThisRollActivated() {
		if (_THREAD_STAMPS.TryGetValue(Thread.CurrentThread, out bool[]? map)) {
			return map[(int) Code];
		}
		return false;
	}
	
	/// <summary>
	///     Gets a key listener instance.
	/// </summary>
	/// <param name="code">Key code.</param>
	/// <returns>A cached key listener.</returns>
	public static KeyListener Get(KeyCode code) {
		if (_ACTIVE_LISTENERS.TryGetValue(code, out KeyListener? value)) {
			return value;
		}
		return _ACTIVE_LISTENERS[code] = new KeyListener(code);
	}

	/// <summary>
	///     Enable a thread to listener managed key events.
	/// </summary>
	/// <param name="thread">Listener thread.</param>
	public static void AddListeningThread(Thread thread) {
		_LISTENER_THREADS.Add(thread);
	}

	/// <summary>
	///     Ends a listening roll.
	/// </summary>
	public static void NextListeningRoll() {
		if (_THREAD_STAMPS.TryGetValue(Thread.CurrentThread, out bool[]? map)) {
			Array.Fill(map, false);
		}
	}

	// Window key event callback:
	// Notify current thread keys.
	private static void keyCallback(KeyCode keyCode, KeyModifier modifier, KeyStatus status) {
		if (status == KeyStatus.Press) {
			foreach (Thread thread in _LISTENER_THREADS) {
				if (!_THREAD_STAMPS.TryGetValue(thread, out bool[]? value)) {
					value = new bool[1024];
					_THREAD_STAMPS[thread] = value;
				}
				value[(int) keyCode] = true;
			}
		}
	}
}
