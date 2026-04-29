using System.Collections.Concurrent;
using Mino.Utility.Logging;

namespace Mino.Modular.Registry;

/// <summary>
///		A deferred register of class types.
/// </summary>
public class DeferredRegistry<T> where T : class, RegistryObject {
	private ConcurrentDictionary<Identifier, T> _map = new ConcurrentDictionary<Identifier, T>();
	private List<T> _arrMap = new List<T>();
	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	private Queue<Action> _pendingTasks = new Queue<Action>();
	private Domain _domain;
	private string _tName;
	private T? _defValue = null;
	private int _next = 0;
	private bool _frozen;
	
	public DeferredRegistry(Domain domain) {
		_domain = domain;
		_tName = typeof(T).Name;
	}

	/// <summary>
	///		The reg item count.
	/// </summary>
	public int Count {
		get => _map.Count;
	}
	
	/// <summary>
	///		Registers a default object.
	/// </summary>
	/// <param name="key">Object id key.</param>
	/// <param name="factory">Object factory.</param>
	/// <returns>A deferred entry.</returns>
	public DeferredEntry<T> SetDefault(Identifier key, Func<T> factory) {
		key = Identifier.Fallback(_domain, key);
		
		_lock.EnterWriteLock();
		if (_frozen) {
			throw new RMLException($"Try to register '{key}' after registry is frozen");
		}
		Log.Debug($"Register default {_domain}:{_tName}, key={key}");
		
		DeferredEntry<T> entry = new DeferredEntry<T>(key);
		_pendingTasks.Enqueue(() => insert(entry, key, factory, true));
		
		_lock.ExitWriteLock();
		return entry;
	}

	/// <summary>
	///		Registers an object.
	/// </summary>
	/// <param name="key">Object id key.</param>
	/// <param name="factory">Object factory.</param>
	/// <returns>A deferred entry.</returns>
	public DeferredEntry<T> Register(Identifier key, Func<T> factory) {
		key = Identifier.Fallback(_domain, key);
		
		_lock.EnterWriteLock();
		if (_frozen) {
			throw new RMLException($"Try to register '{key}' after registry is frozen");
		}
		Log.Debug($"Register {_domain}:{_tName}, key={key}");
		
		DeferredEntry<T> entry = new DeferredEntry<T>(key);
		_pendingTasks.Enqueue(() => insert(entry, key, factory, false));
		
		_lock.ExitWriteLock();
		return entry;
	}

	/// <summary>
	///		Executes all pending register requests and stop accepting.
	/// </summary>
	public void Freeze() {
		_lock.EnterWriteLock();

		if (_defValue == null) {
			Log.Fatal($"Null default value for registry {_domain}:{_tName}");
		}
		
		Log.Info($"Registry {_domain}:{_tName} is frozen");
		
		while (_pendingTasks.Count > 0) {
			if (_pendingTasks.TryDequeue(out Action? act)) {
				act();
			}
		}
		_frozen = true;
		_lock.ExitWriteLock();
	}
	
	/// <summary>
	///		Gets an value by an identifier.
	/// </summary>
	public T this[in Identifier key] {
		get => _map.GetValueOrDefault(key, _defValue!);
	}
	
	/// <summary>
	///		Gets an value by an integer ID.
	/// </summary>
	public T this[int key] {
		get {
			_lock.EnterReadLock();
			if (key < 0 || key >= _arrMap.Count) {
				return _defValue!;
			}
			
			T t = _arrMap[key];
			_lock.ExitReadLock();
			return t;
		}
	}

	// Inserts an entry.
	private void insert(DeferredEntry<T> entry, in Identifier key, Func<T> factory, bool isDef) {
		T t = factory.Invoke();
		t.Freeze(key, _next++);
		if (_map.ContainsKey(key)) {
			throw new RMLException($"Duplicated key: {key}");
		}
		_map[key] = t;
		
		if (isDef) {
			_defValue = t;
		} else {
			_arrMap.Add(t);
		}
		
		entry.Value = t;
	}
}
