namespace Mino.Framework.Registry;

/// <summary>
///		A registerable object.
/// </summary>
public interface Registerable {
	/// <summary>
	///		Named identifier.
	/// </summary>
	Identifier Id { get; set; }
	
	/// <summary>
	///		Integer identifier.
	/// </summary>
	int IntId { get; set; }
}
