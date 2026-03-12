namespace Mino.Modding;

/// <summary>
///		Marks a static method is a subscriber.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SubscribeEventAttribute : Attribute {
	public EventPriority Priority { get; set; } = EventPriority.Normal;
	public bool ReceiveCanceled { get; set; } = false;
}