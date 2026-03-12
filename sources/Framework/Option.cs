using Mino.Nio.NBT;

namespace Mino.Framework;

/// <summary>
///		An auto-saved option.
/// </summary>
public class Option<T> {
	private T? _fallback;
	private Seq _key;

	public Option(Seq key, T? fallback) {
		_key = key;
		_fallback = fallback;
		OptionSystem._finalW.Enqueue(delegate {
			// Flush the value.
			Set(Get());	
		});
	}

	/// <summary>
	///		Sets the value.
	/// </summary>
	/// <param name="value">Value to set.</param>
	public void Set(T value) {
		if (!OptionSystem._init) {
			throw new Error("local data not loaded");
		}
		OptionSystem._G.Set(_key, value);
	}

	/// <summary>
	///		Gets the value.
	/// </summary>
	/// <returns>The option value.</returns>
	public T Get() {
		if (!OptionSystem._init) {
			throw new Error("local data not loaded");
		}
		return OptionSystem._G.Get(_key, _fallback);
	}

	public static implicit operator T(Option<T> opt) {
		return opt.Get();
	}
}