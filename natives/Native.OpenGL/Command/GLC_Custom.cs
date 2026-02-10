namespace Mino.Native.OpenGL.Command;

public class GLC_Custom : GLC_Command {
	private Action<GLBackend> _action;
	
	public GLC_Custom(Action<GLBackend> action) {
		_action = action;
	}

	public void Execute(GLBackend backend) {
		_action(backend);
	}
}
