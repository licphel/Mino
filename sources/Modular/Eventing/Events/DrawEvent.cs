using Mino.Framework;

namespace Mino.Modular.Eventing.Events;

/// <summary>
///		Event: on Executor.OnDraw.
/// </summary>
public sealed class DrawEvent : Event {
	public readonly Executor Executor;
	
	public DrawEvent(Executor executor) {
		Executor = executor;
	}
}
