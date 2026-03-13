using System.Collections.Concurrent;

namespace Mino.Modular.Registry;

/// <summary>
///		A deferred register of class types.
/// </summary>
public class DeferredRegistry<T> where T : Registerable {
	private ConcurrentDictionary<Identifier, T> _map = new ConcurrentDictionary<Identifier, T>();
	private List<T> _arrMap = new List<T>();
	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	private ConcurrentQueue<Action> _pendingTasks = new ConcurrentQueue<Action>();
	private string _scope;
	private string _tName;
	private int _next = 0;
	
	public DeferredRegistry(string scope) {
		_scope = scope;
		_tName = typeof(T).Name;
	}

	/// <summary>
	///		The reg item count.
	/// </summary>
	public int Count {
		get => _map.Count;
	}

	/// <summary>
	///		Registers an object.
	/// </summary>
	/// <param name="key">Object id key.</param>
	/// <param name="t">Object itself.</param>
	/// <returns>A deferred entry.</returns>
	public DeferredEntry<T> Register(Identifier key, T t) {
		key = Identifier.Fallback(_scope, key);
		t.Id = key;
		t.IntId = _next++;
		_map[key] = t;
		_lock.EnterWriteLock();
		_arrMap.Add(t);
		_lock.ExitWriteLock();
		
		DeferredEntry<T> entry = new DeferredEntry<T>(() => t, key);
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
	
	public T this[int key] {
		get {
			_lock.EnterReadLock();
			T t = _arrMap[key];
			_lock.ExitReadLock();
			return t;
		}
	}
}
