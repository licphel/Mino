namespace Mino.Audio;

/// <summary>
///     Line data extractor interface.
/// </summary>
public interface LineReader {
	/// <summary>
	///     Full data of the line.
	/// </summary>
	byte[] Data { get; }

	/// <summary>
	///     Reads a chunk of data from the line.
	/// </summary>
	/// <param name="buffer">Buffer array.</param>
	/// <param name="offset">Reading offset.</param>
	/// <param name="len">Max reading length.</param>
	/// <returns>Actual read length.</returns>
	int Read(byte[] buffer, int offset, int len) {
		byte[] data = Data;
		if (data.Length == 0 || offset >= data.Length) {
			return 0;
		}
		int actual = data.Length - offset;
		actual = Math.Min(len, actual);
		Buffer.BlockCopy(data, offset, buffer, 0, actual);
		return actual;
	}
}
