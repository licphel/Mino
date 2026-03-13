namespace Mino.Utility.Configuration;

/// <summary>
///		An auto-saved config.
/// </summary>
public class Config<T> {
	private Maybe<T> _fallback;
	private string _key;

	public Config(string key, in Maybe<T> fallback) {
		_key = key;
		_fallback = fallback;
		ConfigSystem._finalW.Enqueue(delegate {
			// Flush the value.
			Set(Get());	
		});
	}

	/// <summary>
	///		Sets the value.
	/// </summary>
	/// <param name="value">Value to set.</param>
	public void Set(T value) {
		if (!ConfigSystem._init) {
			throw new Crash("Config system data not loaded");
		}
		ConfigSystem._G.Set(_key, value);
	}

	/// <summary>
	///		Gets the value.
	/// </summary>
	/// <returns>The config value.</returns>
	public T Get() {
		if (!ConfigSystem._init) {
			throw new Crash("Config system data not loaded");
		}
		return ConfigSystem._G.Get(_key, _fallback);
	}

	public static implicit operator T(Config<T> opt) {
		return opt.Get();
	}
}