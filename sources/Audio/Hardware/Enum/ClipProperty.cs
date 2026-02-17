namespace Mino.Audio.Hardware.Enum;

/// <summary>
///     Clip properties.
/// </summary>
public enum ClipProperty {
	Gain, // float, set-only
	Pitch, // float, set-only
	Pan, // float, set-only
	Playback, // int, set, get
	FramePosition, // int, set, get
	Looping // bool, set-only
}
