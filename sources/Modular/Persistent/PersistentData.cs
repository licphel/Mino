using Mino.Utility;
using Mino.Utility.Logging;

namespace Mino.Modular.Persistent;

/// <summary>
///		An auto-saved data.
/// </summary>
public class PersistentData<T> {
	private Maybe<T> _fallback;
	private string _key;
	private PersistentSystem _sys;

	public PersistentData(in Identifier key, in Maybe<T> fallback) {
		_key = key.Path;
		_fallback = fallback;

		try {
			_sys = Mod.Mods[key.Domain.Name].PersistentSystem;
			_sys._finalW.Enqueue(
				delegate {
					// Flush the value.
					Set(Get());
				});
		} catch (Exception ex) {
			Log.Fatal(ex);
			_sys = new PersistentSystem(); // compensate.
		}
	}

	/// <summary>
	///		Sets the value.
	/// </summary>
	/// <param name="value">Value to set.</param>
	public void Set(T value) {
		if (!_sys._init) {
			throw new RMLException("Persistent system data not loaded");
		}
		_sys._G.Set(_key, value);
	}

	/// <summary>
	///		Gets the value.
	/// </summary>
	/// <returns>The data value.</returns>
	public T Get() {
		if (!_sys._init) {
			throw new RMLException("Persistent system data not loaded");
		}
		return _sys._G.Get(_key, _fallback);
	}

	public static implicit operator T(PersistentData<T> opt) {
		return opt.Get();
	}
}