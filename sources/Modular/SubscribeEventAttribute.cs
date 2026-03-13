namespace Mino.Modular;

/// <summary>
///		Marks a static method is a subscriber.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SubscribeEventAttribute : Attribute {
	/// <summary>
	///		The event subscriber priority.
	/// </summary>
	public EventPriority Priority { get; set; } = EventPriority.Normal;
	
	/// <summary>
	///		Whether to receive canceled events.
	/// </summary>
	public bool ReceiveCanceled { get; set; } = false;
}