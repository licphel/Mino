namespace Mino.Modding.Mino.Framework.Event;

/// <summary>
/// 事件监听器属性
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SubscribeEventAttribute : Attribute {
	public EventPriority Priority { get; set; } = EventPriority.Normal;
	public bool ReceiveCanceled { get; set; } = false;
}