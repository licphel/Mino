using Mino.Nio;

namespace Mino.Framework.Registry;

/// <summary>
///		Deferred registered value.
/// </summary>
public class DeferredEntry<T> where T : class {
	private Func<T> _fetcher;
	
	public DeferredEntry(Func<T> fetcher, in Url url) {
		_fetcher = fetcher;
		Url = url;
	}
	
	/// <summary>
	///		The registry entry url.
	/// </summary>
	public Url Url { get; }

	/// <summary>
	///		The optional value.
	/// </summary>
	public T? Value { get; private set; } = null;

	/// <summary>
	///		Whether the value is fetched.
	/// </summary>
	public bool HasValue {
		get {
			if (Value != null) {
				return true;
			}
			return Fetch();
		}
	}

	/// <summary>
	///		Fetches a registered value.
	/// </summary>
	/// <returns>If the value is present.</returns>
	public bool Fetch() {
		if (Value != null) {
			return true;
		}
		try {
			Value = _fetcher();
			return true;
		} catch {
			return false;
		}
	}

	public static implicit operator T(DeferredEntry<T> entry) {
		if (entry.HasValue) {
			return entry.Value!;
		}
		throw new Error("cannot fetch value");
	}
}
