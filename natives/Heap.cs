namespace Mino;

public class Heap<T> {
	private const uint InvalidIndex = 0;
	private const int InitialCapacity = 16;
	private const int MaxCapacity = 0x7FFFFFFF;

	private T[] _data;
	private uint[] _dense;
	private int _freeCapacity;
	private int _freeCount;
	private uint[] _freeList;
	private uint[] _sparse;

	public Heap(int initialCapacity = InitialCapacity) {
		if (initialCapacity <= 0) {
			initialCapacity = InitialCapacity;
		}

		Capacity = initialCapacity;
		_sparse = new uint[Capacity];
		_dense = new uint[Capacity];
		_data = new T[Capacity];
		_freeList = new uint[Capacity];

		Count = 0;
		_freeCount = 0;
		_freeCapacity = Capacity;
	}

	public int Count { get; private set; }
	public int Capacity { get; private set; }

	public uint Allocate() {
		uint handle;

		if (_freeCount > 0) {
			handle = _freeList[--_freeCount];
		} else {
			handle = (uint) Count + 1;

			if (handle >= _sparse.Length) {
				GrowSparse();
			}
		}

		if (Count >= Capacity) {
			GrowDense();
		}

		uint denseIndex = (uint) Count;
		_sparse[handle] = denseIndex + 1;
		_dense[Count] = handle;

		Count++;
		return handle;
	}

	public uint Allocate(in T data) {
		uint handle = Allocate();
		SetData(handle, data);
		return handle;
	}

	public void SetData(uint handle, in T data) {
		uint denseIndex = GetDenseIndex(handle);
		_data[denseIndex] = data;
	}

	public ref T GetData(uint handle) {
		uint denseIndex = GetDenseIndex(handle);
		return ref _data[denseIndex];
	}

	public void Free(uint handle) {
		if (!IsValid(handle)) {
			throw new Error("invalid handle");
		}

		uint denseIndex = _sparse[handle] - 1;
		uint lastDenseIndex = (uint) (Count - 1);

		if (denseIndex != lastDenseIndex) {
			uint lastHandle = _dense[lastDenseIndex];

			_dense[denseIndex] = lastHandle;
			_data[denseIndex] = _data[lastDenseIndex];
			_sparse[lastHandle] = denseIndex + 1;
		}

		// Disable null warning.
		ref T refData = ref _data[lastDenseIndex];
		// Dispose if needed.
		if (refData is IDisposable disposable) {
			disposable.Dispose();
		}
		refData = default!;
		Count--;

		_sparse[handle] = InvalidIndex;

		if (_freeCount >= _freeCapacity) {
			GrowFreeList();
		}
		_freeList[_freeCount++] = handle;
	}

	public bool IsValid(uint handle) {
		if (handle == 0 || handle >= (uint) _sparse.Length) {
			return false;
		}

		uint denseIndex = _sparse[handle];
		return denseIndex != InvalidIndex && denseIndex <= (uint) Count;
	}

	public void ForEach(Action<uint, T> action) {
		for (int i = 0; i < Count; i++) {
			action(_dense[i], _data[i]);
		}
	}

	private uint GetDenseIndex(uint handle) {
		if (handle == 0 || handle >= (uint) _sparse.Length) {
			throw new Error($"invalid handle: {handle}", nameof(handle));
		}

		uint denseIndex = _sparse[handle];
		if (denseIndex == InvalidIndex || denseIndex > (uint) Count) {
			throw new Error($"freed or corrupted handle: {handle}", nameof(handle));
		}

		return denseIndex - 1;
	}

	private void GrowSparse() {
		int newSize = Math.Min(_sparse.Length * 2, MaxCapacity);
		Array.Resize(ref _sparse, newSize);
	}

	private void GrowDense() {
		int newCapacity = Math.Min(Capacity * 2, MaxCapacity);

		Array.Resize(ref _dense, newCapacity);
		Array.Resize(ref _data, newCapacity);

		Capacity = newCapacity;
	}

	private void GrowFreeList() {
		_freeCapacity = Math.Min(_freeCapacity * 2, MaxCapacity);
		Array.Resize(ref _freeList, _freeCapacity);
	}

	public void Clear() {
		Array.Clear(_sparse, 0, _sparse.Length);
		Array.Clear(_data, 0, _data.Length);
		Count = 0;
		_freeCount = 0;
	}
}
