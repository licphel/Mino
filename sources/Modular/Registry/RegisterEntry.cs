namespace Mino.Modular.Registry;

/// <summary>
///		A registerable object.
/// </summary>
public class RegisterEntry : RegisterInterface {
	public Identifier Id { get; private set; }
	public int IntId { get; private set; }
	
	public void Freeze(Identifier id, int iid) {
		Id = id;
		IntId = iid;
	}
}
