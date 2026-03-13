using Mino.Utility;

namespace Mino.Modular.Resource;

/// <summary>
///		Asset ref.
/// </summary>
public struct Holder<T> where T : class {
	public readonly Identifier Id;
	private T? _fallback;
	private HolderNotifier _notifier;
	
	public Holder(Identifier id, HolderNotifier notifier, T? fallback = null) {
		Id = id;
		_fallback = fallback;
		_notifier = notifier;
	}

	/// <summary>
	///		Tries to get an asset.
	/// </summary>
	/// <exception cref="Crash">Thrown if cannot get and no fallback is bound.</exception>
	public T Value {
		get {
			object? obj = _notifier._object as T ?? _fallback;
			if (obj == null) {
				throw new Crash($"There's no asset named '{Id}'");
			}
			if (obj is not T) {
				throw new Crash($"Asset type does not match: expected={typeof(T)}, got={obj.GetType()}");
			}
			return (T) obj;
		}
	}

	public static implicit operator T(in Holder<T> holder) {
		return holder.Value;
	}
}
