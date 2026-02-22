#region
using System.Collections.Concurrent;
#endregion

namespace Mino.Utility;

/// <summary>
///     Simple object pool.
/// </summary>
public class Pool<T> where T : class, new() {
	public const int MaxCapacity = 1024;

	private readonly ConcurrentQueue<T> _pool = new ConcurrentQueue<T>();
	private readonly int _maxSize;

	public Pool(int maxSize = MaxCapacity) {
		_maxSize = maxSize;
	}

	/// <summary>
	///		Gets a dirty pooled object.
	/// </summary>
	/// <returns>An object.</returns>
	public T Get() {
		if (_pool.TryDequeue(out T? o)) {
			return o;
		}
		return new T();
	}

	/// <summary>
	///		Returns a object.
	/// </summary>
	/// <param name="o">The object to return.</param>
	public void Return(in T o) {
		if (_pool.Count < _maxSize) {
			_pool.Enqueue(o);
		}
	}

	/// <summary>
	///		A returnable pooled object.
	/// </summary>
	public interface PooledObject {
		void Return();
	}
}
