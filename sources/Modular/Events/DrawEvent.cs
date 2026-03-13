using Mino.Framework;

namespace Mino.Modular.Events;

/// <summary>
///		Event: on Executor.OnDraw.
/// </summary>
public class DrawEvent : Event {
	public readonly Executor Executor;
	
	public DrawEvent(Executor executor) {
		Executor = executor;
	}
}
