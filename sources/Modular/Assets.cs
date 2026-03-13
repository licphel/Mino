using System.Collections.Concurrent;

namespace Mino.Modular;

/// <summary>
///		Provides global asset management.
/// </summary>
public static class Assets {
	internal static ConcurrentDictionary<Identifier, object> _mapped = new ConcurrentDictionary<Identifier, object>();

	/// <summary>
	///		Sets an asset.
	/// </summary>
	/// <param name="key">Asset key.</param>
	/// <param name="value">Asset object.</param>
	public static void Set(in Identifier key, object value) {
		_mapped[key] = value;
	}

	/// <summary>
	///		Gets an asset.
	/// </summary>
	/// <param name="key">Asset key.</param>
	/// <typeparam name="T">Asset type generic.</typeparam>
	/// <returns>An asset ref.</returns>
	/// <exception cref="Error">Thrown if type does not match or no such key.</exception>
	public static Asset<T> Get<T>(in Identifier key) {
		return new Asset<T>(key);
	}

	///  <summary>
	/// 		Gets an asset.
	///  </summary>
	///  <param name="key">Asset key.</param>
	///  <param name="fallback">Fallback value.</param>
	///  <typeparam name="T">Asset type generic.</typeparam>
	///  <returns>An asset ref.</returns>
	///  <exception cref="Error">Thrown if type does not match or no such key.</exception>
	public static Asset<T> Get<T>(in Identifier key, T fallback) {
		return new Asset<T>(key, fallback);
	}

	/// <summary>
	///		Iterates across the assets.
	/// </summary>
	/// <param name="act">Act to perform.</param>
	public static void Foreach(Action<Identifier, object> act) {
		foreach (var kv in _mapped) {
			act(kv.Key, kv.Value);
		}
	}
}
