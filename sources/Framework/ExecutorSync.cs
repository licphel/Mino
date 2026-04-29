#region
using System.Diagnostics;
using Mino.Desktop;
using Mino.Input;
using Mino.Modular.Eventing;
using Mino.Modular.Eventing.Events;
using Mino.Utility.Logging;
#endregion

namespace Mino.Framework;

/// <summary>
///     Synchronous executor with precise timing and accurate FPS/TPS statistics.
///     Uses fixed timestep for logic updates and variable frame rate for rendering.
/// </summary>
public class ExecutorSync : Executor {
	private const double OneNano = 1_000_000_000.0;
	private const long TicksPerSecond = 10_000_000;
	private Stopwatch _stopwatch = new Stopwatch();

	/// <summary>
	///     Starts the application main loop with fixed timestep.
	/// </summary>
	/// <param name="window">The display window.</param>
	/// <param name="tps">Target ticks per second (logic updates).</param>
	/// <param name="fps">Target frames per second. Use -1 for unlimited (VSync).</param>
	public override void Start(Window window, int tps, int fps = -1) {
		if (tps <= 0) {
			throw new ArgumentException("Tps must be > 0");
		}
		
		_stopwatch.Start();
		InputSnapshot.AddListeningThread(Thread.CurrentThread);
		
		long tickInterval = TicksPerSecond / tps;
		long frameInterval = fps > 0 ? TicksPerSecond / fps : 0;

		long previousTick = _stopwatch.ElapsedTicks;
		long previousFrame = _stopwatch.ElapsedTicks;
		long previousStat = _stopwatch.ElapsedTicks;

		int tickCount = 0, frameCount = 0;
		TimeSpan gameTime = TimeSpan.Zero;

		try {
			while (!window.Closed) {
				long current = _stopwatch.ElapsedTicks;

				window.ProcessWindowEvents();

				// Fixed step ticking.
				while (current - previousTick >= tickInterval) {
					previousTick += tickInterval;

					double delta = (double) tickInterval / TicksPerSecond;
					OnUpdate?.Invoke(new TimeStep(gameTime, delta));
					EventBus.Instance.Post(new UpdateEvent(this, Step = new TimeStep(gameTime, delta)));

					gameTime += TimeSpan.FromSeconds(delta);
					tickCount++;
					Ticks++;

					InputSnapshot.NextListeningRoll();

					if (current - previousTick >= tickInterval * 4) {
						previousTick = current - tickInterval;
						Log.Warn("Tick falls behind");
						break;
					}
				}

				Partial = (float) ((current - previousTick) / (double) tickInterval);

				// Rendering stage.
				bool shouldRender = frameInterval == 0 || current - previousFrame >= frameInterval;
				if (shouldRender) {
					previousFrame = current;
					OnDraw?.Invoke();
					EventBus.Instance.Post(new DrawEvent(this));
					frameCount++;
				}

				// Stats.
				if (current - previousStat >= TicksPerSecond / 2) {
					Fps = frameCount * 2;
					Tps = tickCount * 2;
					frameCount = tickCount = 0;
					previousStat = current;
				}

				// Frame control.
				if (frameInterval > 0) {
					long nextFrameTime = previousFrame + frameInterval;
					long sleepTicks = nextFrameTime - _stopwatch.ElapsedTicks;

					if (sleepTicks > 0) {
						double sleepMs = sleepTicks * 1000.0 / TicksPerSecond;
						if (sleepMs > 2.0) {
							Thread.Sleep((int) (sleepMs - 1));
						}
						while (_stopwatch.ElapsedTicks < nextFrameTime) {
							Thread.SpinWait(1);
						}
					}
				} else if (!window.Vsync) {
					Thread.Sleep(0);
				}
			}
		} catch (Exception ex) {
			/*gi
			 * Crashes will terminate the game loop.
			 * We handle them for logging stacktrace.
			 */
			Log.Fatal(ex);
		}

		try {
			OnDispose?.Invoke();
			EventBus.Instance.Post(new DisposeEvent(this));
		} catch {
			// Ignored
		}
	}
}
