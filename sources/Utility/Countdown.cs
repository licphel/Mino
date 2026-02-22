#region
using Mino.Framework;
#endregion

namespace Mino.Utility;

/// <summary>
///     Countdown utility.
/// </summary>
public class Countdown {
	/// <summary>
	///     CD remaining time.
	/// </summary>
	public TimeSpan RemainingTime { get; private set; }

	/// <summary>
	///     Whether the CD is ready.
	/// </summary>
	public bool Ready {
		get => RemainingTime <= TimeSpan.Zero;
	}

	/// <summary>
	///     Updates the countdown.
	/// </summary>
	/// <param name="step">Timestep.</param>
	public void Update(TimeStep step) {
		RemainingTime -= TimeSpan.FromSeconds(step.Delta);
	}

	/// <summary>
	///     Pushes a CD time.
	/// </summary>
	/// <param name="span">The time to countdown.</param>
	public void Push(in TimeSpan span) {
		RemainingTime = span;
	}
}
