using Mino.Framework;

namespace Mino.Audio.AHI;

/// <summary>
///     Global audio system.
/// </summary>
public static class AudioSystem {
	private static AudioBackend? _backend;
	private static Lock _lock = new Lock();

	/// <summary>
	///     Loads a native audio binding.
	/// </summary>
	/// <param name="backend">Backend interface.</param>
	/// <exception cref="Error">If there's already a binding.</exception>
	public static void LoadBackend(AudioBackend backend) {
		lock (_lock) {
			_backend = backend;
			_backend.Init();
		}
	}

	/// <summary>
	///     Gets current audio binding.
	/// </summary>
	/// <returns>The current audio binding.</returns>
	/// <exception cref="Error">If there's no audio binding.</exception>
	public static AudioBackend GetBackend() {
		lock (_lock) {
			return _backend ?? throw new Error("audio binding not loaded");
		}
	}

	/// <summary>
	///     Updates the audio system.
	/// </summary>
	/// <param name="step">Update fixed step.</param>
	public static void Update(FixedStep step) {
		lock (_lock) {
			if (_backend == null) {
				return;
			}
			_backend.PollEvents();
		}
	}
}
