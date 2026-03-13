#region
using Mino.Audio.Enum;
using Mino.Utility;
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
			_ => throw new Crash("Invalid arg: " + nameof(state))
		};
	}

	public static BufferFormat Cast(DataLineFormat format) {
		return format switch {
			DataLineFormat.Mono8 => BufferFormat.Mono8,
			DataLineFormat.Mono16 => BufferFormat.Mono16,
			DataLineFormat.Stereo8 => BufferFormat.Stereo8,
			DataLineFormat.Stereo16 => BufferFormat.Stereo16,
			_ => throw new Crash("Invalid arg: " + nameof(format))
		};
	}
}
