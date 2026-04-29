namespace Mino.Modular.Registry;

/// <summary>
///		A registry object implementation.
/// </summary>
public class AbstractRegistryObject : RegistryObject {
	public Identifier Id { get; private set; }
	public int IntId { get; private set; }
	
	public void Freeze(Identifier id, int iid) {
		Id = id;
		IntId = iid;
	}
}
