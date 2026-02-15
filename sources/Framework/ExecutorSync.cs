#region
using System.Diagnostics;
using Mino.Desktop;
using Mino.Input;
#endregion

namespace Mino.Framework;

/// <summary>
///     Synchronous executor with precise timing and accurate FPS/TPS statistics.
///     Uses fixed timestep for logic updates and variable frame rate for rendering.
/// </summary>
public class ExecutorSync : Executor {
	private const double OneNano = 1_000_000_000.0;
	
	private double _lastFixedTickTime;
	private double _lastRenderTime;
	private double _lastStatUpdateTime;
	private double _lastTickTime;
	private int _renderFrameCounter;
	private int _tickFrameCounter;
	private double _timeAccumulator;
	private Stopwatch _timer = new Stopwatch();

	/// <summary>
	///     Starts the application main loop with fixed timestep.
	/// </summary>
	/// <param name="window">The display window.</param>
	/// <param name="tps">Target ticks per second (logic updates).</param>
	/// <param name="fps">Target frames per second. Use -1 for unlimited (VSync).</param>
	public override void Start(Window window, int tps, int fps = -1) {
		if (tps <= 0) {
			throw new Error("negative tps");
		}

		const double MaxFrameTime = 0.25 * OneNano;

		_timer.Start();

		Key.AddListeningThread(Thread.CurrentThread);

		double targetTickInterval = OneNano / tps;
		double targetFrameInterval = fps > 0 ? OneNano / fps : 0.0;
		double currentTime = getCurrentNanos();

		_lastStatUpdateTime = currentTime;
		_lastRenderTime = currentTime;
		_lastTickTime = currentTime;
		_lastFixedTickTime = currentTime;
		
		TimeSpan timeStamp = TimeSpan.Zero;

		while (!window.Closed) {
			currentTime = getCurrentNanos();

			window.ProcessWindowEvents();

			double frameTime = Math.Min(currentTime - _lastTickTime, MaxFrameTime);
			_lastTickTime = currentTime;
			_timeAccumulator += frameTime;

			int ticksThisFrame = 0;
			while (_timeAccumulator >= targetTickInterval) {
				double tickStartTime = getCurrentNanos();
				double diffFixed = tickStartTime - _lastFixedTickTime;
				// Calculate delta from tick diffs.
				double Delta = Math.Clamp(diffFixed / OneNano, 0.0, 0.1);
				OnTick?.Invoke(new TimeStep(timeStamp, Delta));
				_lastFixedTickTime = tickStartTime;

				Key.NextListeningRoll();
				Ticks++;
				timeStamp += TimeSpan.FromSeconds(targetTickInterval / OneNano);

				_timeAccumulator -= targetTickInterval;
				ticksThisFrame++;
				_tickFrameCounter++;

				if (ticksThisFrame >= 4) {
					_timeAccumulator = 0.0;
					Logger.Global.Warn("Falling behind on logic updates.");
					break;
				}
			}

			Partial = Math.Clamp((float) (_timeAccumulator / targetTickInterval), 0.0F, 1.0F);

			bool shouldRender = false;

			if (fps <= 0) {
				shouldRender = true;
			} else {
				double timeSinceLastRender = currentTime - _lastRenderTime;
				if (timeSinceLastRender >= targetFrameInterval) {
					shouldRender = true;
					_lastRenderTime = currentTime;
				}
			}

			if (shouldRender) {
				OnRender?.Invoke();
				_renderFrameCounter++;
			}

			doStatistics(currentTime);

			if (fps > 0) {
				frControl(currentTime, targetFrameInterval);
			} else if (!window.Vsync) {
				yield();
			}
		}

		OnDispose?.Invoke();
	}

	private void doStatistics(double currentTime) {
		const double StatInterval = 0.5 * OneNano;

		double elapsed = currentTime - _lastStatUpdateTime;

		if (elapsed >= StatInterval) {
			double seconds = elapsed / OneNano;

			if (seconds > 0) {
				Fps = (int) (_renderFrameCounter / seconds);
				Tps = (int) (_tickFrameCounter / seconds);
			}

			_renderFrameCounter = 0;
			_tickFrameCounter = 0;
			_lastStatUpdateTime = currentTime;
		}
	}

	private void frControl(double currentTime, double targetFrameInterval) {
		if (targetFrameInterval <= 0) {
			return;
		}

		double timeSinceLastRender = currentTime - _lastRenderTime;
		double timeUntilNextFrame = targetFrameInterval - timeSinceLastRender;

		if (timeUntilNextFrame > 0) {
			double sleepMs = timeUntilNextFrame / 1_000_000.0;

			if (sleepMs > 1.0) {
				Thread.Sleep((int) sleepMs);
			} else if (sleepMs > 0.1) {
				preciseSleep(sleepMs);
			} else if (sleepMs > 0.001) {
				Thread.Sleep(0);
			}
		}
	}

	private static void yield() {
		Thread.Sleep(0);
	}

	private static void preciseSleep(double milliseconds) {
		if (milliseconds <= 0) {
			return;
		}

		long ticks = Stopwatch.Frequency / 1000;
		long targetTicks = (long) (ticks * milliseconds);

		Stopwatch sw = Stopwatch.StartNew();
		while (sw.ElapsedTicks < targetTicks) {
			Thread.SpinWait(8);
		}
	}

	private double getCurrentNanos() {
		return (double) _timer.ElapsedTicks / Stopwatch.Frequency * OneNano;
	}
}
