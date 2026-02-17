#region
using System.Runtime.CompilerServices;
using Mino.Framework;
using Mino.Graphics.Hardware;
using Mino.Graphics.Hardware.Desc;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Gpu-side auto-expandable buffer object.
/// </summary>
public class BufferObject : IDisposable {
	private RenderBackend _backend;
	public readonly HandleRef _handle;
	private bool _disposed;

	public BufferObject(in BufferDesc desc) {
		// Set userdata.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = new HandleRef(_backend.BufferGen());

		// Initially make it expandable.
		CanExpand = true;
	}

	/// <summary>
	///     The buffer desc.
	/// </summary>
	public BufferDesc Desc { get; set; }

	/// <summary>
	///     Current capacity of the buffer.
	/// </summary>
	public int Capacity { get; private set; }

	/// <summary>
	///     Last transferred data length in bytes.
	/// </summary>
	public int LastBound { get; private set; }

	/// <summary>
	///     Whether the buffer can expand when reaching max capacity.
	/// </summary>
	public bool CanExpand { get; set; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.BufferDelete(_handle);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Reallocates the buffer by given capacity and data (nullable).
	/// </summary>
	/// <param name="capacity">New capacity.</param>
	/// <param name="data">Initial data.</param>
	/// <typeparam name="T">Type generic.</typeparam>
	/// <exception cref="Error">Thrown if capacity is negative.</exception>
	public void Allocate<T>(int capacity, ReadOnlySpan<T> data) where T : unmanaged {
		if (capacity < 0) {
			throw new Error("negative capacity");
		}
		_backend.BufferAlloc(_handle, Desc, data, capacity);
		Capacity = capacity;
		LastBound = 0;
	}

	/// <summary>
	///     Submits a span of data to the buffer.
	/// </summary>
	/// <param name="data">Data span.</param>
	/// <param name="offset">Offset in the buffer.</param>
	/// <typeparam name="T">Type generic.</typeparam>
	/// <exception cref="Error">Thrown if offset is negative or buffer is unexpectedly expanded.</exception>
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
			if (!CanExpand) {
				throw new Error("expand disabled");
			}

			int newcap = Math.Max(bOffset + bCnt, Capacity == 0 ? bCnt : Capacity * 2);
			// Reallocate.
			_backend.BufferAlloc<byte>(_handle, Desc, null, newcap);
			Capacity = newcap;
		}

		_backend.BufferSubmit(_handle, data, offset);
		LastBound = bCnt;
	}

	// Implicit cast to native handle.
	public static implicit operator uint(BufferObject obj) {
		return obj._handle;
	}
}
