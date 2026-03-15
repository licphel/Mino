using Mino.Utility;

namespace Mino.Modular.Resource;

/// <summary>
///		Holder notifier for hot-reload.
/// </summary>
public class HolderNotifier {
	internal object? _object;
	private Action<object?, object?>? _onChanged;
	private Type? _resourceType;
	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		
	/// <summary>
	///		Object ref of holder notifier.
	/// </summary>
	public object? Obj {
		get {
			_lock.EnterReadLock();
			object? ret = _object;
			_lock.ExitReadLock();
			return ret;
		}
	}

	/// <summary>
	///		Notifies an asset change.
	/// </summary>
	/// <param name="obj">New value.</param>
	/// <exception cref="Crash">Thrown if type does not match.</exception>
	public void Notify(object? obj) {
		_lock.EnterWriteLock();
		try {
			if (_object == obj) {
				return;
			}
			if (obj != null) {
				Type newType = obj.GetType();
				_resourceType ??= newType;

				if (obj != null && _resourceType != newType) {
					throw new Crash($"Asset type does not match: old={_resourceType}, new={newType}");
				}
			}

			_onChanged?.Invoke(_object, obj);
			_object = obj;
		} finally {
			_lock.ExitWriteLock();
		}
	}

	/// <summary>
	///		Hooks a listener for asset change.
	/// </summary>
	/// <param name="onChanged">Action that will be invoked on change.</param>
	public void Listen(Action<object?, object?> onChanged) {
		_lock.EnterWriteLock();
		_onChanged += onChanged;
		_lock.ExitWriteLock();
	}
}
