namespace Mino.Modular.Eventing;

public delegate void EventFn<in T>(T @event) where T : Event;

/// <summary>
///		An event handler.
/// </summary>
public interface EventHandler {
	/// <summary>
	///		Event class type.
	/// </summary>
	Type EventType { get; }
	
	/// <summary>
	///		Event priority.
	/// </summary>
	EventPriority Priority { get; }
	
	/// <summary>
	///		Accepts an event.
	/// </summary>
	/// <param name="event">Accepted event.</param>
	void Invoke(Event @event);
}