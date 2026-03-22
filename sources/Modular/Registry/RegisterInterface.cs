namespace Mino.Modular.Registry;

/// <summary>
///		A registerable interface.
/// </summary>
public interface RegisterInterface {
	/// <summary>
	///		Named identifier.
	/// </summary>
	Identifier Id { get; }
	
	/// <summary>
	///		Integer identifier.
	/// </summary>
	int IntId { get; }

	/// <summary>
	///		Freezes the entry with given ids.
	/// </summary>
	/// <param name="id">Text id.</param>
	/// <param name="iid">Int id.</param>
	void Freeze(Identifier id, int iid);
}
