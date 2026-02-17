#region
using Mino.Audio.Hardware;
using Mino.Audio.Hardware.Desc;
using Mino.Framework;
#endregion

namespace Mino.Audio;

/// <summary>
///     Represents an audio data line.
/// </summary>
public class Line : LineReader, IDisposable {
	private AudioBackend _backend;
	public readonly HandleRef _handle;
	private bool _disposed;

	/// <summary>
	///     Creates a data line from a data line desc.
	/// </summary>
	/// <param name="desc">The desc of the line.</param>
	public Line(in LineDesc desc) {
		Desc = desc;

		_backend = AudioSystem.GetBackend();
		_handle = new HandleRef(_backend.LineGen());
		_backend.LineData(_handle, desc);
	}

	/// <summary>
	///     The buffer desc.
	/// </summary>
	public LineDesc Desc { get; set; }

	/// <summary>
	///     The derived clip duration when pitch is normal (1.0F).
	/// </summary>
	public TimeSpan Duration {
		get => Desc.Duration;
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.LineDelete(_handle);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Data of the line.
	/// </summary>
	public byte[] Data {
		get => Desc.Data ?? Array.Empty<byte>();
	}

	[NotRecommended]
	public uint GetBackendHandle() {
		return _handle;
	}

	// Implicit cast to native handle.
	public static implicit operator uint(Line obj) {
		return obj._handle;
	}
}
