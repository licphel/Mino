namespace Mino.Framework;

/// <summary>
///		Asset ref.
/// </summary>
public struct Asset<T> {
	public readonly Identifier Id;
	private T? _value;
	private T? _fallback;
	
	public Asset(Identifier id, T? fallback = default) {
		Id = id;
		_fallback = fallback;
	}

	/// <summary>
	///		Tries to get an asset.
	/// </summary>
	/// <exception cref="Error">Thrown if cannot get and no fallback is bound.</exception>
	public T Value {
		get {
			if (_value != null) {
				return _value;
			}
			if (Assets._mapped.TryGetValue(Id, out object? value)) {
				if (value is T t) {
					return _value = t;
				}
			}
			return _fallback ?? throw new Error("no such asset.");
		}
	}

	public static implicit operator T(in Asset<T> asset) {
		return asset.Value;
	}
}
