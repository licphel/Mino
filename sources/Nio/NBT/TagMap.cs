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
	
	// For seek.
	private static readonly TagMap _tmp = new TagMap();
	private static readonly Func<TagMap> _tmpFactory = () => _tmp;
	
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
	public bool Has(in Seq key) {
		if (key.ShouldSplit) {
			SeekForDest(key, false, out TagMap? map, out _);
			return map != null;
		}
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
	public T Get<T>(in Seq key, T? fallback = default) {
		if (key.ShouldSplit) {
			SeekForDest(key, false, out TagMap? map, out Seq key1);
			if (map == null) {
				return TagSystem.GetNonnullFallback(fallback);
			}
			return map.Get(key1, fallback);
		}
		// Default case.
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
	public T Get<T>(in Seq key, Func<T> fallback) {
		if (key.ShouldSplit) {
			SeekForDest(key, false, out TagMap? map, out Seq key1);
			if (map == null) {
				return fallback();
			}
			return map.Get(key1, fallback);
		}
		// Default case.
		if (_dict.TryGetValue(key, out object? value)) {
			return TagSystem.AsWithFallback(value, fallback);
		}
		return fallback.Invoke();
	}
	
	/// <summary>
	///     Tries getting a value.
	/// </summary>
	/// <param name="key">Map key.</param>
	/// <param name="value">Output value.</param>
	/// <typeparam name="T">Type cast target.</typeparam>
	/// <returns>True if this map has the key, otherwise false.</returns>
	public bool TryGet<T>(in Seq key, out T value) {
		if (key.ShouldSplit) {
			SeekForDest(key, false, out TagMap? map, out Seq key1);
			if (map == null) {
				value = default!;
				return false;
			}
			return map.TryGet(key1, out value);
		}
		// Default case.
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
	public void Set(in Seq key, object? v) {
		if (!TagSystem.Validate(v)) {
			throw new Error($"invalid type: {v?.GetType()}");
		}
		if (key.ShouldSplit) {
			SeekForDest(key, true, out TagMap? map, out Seq key1);
			map!.Set(key1, v);
		} else {
			_dict[key] = v;
		}
	}

	/// <summary>
	///     Removes a key-value pair.
	/// </summary>
	/// <param name="key">Map key.</param>
	public void Remove(in Seq key) {
		if (key.ShouldSplit) {
			SeekForDest(key, false, out TagMap? map, out Seq key1);
			map?.Remove(key1);
		} else {
			_dict.Remove(key);
		}
	}

	///  <summary>
	/// 		Seeks a tag map.
	///  </summary>
	///  <param name="key">Key sequence.</param>
	///  <param name="shouldCreate">If the seeking should create new map in the way.</param>
	///  <param name="mapEnd">Output sought map.</param>
	///  <param name="keyEnd">Output sought key.</param>
	public void SeekForDest(in Seq key, bool shouldCreate, out TagMap? mapEnd, out Seq keyEnd) {
		string[] keys = key.Semantic.Split('.');
		if (keys.Length <= 1) {
			keyEnd = key;
			mapEnd = this;
			return;
		}
		Func<TagMap> factory = shouldCreate ? NewMap : _tmpFactory;

		TagMap map = Get(keys[0], factory);
		// Bug fixed: first level map does not expose to its parent.
		if (shouldCreate) {
			Set(keys[0], map);
		}
		
		for (int i = 1; i < keys.Length - 1; i++) {
			TagMap nMap = map.Get(keys[i], factory);
			if (shouldCreate) {
				map.Set(keys[i], nMap);
			}
			map = nMap;
		}
		
		keyEnd = keys[^1];
		mapEnd = ReferenceEquals(_tmp, map) ? null : map;
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
