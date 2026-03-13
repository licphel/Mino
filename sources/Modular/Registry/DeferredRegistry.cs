using System.Collections.Concurrent;
using Mino.Utility;

namespace Mino.Modular.Registry;

/// <summary>
///		A deferred register of class types.
/// </summary>
public class DeferredRegistry<T> where T : class, Registerable {
	private ConcurrentDictionary<Identifier, T> _map = new ConcurrentDictionary<Identifier, T>();
	private List<T> _arrMap = new List<T>();
	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	private Queue<Action> _pendingTasks = new Queue<Action>();
	private string _scope;
	private string _tName;
	private int _next = 0;
	private bool _frozen;
	
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
	/// <param name="factory">Object factory.</param>
	/// <returns>A deferred entry.</returns>
	public DeferredEntry<T> Register(Identifier key, Func<T> factory) {
		_lock.EnterWriteLock();

		if (_frozen) {
			throw new Crash($"Try to register '{key}' after registry is frozen");
		}
		
		key = Identifier.Fallback(_scope, key);
		DeferredEntry<T> entry = new DeferredEntry<T>(key);
		_pendingTasks.Enqueue(() => {
			T t = factory.Invoke();
			t.Id = key;
			t.IntId = _next++;
			_map[key] = t;
			_arrMap.Add(t);
			entry.Value = t;
		});
		_lock.ExitWriteLock();
		return entry;
	}

	/// <summary>
	///		Executes all pending register requests and stop accepting.
	/// </summary>
	public void Freeze() {
		_lock.EnterWriteLock();
		while (_pendingTasks.Count > 0) {
			if (_pendingTasks.TryDequeue(out Action? act)) {
				act();
			}
		}
		_frozen = true;
		_lock.ExitWriteLock();
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
