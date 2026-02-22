using Mino.Native.OpenGL.Object;

namespace Mino.Native.OpenGL.Command;

public class EncodedSetRenderPipe : EncodedCommand {
	private GLRenderPipe _pipe;

	public EncodedSetRenderPipe(GLRenderPipe pipe) {
		_pipe = pipe;
	}

	public void Execute(GLExecutionContext ctx) {
		ctx.ExecuteBindRenderPipe(_pipe);
	}
}
