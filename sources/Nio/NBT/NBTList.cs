#region
using System.Collections;
using Mino.Utility;
#endregion

namespace Mino.Nio.NBT;

/// <summary>
///     NBT List component.
/// </summary>
public class NBTList : IEnumerable<object> {
	private List<object?> _list = new List<object?>();

	/// <summary>
	///     Count of the list.
	/// </summary>
	public int Count {
		get => _list.Count;
	}

	/// <summary>
	///     Clears the list.
	/// </summary>
	public void Clear() {
		_list.Clear();
	}

	/// <summary>
	///     Gets the value at the given index.
	/// </summary>
	/// <param name="i">Index.</param>
	/// <param name="fallback">Fallback value.</param>
	/// <typeparam name="T">Type cast target.</typeparam>
	/// <returns>A casted value.</returns>
	public T Get<T>(int i, in Maybe<T> fallback = default) {
		if (i >= Count || i < 0) {
			return NBTSystem.GetNonnullFallback(fallback);
		}
		return NBTSystem.AsWithFallback(_list[i], fallback);
	}

	/// <summary>
	///     Gets the value at the given index.
	/// </summary>
	/// <param name="i">Index.</param>
	/// <param name="fallback">Fallback value.</param>
	/// <typeparam name="T">Type cast target.</typeparam>
	/// <returns>A casted value.</returns>
	public T Get<T>(int i, Func<T> fallback) {
		if (i >= Count || i < 0) {
			return fallback.Invoke();
		}
		return NBTSystem.AsWithFallback(_list[i], fallback);
	}

	/// <summary>
	///     Adds a value to the end of the list.
	/// </summary>
	/// <param name="v">Pushed value.</param>
	/// <exception cref="InvalidOperationException">Thrown if value type is invalid.</exception>
	public void Add<T>(T? v) {
		if (!NBTSystem.Validate(v)) {
			throw new InvalidOperationException($"Invalid type: {v?.GetType()}");
		}
		_list.Add(v);
	}

	/// <summary>
	///     Inserts a value to the given index.
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="v">Inserted value.</param>
	/// <exception cref="InvalidOperationException">Thrown if value type is invalid.</exception>
	public void Insert<T>(int index, T? v) {
		if (!NBTSystem.Validate(v)) {
			throw new InvalidOperationException($"Invalid type: {v?.GetType()}");
		}
		_list.Insert(index, v);
	}

	/// <summary>
	///     Sets the value at the given index.
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="v">Set value.</param>
	/// <exception cref="InvalidOperationException">Thrown if value type is invalid.</exception>
	public void Set<T>(int index, T? v) {
		if (!NBTSystem.Validate(v)) {
			throw new InvalidOperationException($"Invalid type: {v?.GetType()}");
		}
		_list[index] = v;
	}

	/// <summary>
	///     Removes an object from the list.
	/// </summary>
	/// <param name="v"></param>
	public void Remove(object v) {
		_list.Remove(v);
	}

	/// <summary>
	///     Removes an object by its index.
	/// </summary>
	/// <param name="i"></param>
	public void RemoveAt(int i) {
		_list.RemoveAt(i);
	}

	public IEnumerator<object> GetEnumerator() {
		return _list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}

	/// <summary>
	///     Clones the list.
	/// </summary>
	/// <returns>A new identical list.</returns>
	public NBTList Clone() {
		NBTList newList = new NBTList();

		foreach (object obj in this) {
			if (obj is NBTCompound map) {
				newList.Add(map.Clone());
			} else if (obj is NBTList list) {
				newList.Add(list.Clone());
			} else {
				newList.Add(obj);
			}
		}

		return newList;
	}

	public override bool Equals(object? obj) {
		return obj is NBTList list && Equals(list);
	}

	public override int GetHashCode() {
		// Hashcode is not stable.
		throw new InvalidOperationException("Cannot use NBTList as keys");
	}

	public static bool operator ==(NBTList? left, NBTList? right) {
		return Equals(left, right);
	}

	public static bool operator !=(NBTList? left, NBTList? right) {
		return !Equals(left, right);
	}

	public bool Equals(NBTList other) {
		return _list.SequenceEqual(other._list);
	}
}
