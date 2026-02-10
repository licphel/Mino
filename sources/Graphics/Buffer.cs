using System.Runtime.CompilerServices;
using Mino.Framework;
using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;

namespace Mino.Graphics;

public class Buffer : IDisposable {
	private RenderBackend _backend;
	private bool _disposed;
	private uint _handle;

	public Buffer(in BufferDesc desc) {
		_backend = RenderSystem.GetBackend();
		_handle = _backend.BufferGen();
		// Set userdata.
		Desc = desc;
	}

	public BufferDesc Desc { get; }

	/// <summary>
	///     Current capacity of the buffer.
	/// </summary>
	public int Capacity { get; private set; }

	/// <summary>
	///     Last transferred data length in bytes.
	/// </summary>
	public int LastBound { get; private set; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.BufferDelete(_handle);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Submits a span of data to the buffer.
	/// </summary>
	/// <param name="data">Data span.</param>
	/// <param name="offset">Offset in the buffer.</param>
	/// <typeparam name="T">Type generic.</typeparam>
	/// <exception cref="Error">Thrown if offset is negative.</exception>
	public void Submit<T>(ReadOnlySpan<T> data, int offset = 0) where T : unmanaged {
		if (data.IsEmpty) {
			LastBound = 0;
			return;
		}
		if (offset < 0) {
			throw new Error("negative offset");
		}

		int stride = Unsafe.SizeOf<T>();
		int bCnt = data.Length * stride;
		int bOffset = offset * stride;

		// Need to expand.
		if (bOffset + bCnt > Capacity) {
			int newcap = Math.Max(bOffset + bCnt, Capacity == 0 ? bCnt : Capacity * 2);

			// Reallocate.
			_backend.BufferAlloc<byte>(_handle, Desc, null, newcap);
			Capacity = newcap;
		}

		_backend.BufferSubmit(_handle, data, offset);
		LastBound = bCnt;
	}

	[NotRecommended]
	public uint GetBackendHandle() {
		return _handle;
	}
}
