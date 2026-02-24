using System.Collections.Concurrent;

namespace Mino.Framework;

/// <summary>
///		Provides global asset management.
/// </summary>
public static class Assets {
	private static ConcurrentDictionary<string, object> _mapped = new ConcurrentDictionary<string, object>();

	/// <summary>
	///		Sets an asset.
	/// </summary>
	/// <param name="key">Asset key.</param>
	/// <param name="value">Asset object.</param>
	public static void Set(string key, object value) {
		_mapped[key] = value;
	}

	/// <summary>
	///		Gets an asset.
	/// </summary>
	/// <param name="key">Asset key.</param>
	/// <typeparam name="T">Asset type generic.</typeparam>
	/// <returns>A converted asset.</returns>
	/// <exception cref="Error">Thrown if type does not match or no such key.</exception>
	public static T Get<T>(string key) {
		if (_mapped.TryGetValue(key, out object? value)) {
			if (value is T t) {
				return t;
			}
		}
		throw new Error($"no such asset: {key}");
	}
}
