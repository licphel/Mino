namespace Mino.Modular.Eventing.Events;

/// <summary>
///		Event: on mod changes.
/// </summary>
public sealed class ModEvent : Event {
	public readonly string ModId;
	// "e" - enabling
	// "d" - disabling
	public string Op;
	
	public ModEvent(string modId, string op) {
		ModId = modId;
		Op = op;
	}
}
