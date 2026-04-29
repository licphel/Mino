using Mino.Framework.Resource;

namespace Mino.Audio;

/// <summary>
///     Global audio system.
/// </summary>
public static class AudioSystem {
	private static ThreadContext? _ctx;
	private static Lock _lock = new Lock();

	/// <summary>
	///     Loads a native audio context.
	/// </summary>
	/// <param name="backend">Backend interface.</param>
	/// <exception cref="InvalidOperationException">If there's already a context.</exception>
	public static void LoadContext(ThreadContext backend) {
		lock (_lock) {
			_ctx = backend;
			_ctx.Init();
		}
	}

	/// <summary>
	///     Gets current audio context.
	/// </summary>
	/// <exception cref="NullReferenceException">If there's no audio context.</exception>
	public static ThreadContext Context {
		get {
			lock (_lock) {
				return _ctx ?? throw new NullReferenceException("Audio context not loaded");
			}
		}
	}

	// A fast delegate to the resource factory.
	public static I Create<I>(params object[] args) {
		return Context.Factory.Create<I>(args);
	}
}
