#region
using Mino.Graphics.Desc;
#endregion

namespace Mino.Native.OpenGL.Command;

public class EncodedSetScissor : EncodedCommand {
	private ScissorDesc _desc;

	public EncodedSetScissor(ScissorDesc desc) {
		_desc = desc;
	}

	public void Execute(GLExecutionContext ctx) {
		ctx.ExecuteScissor(_desc);
	}
}
