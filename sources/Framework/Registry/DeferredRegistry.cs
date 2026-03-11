using System.Collections.Concurrent;
using Mino.Nio;

namespace Mino.Framework.Registry;

/// <summary>
///		A deferred register of class types.
/// </summary>
public class DeferredRegistry<T> where T : class {
	private ConcurrentDictionary<Url, DeferredEntry<T>> _map = new ConcurrentDictionary<Url, DeferredEntry<T>>();
	private ConcurrentQueue<Action> _pendingTasks = new ConcurrentQueue<Action>();
	private string _scope;
	private string _tName;
	
	public DeferredRegistry(string scope) {
		_scope = scope;
		_tName = typeof(T).Name;
	}

	/// <summary>
	///		Registers an object.
	/// </summary>
	/// <param name="key">Object url key.</param>
	/// <param name="t">Object itself.</param>
	/// <returns>A deferred entry.</returns>
	public DeferredEntry<T> Register(in Url key, T t) {
		DeferredEntry<T> entry = _map[key] = new DeferredEntry<T>(() => t, key);
		_pendingTasks.Enqueue(() => {
			entry.Fetch();
		});
		return entry;
	}
	
	public T this[in Url key] {
		get => _map[key];
	}
}
