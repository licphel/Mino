using Mino.Audio.AHI;
using Mino.Audio.AHI.Desc;
using Mino.Framework;

namespace Mino.Audio;

/// <summary>
///     Represents an audio data line.
/// </summary>
public class Line : LineReader, IDisposable {
	private AudioBackend _backend;
	private bool _disposed;
	internal uint _handle;

	/// <summary>
	///     Creates a data line from a data line desc.
	/// </summary>
	/// <param name="desc">The desc of the line.</param>
	public Line(in LineDesc desc) {
		Desc = desc;

		_backend = AudioSystem.GetBackend();
		_handle = _backend.LineGen();
		_backend.LineData(_handle, desc);
	}

	/// <summary>
	///     The buffer desc.
	/// </summary>
	public LineDesc Desc { get; }

	/// <summary>
	///     The derived clip duration when pitch is normal (1.0F).
	/// </summary>
	public TimeSpan Duration {
		get => Desc.Duration;
	}
	
	/// <summary>
	///		Data of the line.
	/// </summary>
	public byte[] Data {
		get => Desc.Data ?? Array.Empty<byte>();
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.LineDelete(_handle);
		GC.SuppressFinalize(this);
	}
	
	[NotRecommended]
	public uint GetBackendHandle() {
		return _handle;
	}

	// Finalizer in case.
	~Line() {
		Dispose();
	}
}
