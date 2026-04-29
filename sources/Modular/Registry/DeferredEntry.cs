namespace Mino.Modular.Registry;

/// <summary>
///		Deferred registered value.
/// </summary>
public class DeferredEntry<T> where T : class, RegistryObject {
	public DeferredEntry(in Identifier id) {
		Id = id;
	}
	
	/// <summary>
	///		The registry entry id.
	/// </summary>
	public Identifier Id { get; }

	/// <summary>
	///		The deferred-injected value.
	/// </summary>
	public T? Value { get; internal set; } = null;

	/// <summary>
	///		Whether the value is fetched.
	/// </summary>
	public bool HasValue {
		get => Value != null;
	}

	public static implicit operator T(DeferredEntry<T> entry) {
		if (entry.HasValue) {
			return entry.Value!;
		}
		throw new RMLException("Deferred registry entry has no value");
	}
}
