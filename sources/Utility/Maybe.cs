namespace Mino.Utility;

/// <summary>
///		Maybe value handling both class and struct type.
/// </summary>
public readonly struct Maybe<T> : IEquatable<Maybe<T>> {
	public static readonly Maybe<T> None = new Maybe<T>(default, false);
	
	private readonly T? _value;

	public Maybe() {
		// i.e. None.
		_value = default;
		HasValue = false;
	}
	
	private Maybe(T? value, bool hasValue) {
		_value = value;
		HasValue = hasValue;
	}
	
	/// <summary>
	///		Creates a maybe.
	/// </summary>
	/// <param name="value">Nullable value.</param>
	/// <returns>A maybe.</returns>
	/// <exception cref="Error">Thrown if input is null.</exception>
    public static Maybe<T> Of(in T value) {
		if (default(T) is null && value is null) {
			throw new Error(nameof(value), "cannot create some with null value");
		}
		return new Maybe<T>(value, true);
	}
	
	/// <summary>
	///		Whether the maybe is some.
	/// </summary>
    public bool HasValue { get; }
	
	/// <summary>
	///		The value of the maybe.
	/// </summary>
	/// <exception cref="Error">Thrown if this is none.</exception>
    public T Value {
		get {
			if (!HasValue) {
				throw new Error("maybe has no value");
			}
			return _value!;
		}
	}
	
    public T GetValueOrDefault(T defaultValue = default!) {
		return HasValue ? _value! : defaultValue;
	}
	
    public static implicit operator Maybe<T>(T value) {
		if (default(T) is null && value is null) {
			return None;
		}
		return Of(value);
	}
	
    public static explicit operator T(Maybe<T> maybe) {
		return maybe.Value;
	}
	
	public bool Equals(Maybe<T> other) {
		if (HasValue != other.HasValue) {
			return false;
		}
		if (!HasValue) {
			return true;
		}
		if (_value is null) {
			return other._value is null;
		}
		return _value.Equals(other._value);
	}

	public override bool Equals(object? obj) {
		return obj is Maybe<T> other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(_value, HasValue);
	}

	public override string ToString() {
		return HasValue
			? $"Some({_value})"
			: "None";
	}

    public static bool operator ==(Maybe<T> left, Maybe<T> right) {
        return left.Equals(right);
    }

    public static bool operator !=(Maybe<T> left, Maybe<T> right) {
        return !(left == right);
    }
}
