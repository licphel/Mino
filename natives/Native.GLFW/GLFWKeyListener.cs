using Mino.Graphics.Input;
using static Mino.Graphics.Input.KeyListener;

namespace Mino.Native.GLFW;

public class GLFWKeyListener : KeyListener {
	private GLFWWindow _window;

	public GLFWKeyListener(GLFWWindow window, KeyCode code) {
		_window = window;
		Code = code;
	}

	public KeyCode Code { get; }

	public bool Press {
		get => isThisRollActivated() && Hold;
	}

	public bool Hold {
		get => _window.GetStatus(Code) != KeyStatus.Release;
	}

	public bool Repeat {
		get => _window.GetStatus(Code) == KeyStatus.Repeat;
	}

	public bool With(KeyModifier mod) {
		if (mod == KeyModifier.Any) {
			return true;
		}
		return (_window.GetModifiers(Code) & mod) == mod;
	}

	internal void notify(KeyStatus status) {
		if (status == KeyStatus.Press) {
			foreach (Thread thread in _LISTENER_THREADS) {
				if (!_THREAD_STAMPS.TryGetValue(thread, out bool[]? value)) {
					value = new bool[1024];
					_THREAD_STAMPS[thread] = value;
				}
				value[(int) Code] = true;
			}
		}
	}

	private bool isThisRollActivated() {
		if (_THREAD_STAMPS.TryGetValue(Thread.CurrentThread, out bool[]? map)) {
			return map[(int) Code];
		}
		return false;
	}
}
