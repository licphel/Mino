using System.Collections.Concurrent;

namespace Mino.Graphics.Input;

/// <summary>
///     Provides key input observation.
/// </summary>
public interface KeyListener {
	internal static readonly ConcurrentBag<Thread> _LISTENER_THREADS = new ConcurrentBag<Thread>();
	internal static readonly ConcurrentDictionary<Thread, bool[]> _THREAD_STAMPS =
		new ConcurrentDictionary<Thread, bool[]>();

	/// <summary>
	///     The key is just pressed.
	/// </summary>
	bool Press { get; }

	/// <summary>
	///     The key is being held.
	/// </summary>
	bool Hold { get; }

	/// <summary>
	///     The key is repeating.
	/// </summary>
	bool Repeat { get; }

	/// <summary>
	///     The key is just pressed or repeating.
	/// </summary>
	bool React {
		get => Press || Repeat;
	}

	/// <summary>
	///     Returns if the key is pressed with given modifiers.
	/// </summary>
	/// <param name="mod">Modifiers, can be 'Any'.</param>
	/// <returns>True if the modifier combination is applied.</returns>
	bool With(KeyModifier mod);

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
}
