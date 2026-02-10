namespace Mino.Native.OpenGL.Command;

public class GLC_SetResourceSet : GLC_Command {
	private uint _set;
	private int _slot;

	public GLC_SetResourceSet(int slot, uint set) {
		_set = set;
		_slot = slot;
	}

	public void Execute(GLBackend backend) {
		backend.executeBindResourceSet(_slot, _set);
	}
}
