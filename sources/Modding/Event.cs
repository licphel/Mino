namespace Mino.Modding.Mino.Framework.Event;

/// <summary>
/// 普通事件基类
/// </summary>
public abstract class Event : IEvent {
	public bool IsCanceled { get; set; }
	public bool IsHandled { get; set; }
}