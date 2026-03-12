using System.Collections.Concurrent;

namespace Mino.Framework.Registry;

/// <summary>
///		A deferred register of class types.
/// </summary>
public class DeferredRegistry<T> where T : class {
	private ConcurrentDictionary<Identifier, DeferredEntry<T>> _map = new ConcurrentDictionary<Identifier, DeferredEntry<T>>();
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
	/// <param name="key">Object id key.</param>
	/// <param name="t">Object itself.</param>
	/// <returns>A deferred entry.</returns>
	public DeferredEntry<T> Register(Identifier key, T t) {
		key = Identifier.Fallback(_scope, key);
		DeferredEntry<T> entry = _map[key] = new DeferredEntry<T>(() => t, key);
		_pendingTasks.Enqueue(() => {
			entry.Fetch();
		});
		return entry;
	}

	/// <summary>
	///		Executes all pending register requests.
	/// </summary>
	public void ExecuteAll() {
		while (!_pendingTasks.IsEmpty) {
			if (_pendingTasks.TryDequeue(out Action? act)) {
				act();
			}
		}
	}
	
	public T this[in Identifier key] {
		get => _map[key];
	}
}
