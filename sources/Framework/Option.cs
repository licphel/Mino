using Mino.Nio.NBT;

namespace Mino.Framework;

/// <summary>
///		An auto-saved option.
/// </summary>
public class Option<T> {
	private T _value;
	private Seq _key;

	public Option(Seq key, T? defaultValue = default) {
		if (!OptionSystem._init) {
			throw new Error("local data not loaded");
		}
		_key = key;
		if (!OptionSystem._G.Has(key)) {
			OptionSystem._G.Set(key, defaultValue);
		}
		_value = OptionSystem._G.Get(key, defaultValue);
	}

	/// <summary>
	///		Sets the value.
	/// </summary>
	/// <param name="value">Value to set.</param>
	public void Set(T value) {
		_value = value;
		OptionSystem._G.Set(_key, _value);
	}

	/// <summary>
	///		Gets the value.
	/// </summary>
	/// <returns>The option value.</returns>
	public T Get() {
		return _value;
	}
}