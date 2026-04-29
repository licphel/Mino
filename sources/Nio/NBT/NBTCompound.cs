#region
using System.Collections;
using Mino.Utility;
#endregion

namespace Mino.Nio.NBT;

/// <summary>
///     NBT Map component.
/// </summary>
public class NBTCompound : IEnumerable<KeyValuePair<string, object>> {
	/*
	 * 'Split semantic' is what makes a key sequence like 'a.b.c' able to get a value in layered objects properly.
	 * To use this, you must add a '$' before your key content.
	 * However, 'a.b.<List Index>' is unacceptable now.
	 */
	public const char SplitSemanticChar = '$';
	
	// Toolkit factories.
	// Used like map.Get<NBTCompound>("my_key", NBTCompound.NewMap);
	public static readonly Func<NBTCompound> NewMap = () => new NBTCompound();
	public static readonly Func<NBTList> NewList = () => new NBTList();
	
	// For seek.
	private static readonly NBTCompound _tmp = new NBTCompound();
	private static readonly Func<NBTCompound> _tmpFactory = () => _tmp;
	
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
		if (key.StartsWith(SplitSemanticChar)) {
			SeekForDest(key, false, out NBTCompound? map, out _);
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
	public T Get<T>(string key, in Maybe<T> fallback = default) {
		if (key.StartsWith(SplitSemanticChar)) {
			SeekForDest(key, false, out NBTCompound? map, out string key1);
			if (map == null) {
				return NBTSystem.GetNonnullFallback(fallback);
			}
			return map.Get(key1, fallback);
		}
		// Default case.
		if (_dict.TryGetValue(key, out object? value)) {
			return NBTSystem.AsWithFallback(value, fallback);
		}
		return NBTSystem.GetNonnullFallback(fallback);
	}

	/// <summary>
	///     Gets the value under the given key.
	/// </summary>
	/// <param name="key">Map key.</param>
	/// <param name="fallback">Fallback value.</param>
	/// <typeparam name="T">Type cast target.</typeparam>
	/// <returns>A casted value.</returns>
	public T Get<T>(string key, Func<T> fallback) {
		if (key.StartsWith(SplitSemanticChar)) {
			SeekForDest(key, false, out NBTCompound? map, out string key1);
			if (map == null) {
				return fallback();
			}
			return map.Get(key1, fallback);
		}
		// Default case.
		if (_dict.TryGetValue(key, out object? value)) {
			return NBTSystem.AsWithFallback(value, fallback);
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
	public bool TryGet<T>(string key, out T value) {
		if (key.StartsWith(SplitSemanticChar)) {
			SeekForDest(key, false, out NBTCompound? map, out string key1);
			if (map == null) {
				value = default!;
				return false;
			}
			return map.TryGet(key1, out value);
		}
		// Default case.
		if (_dict.TryGetValue(key, out object? raw)) {
			if (raw != null) {
				value = NBTSystem.AsWithFallback(raw, Maybe<T>.None);
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
	/// <exception cref="InvalidOperationException">Thrown if value type is invalid.</exception>
	public void Set<T>(string key, T? v) {
		if (!NBTSystem.Validate(v)) {
			throw new InvalidOperationException($"Invalid type: {v?.GetType()}");
		}
		if (key.StartsWith(SplitSemanticChar)) {
			SeekForDest(key, true, out NBTCompound? map, out string key1);
			map!.Set(key1, v);
		} else {
			_dict[key] = v;
		}
	}

	/// <summary>
	///     Removes a key-value pair.
	/// </summary>
	/// <param name="key">Map key.</param>
	public void Remove(string key) {
		if (key.StartsWith(SplitSemanticChar)) {
			SeekForDest(key, false, out NBTCompound? map, out string key1);
			map?.Remove(key1);
		} else {
			_dict.Remove(key);
		}
	}

	///  <summary>
	/// 		Seeks a nbt compound.
	///  </summary>
	///  <param name="key">Key sequence.</param>
	///  <param name="shouldCreate">If the seeking should create new map in the way.</param>
	///  <param name="mapEnd">Output sought map.</param>
	///  <param name="keyEnd">Output sought key.</param>
	public void SeekForDest(string key, bool shouldCreate, out NBTCompound? mapEnd, out string keyEnd) {
		string[] keys = key.Substring(1).Split('.');
		if (keys.Length <= 1) {
			keyEnd = key;
			mapEnd = this;
			return;
		}
		Func<NBTCompound> factory = shouldCreate ? NewMap : _tmpFactory;

		NBTCompound map = Get(keys[0], factory);
		// Bug fixed: first level map does not expose to its parent.
		if (shouldCreate) {
			Set(keys[0], map);
		}
		
		for (int i = 1; i < keys.Length - 1; i++) {
			NBTCompound nMap = map.Get(keys[i], factory);
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
	public NBTCompound Clone() {
		NBTCompound newMap = new NBTCompound();

		foreach (KeyValuePair<string, object> pair in this) {
			if (pair.Value is NBTCompound map) {
				newMap.Set(pair.Key, map.Clone());
			} else if (pair.Value is NBTList list) {
				newMap.Set(pair.Key, list.Clone());
			} else {
				newMap.Set(pair.Key, pair.Value);
			}
		}

		return newMap;
	}

	public override bool Equals(object? obj) {
		return obj is NBTCompound map && Equals(map);
	}

	public override int GetHashCode() {
		// Hashcode is not stable.
		throw new InvalidOperationException("Cannot use NBTCompound as keys");
	}

	public static bool operator ==(NBTCompound? left, NBTCompound? right) {
		return Equals(left, right);
	}

	public static bool operator !=(NBTCompound? left, NBTCompound? right) {
		return !Equals(left, right);
	}

	public bool Equals(NBTCompound other) {
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
