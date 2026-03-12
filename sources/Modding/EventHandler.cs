namespace Mino.Modding;

internal interface EventHandler {
	void Invoke(Event @event);
	Type EventType { get; }
	EventPriority Priority { get; }
	object? Target { get; }
}