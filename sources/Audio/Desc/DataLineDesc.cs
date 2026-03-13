#region
using Mino.Audio.Enum;
using Mino.Nio;
using Mino.Utility;
using NAudio.Wave;
#endregion

namespace Mino.Audio.Desc;

/// <summary>
///     Describes an audio data line.
/// </summary>
public record struct DataLineDesc {
	public int BitsPerSample;
	public int BlockAlign;
	public byte[]? Data;
	public TimeSpan Duration;
	public DataLineFormat Format;
	public int NumChannels;
	public int SampleRate;

	public DataLineDesc() {
		BitsPerSample = 0;
		BlockAlign = 0;
		Data = null;
		Duration = TimeSpan.Zero;
		Format = default;
		NumChannels = 0;
		SampleRate = 0;
	}

	/// <summary>
	///     Frame byte size.
	/// </summary>
	public int FrameBytes {
		get => BitsPerSample / 8 * NumChannels;
	}

	/// <summary>
	///     Parses an audio data line from a byte buffer.
	/// </summary>
	/// <param name="buffer">an untouched byte buffer</param>
	/// <returns>A parsed audio data.</returns>
	public static DataLineDesc Parse(ByteBuffer buffer) {
		MemoryStream input = new MemoryStream(buffer.BufferArray);
		DataLineDesc sd = new DataLineDesc();
		// Currently we only support WAVE.
		// To support other formats we'd better detect the buffer header,
		// But now it's enough.
		WaveStream waveStream = new WaveFileReader(input);
		sd.Data = new byte[waveStream.Length];
		waveStream.ReadExactly(sd.Data, 0, sd.Data.Length);

		sd.Format = mapToSoundFormat(
			waveStream.WaveFormat.Channels,
			waveStream.WaveFormat.BitsPerSample
		);
		sd.SampleRate = waveStream.WaveFormat.SampleRate;
		sd.Duration = waveStream.TotalTime;
		sd.BitsPerSample = waveStream.WaveFormat.BitsPerSample;
		sd.NumChannels = waveStream.WaveFormat.Channels;
		sd.BlockAlign = waveStream.WaveFormat.BlockAlign;
		return sd;
	}

	// Maps audio data args to audio format.
	private static DataLineFormat mapToSoundFormat(int channels, int bitsPerSample) {
		return (channels, bitsPerSample) switch {
			(1, 8) => DataLineFormat.Mono8,
			(1, 16) => DataLineFormat.Mono16,
			(2, 8) => DataLineFormat.Stereo8,
			(2, 16) => DataLineFormat.Stereo16,
			_ => throw new Crash(
				$"Unsupported PCM WAVE format: {channels}ch {bitsPerSample}bit")
		};
	}
}
