using System.Collections.Concurrent;
using Mino.Utility;

namespace Mino.Modular.Resource;

/// <summary>
///		Provides global asset management.
/// </summary>
public static class Assets {
	private static ConcurrentDictionary<Identifier, HolderNotifier> _notifiers =
		new ConcurrentDictionary<Identifier, HolderNotifier>();

	/// <summary>
	///		Sets an asset.
	/// </summary>
	/// <param name="key">Asset key.</param>
	/// <param name="value">Asset object.</param>
	public static void Set(in Identifier key, object? value) {
		_notifiers.GetOrAdd(key, k => new HolderNotifier()).Notify(value);
	}
	
	///  <summary>
	/// 		Gets an asset.
	///  </summary>
	///  <param name="key">Asset key.</param>
	///  <param name="fallback">Fallback value.</param>
	///  <typeparam name="T">Asset type generic.</typeparam>
	///  <returns>An asset ref.</returns>
	///  <exception cref="Crash">Thrown if type does not match or no such key.</exception>
	public static Holder<T> Get<T>(in Identifier key, T? fallback = null) where T : class {
		return new Holder<T>(key, _notifiers.GetOrAdd(key, k => new HolderNotifier()), fallback);
	}
	
	///  <summary>
	/// 		Gets an asset holder notifier.
	///  </summary>
	///  <param name="key">Asset key.</param>
	///  <returns>An asset holder notifier.</returns>
	public static HolderNotifier GetNotifier(in Identifier key) {
		return _notifiers.GetOrAdd(key, k => new HolderNotifier());
	}

	/// <summary>
	///		Iterates across the assets.
	/// </summary>
	/// <param name="act">Act to perform.</param>
	public static void Foreach(Action<Identifier, HolderNotifier> act) {
		foreach (var kv in _notifiers) {
			act(kv.Key, kv.Value);
		}
	}
}
