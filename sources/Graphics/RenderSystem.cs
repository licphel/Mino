#region
using Mino.Desktop;
using Mino.Framework;
using Mino.Framework.Resource;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Global render system.
/// </summary>
public class RenderSystem {
	private static ThreadContext? _ctx;
	private static Window? _window;
	private static Lock _lock = new Lock();

	/// <summary>
	///     Loads a native render context.
	/// </summary>
	/// <param name="window">A native window.</param>
	/// <param name="context">Backend interface.</param>
	/// <exception cref="Error">If there's already a context.</exception>
	public static void LoadContext(Window window, ThreadContext context) {
		lock (_lock) {
			_ctx = context;
			_window = window;
			_ctx.Init();
		}
	}

	/// <summary>
	///     Gets current render context.
	/// </summary>
	/// <exception cref="Error">Thrown if there's no render context.</exception>
	public static ThreadContext Context {
		get {
			lock (_lock) {
				return _ctx ?? throw new Error("render context not loaded");
			}
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

	// A fast delegate to the resource factory.
	public static I Create<I>(params object[] args) {
		return Context.Factory.Create<I>(args);
	}
}
