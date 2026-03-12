namespace Mino.Modding.Events;

/// <summary>
///		Event: on mod enabled.
/// </summary>
public class ModEnableEvent : Event {
	public readonly string ModId;
	
	public ModEnableEvent(string modId) {
		ModId = modId;
	}
}
