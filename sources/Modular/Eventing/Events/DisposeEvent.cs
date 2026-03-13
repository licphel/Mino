using Mino.Framework;

namespace Mino.Modular.Eventing.Events;

/// <summary>
///		Event: on Executor.OnDispose.
/// </summary>
public class DisposeEvent : Event {
	public readonly Executor Executor;
	
	public DisposeEvent(Executor executor) {
		Executor = executor;
	}
}
