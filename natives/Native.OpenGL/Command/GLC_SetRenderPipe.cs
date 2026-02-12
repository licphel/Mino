namespace Mino.Native.OpenGL.Command;

public class GLC_SetRenderPipe : GLC_Command {
	private uint _pipeId;

	public GLC_SetRenderPipe(uint pipeId) {
		_pipeId = pipeId;
	}

	public void Execute(GLBackend backend) {
		backend.executeBindRenderPipe(_pipeId);
	}
}
