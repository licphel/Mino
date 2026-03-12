using Mino.Framework;

namespace Mino.Modding.Events;

/// <summary>
///		Event: on Executor.OnUpdate.
/// </summary>
public class UpdateEvent : Event {
	public readonly Executor Executor;
	public readonly TimeStep Step;
	
	public UpdateEvent(Executor executor, TimeStep step) {
		Step = step;
		Executor = executor;
	}
}
