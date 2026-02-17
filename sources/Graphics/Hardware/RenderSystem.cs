#region
using Mino.Desktop;
using Mino.Framework;
#endregion

namespace Mino.Graphics.Hardware;

/// <summary>
///     Global render system.
/// </summary>
public class RenderSystem {
	private static RenderBackend? _backend;
	private static Window? _window;
	private static Lock _lock = new Lock();

	/// <summary>
	///     Loads a native render backend.
	/// </summary>
	/// <param name="window">A native window.</param>
	/// <param name="backend">Backend interface.</param>
	/// <exception cref="Error">If there's already a backend.</exception>
	public static void LoadBackend(Window window, RenderBackend backend) {
		lock (_lock) {
			_backend = backend;
			_window = window;
			_backend.Init(window);
		}
	}

	/// <summary>
	///     Gets current render backend.
	/// </summary>
	/// <returns>The current render backend.</returns>
	/// <exception cref="Error">Thrown if there's no render backend.</exception>
	public static RenderBackend GetBackend() {
		lock (_lock) {
			return _backend ?? throw new Error("render backend not loaded");
		}
	}

	/// <summary>
	///     Gets current window.
	/// </summary>
	/// <returns>The current window.</returns>
	/// <exception cref="Error">Thrown if there's no window.</exception>
	public static Window GetWindow() {
		lock (_lock) {
			return _window ?? throw new Error("window not loaded");
		}
	}

	/// <summary>
	///     Updates the render system.
	/// </summary>
	/// <param name="step">Update fixed step.</param>
	public static void Update(TimeStep step) {
		lock (_lock) {
			if (_backend == null) {
				return;
			}
			_backend.PollEvents();
		}
	}
}
