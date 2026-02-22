using Mino.Native.OpenGL.Object;

namespace Mino.Native.OpenGL.Command;

public class EncodedSetResourceSet : EncodedCommand {
	private GLResourceSet _set;
	private int _slot;

	public EncodedSetResourceSet(int slot, GLResourceSet set) {
		_set = set;
		_slot = slot;
	}

	public void Execute(GLExecutionContext ctx) {
		ctx.ExecuteBindResourceSet(_slot, _set);
	}
}
