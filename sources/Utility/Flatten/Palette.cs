using System.Collections;

namespace Mino.Utility.Flatten;

public sealed class Palette<T> : IEnumerable<T> where T : notnull {
	private Dictionary<int, T> _itot = new Dictionary<int, T>();
	private Dictionary<T, int> _ttoi = new Dictionary<T, int>();

	public int IdFor(T t) {
		return _ttoi[t];
	}

	public T FromId(int id) {
		return _itot[id];
	}

	public void Add(T t) {
		int id = Count++;
		_itot[id] = t;
		_ttoi[t] = id;
	}

	public int Count { get; private set; }
	
	public IEnumerator<T> GetEnumerator() {
		return _itot.Values.GetEnumerator();
	}
	
	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}
