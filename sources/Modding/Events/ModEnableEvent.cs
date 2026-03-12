namespace Mino.Modding.Events;

/// <summary>
///		Event: on mod disabled.
/// </summary>
public class ModDisableEvent : Event {
	public string ModId;
	
	public ModDisableEvent(string modId) {
		ModId = modId;
	}
}
