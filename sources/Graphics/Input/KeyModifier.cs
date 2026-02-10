namespace Mino.Graphics.Input;

/// <summary>
///     Standard key modifiers.
/// </summary>
[Flags]
public enum KeyModifier {
	None = 0x0000,
	Shift = 0x0001,
	Control = 0x0002,
	Alt = 0x0004,
	Super = 0x0008,
	CapsLock = 0x0010,
	NumsLock = 0x0020,
	Any = -1 // Any modifier is acceptable.
}
