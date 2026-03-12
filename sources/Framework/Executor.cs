#region
using Mino.Desktop;
#endregion

namespace Mino.Framework;

/// <summary>
///     Provides lifecycle and event management.
/// </summary>
public abstract class Executor {
	/// <summary>
	///     Current tps.
	/// </summary>
	public int Tps { get; protected set; }

	/// <summary>
	///     Current fps.
	/// </summary>
	public int Fps { get; protected set; }

	/// <summary>
	///     Tick timestamp.
	/// </summary>
	public long Ticks { get; protected set; }

	/// <summary>
	///     Rendering partial tick used to lerp.
	/// </summary>
	public float Partial { get; protected set; }

	/// <summary>
	///     Current time step.
	/// </summary>
	public TimeStep Step { get; protected set; }

	/// <summary>
	///     Called on logic ticks.
	/// </summary>
	public Action<TimeStep>? OnUpdate { get; set; }

	/// <summary>
	///     Called on render ticks.
	/// </summary>
	public Action? OnDraw { get; set; }

	/// <summary>
	///     Called on dispose stage.
	/// </summary>
	public Action? OnDispose { get; set; }

	/// <summary>
	///     Starts the app lifecycle.
	/// </summary>
	/// <param name="window">The displaying window.</param>
	/// <param name="tps">Ticks per second.</param>
	/// <param name="fps">Frames per second. 0 or negative means limitless.</param>
	public abstract void Start(Window window, int tps, int fps = -1);
}
