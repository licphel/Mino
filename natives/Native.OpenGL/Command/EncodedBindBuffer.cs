#region
using Mino.Native.OpenGL.Object;
#endregion

namespace Mino.Native.OpenGL.Command;

public class EncodedBindBuffer : EncodedCommand {
	private GLBufferObject _buffer;

	public EncodedBindBuffer( GLBufferObject buffer) {
		_buffer = buffer;
	}

	public void Execute(GLExecutionContext ctx) {
		ctx.ExecuteBindBuffer(_buffer);
	}
}
