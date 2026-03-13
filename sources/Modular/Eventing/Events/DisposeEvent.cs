using Mino.Framework;

namespace Mino.Modular.Eventing.Events;

/// <summary>
///		Event: on Executor.OnDispose.
/// </summary>
public sealed class DisposeEvent : Event {
	public readonly Executor Executor;
	
	public DisposeEvent(Executor executor) {
		Executor = executor;
	}
}
