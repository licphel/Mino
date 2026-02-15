#region
using System.Collections;
#endregion

namespace Mino.Nio.NBT;

/// <summary>
///     NBT Map component.
/// </summary>
public class TagMap : IEnumerable<KeyValuePair<string, object>> {
	// Toolkit factories.
	// Used like map.Get<TagMap>("my_key", TagMap.NewMap);
	public static readonly Func<TagMap> NewMap = () => new TagMap();
	public static readonly Func<TagList> NewList = () => new TagList();

	private Dictionary<string, object?> _dict = new Dictionary<string, object?>();

	/// <summary>
	///     Count of the map.
	/// </summary>
	public int Count {
		get => _dict.Count;
	}

	/// <summary>
	///     Clears the map.
	/// </summary>
	public void Clear() {
		_dict.Clear();
	}

	/// <summary>
	///     Checks if the map has a key.
	/// </summary>
	/// <param name="key">Checking key.</param>
	/// <returns>True if has, otherwise false.</returns>
	public bool Has(string key) {
		return _dict.ContainsKey(key);
	}

	/// <summary>
	///     Gets the value under the given key.
	/// </summary>
	/// <param name="key">Map key.</param>
	/// ///
	/// <param name="fallback">Fallback value.</param>
	/// <typeparam name="T">Type cast target.</typeparam>
	/// <returns>A casted value.</returns>
	public T Get<T>(string key, T? fallback = default) {
		if (_dict.TryGetValue(key, out object? value)) {
			return TagSystem.AsWithFallback(value, fallback);
		}
		return TagSystem.GetNonnullFallback(fallback);
	}

	/// <summary>
	///     Gets the value under the given key.
	/// </summary>
	/// <param name="key">Map key.</param>
	/// <param name="fallback">Fallback value.</param>
	/// <typeparam name="T">Type cast target.</typeparam>
	/// <returns>A casted value.</returns>
	public T Get<T>(string key, Func<T> fallback) {
		if (_dict.TryGetValue(key, out object? value)) {
			return TagSystem.AsWithFallback(value, fallback);
		}
		return fallback.Invoke();
	}

	/// <summary>
	///     Seeks by a key sequence like "key1.key2.key3".
	/// </summary>
	/// <param name="key">Key sequence.</param>
	/// <param name="fallback">Fallback value.</param>
	/// <typeparam name="T">Type cast target.</typeparam>
	/// <returns></returns>
	public T Seek<T>(string key, T? fallback = default) {
		string[] keys = key.Split('.');
		if (keys.Length <= 1) {
			return Get(key, fallback);
		}

		TagMap map = Get(keys[0], NewMap);
		for (int i = 1; i < keys.Length - 1; i++) {
			map = map.Get(keys[i], NewMap);
		}

		return map.Get(keys[^1], fallback);
	}

	/// <summary>
	///     Seeks by a key sequence like "key1.key2.key3".
	/// </summary>
	/// <param name="key">Key sequence.</param>
	/// <param name="fallback">Fallback value.</param>
	/// <typeparam name="T">Type cast target.</typeparam>
	/// <returns></returns>
	public T Seek<T>(string key, Func<T> fallback) {
		string[] keys = key.Split('.');
		if (keys.Length <= 1) {
			return Get(key, fallback);
		}

		TagMap map = Get(keys[0], NewMap);
		for (int i = 1; i < keys.Length - 1; i++) {
			map = map.Get(keys[i], NewMap);
		}

		return map.Get(keys[^1], fallback);
	}

	/// <summary>
	///     Tries getting a value.
	/// </summary>
	/// <param name="key">Map key.</param>
	/// <param name="value">Output value.</param>
	/// <typeparam name="T">Type cast target.</typeparam>
	/// <returns>True if this map has the key, otherwise false.</returns>
	public bool TryGet<T>(string key, out T value) {
		if (_dict.TryGetValue(key, out object? raw)) {
			if (raw != null) {
				value = TagSystem.AsWithFallback(raw, default(T));
				return true;
			}
		}
		value = default!;
		return false;
	}

	/// <summary>
	///     Sets the value with the given key.
	/// </summary>
	/// <param name="key">Map key.</param>
	/// <param name="v">Set value.</param>
	/// <exception cref="Error">Thrown if value type is invalid.</exception>
	public void Set(string key, object? v) {
		if (!TagSystem.Validate(v)) {
			throw new Error($"invalid type: {v?.GetType()}");
		}
		_dict[key] = v;
	}

	/// <summary>
	///     Removes a key-value pair.
	/// </summary>
	/// <param name="key">Map key.</param>
	public void Remove(string key) {
		_dict.Remove(key);
	}

	public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
		return _dict.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}

	/// <summary>
	///     Clones the map.
	/// </summary>
	/// <returns>A new identical map.</returns>
	public TagMap Clone() {
		TagMap newMap = new TagMap();

		foreach (KeyValuePair<string, object> pair in this) {
			if (pair.Value is TagMap map) {
				newMap.Set(pair.Key, map.Clone());
			} else if (pair.Value is TagList list) {
				newMap.Set(pair.Key, list.Clone());
			} else {
				newMap.Set(pair.Key, pair.Value);
			}
		}

		return newMap;
	}

	public override bool Equals(object? obj) {
		return obj is TagMap map && Equals(map);
	}

	public override int GetHashCode() {
		// Hashcode is not stable.
		throw new Error("cannot use as key");
	}

	public static bool operator ==(TagMap? left, TagMap? right) {
		return Equals(left, right);
	}

	public static bool operator !=(TagMap? left, TagMap? right) {
		return !Equals(left, right);
	}

	public bool Equals(TagMap other) {
		if (Count != other.Count) {
			return false;
		}

		foreach (KeyValuePair<string, object> kv in this) {
			if (!other._dict.TryGetValue(kv.Key, out object? obj)) {
				return false;
			}
			if (!kv.Value.Equals(obj)) {
				return false;
			}
		}

		return true;
	}
}
