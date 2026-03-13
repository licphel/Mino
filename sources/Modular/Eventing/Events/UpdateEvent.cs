using Mino.Framework;

namespace Mino.Modular.Eventing.Events;

/// <summary>
///		Event: on Executor.OnUpdate.
/// </summary>
public sealed class UpdateEvent : Event {
	public readonly Executor Executor;
	public readonly TimeStep Step;
	
	public UpdateEvent(Executor executor, TimeStep step) {
		Step = step;
		Executor = executor;
	}
}
