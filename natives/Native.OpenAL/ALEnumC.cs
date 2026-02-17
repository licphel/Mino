#region
using Mino.Audio.Hardware.Enum;
using Silk.NET.OpenAL;
#endregion

namespace Mino.Native.OpenAL;

internal static class ALEnumC {
	public static ClipPlayback Cast(SourceState state) {
		return state switch {
			SourceState.Initial => ClipPlayback.Inactive,
			SourceState.Playing => ClipPlayback.Active,
			SourceState.Paused => ClipPlayback.Inactive,
			SourceState.Stopped => ClipPlayback.Inactive,
			_ => throw new Error("invalid arg: " + nameof(state))
		};
	}

	public static BufferFormat Cast(LineFormat format) {
		return format switch {
			LineFormat.Mono8 => BufferFormat.Mono8,
			LineFormat.Mono16 => BufferFormat.Mono16,
			LineFormat.Stereo8 => BufferFormat.Stereo8,
			LineFormat.Stereo16 => BufferFormat.Stereo16,
			_ => throw new Error("invalid arg: " + nameof(format))
		};
	}
}
