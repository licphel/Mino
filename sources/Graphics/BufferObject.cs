#region
using Mino.Framework.Resource;
using Mino.Graphics.Desc;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Gpu-side auto-expandable buffer object.
/// </summary>
public interface BufferObject : ThreadContextHolder, IDisposable {
	/// <summary>
	///     The buffer desc.
	/// </summary>
	BufferObjectDesc Desc { get; }

	/// <summary>
	///     Current capacity of the buffer.
	/// </summary>
	int Capacity { get; }

	/// <summary>
	///     Whether the buffer can expand when reaching max capacity.
	///     By default it is true.
	/// </summary>
	bool CanExpand { get; set; }

	/// <summary>
	///     Reallocates the buffer by given capacity and data (nullable).
	/// </summary>
	/// <param name="capacity">New capacity.</param>
	/// <param name="data">Initial data.</param>
	/// <typeparam name="T">Type generic.</typeparam>
	void Allocate<T>(int capacity, ReadOnlySpan<T> data) where T : unmanaged;

	/// <summary>
	///     Submits a span of data to the buffer.
	/// </summary>
	/// <param name="data">Data span.</param>
	/// <param name="offset">Offset in the buffer.</param>
	/// <typeparam name="T">Type generic.</typeparam>
	void Submit<T>(ReadOnlySpan<T> data, int offset = 0) where T : unmanaged;
}
