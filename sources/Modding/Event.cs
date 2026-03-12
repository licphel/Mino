namespace Mino.Modding;

/// <summary>
///		Event class.
/// </summary>
public abstract class Event {
	/// <summary>
	///		Whether the event is canceled.
	/// </summary>
	public bool Canceled { get; set; }
	
	/// <summary>
	///		Whether the event is handled.
	/// </summary>
	public bool Handled { get; set; }
}