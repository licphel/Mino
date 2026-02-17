using Mino.Audio;
using Mino.Audio.Hardware.Desc;

namespace Mino.Graphics.Gui;

/// <summary>
///		Static GUI configurations.
/// </summary>
public static class GuiSystem {
	public static Emitter SoundEmitter { get; set; } = new Emitter("gui");

	/// <summary>
	///		Plays a sound in GUI emitter.
	/// </summary>
	/// <param name="line">Source line.</param>
	public static void PlaySound(Line? line) {
		if (line == null) {
			return;
		}
		SoundEmitter.Play(new Clip(new ClipDesc {
			Line = line
		}));
	}
}
