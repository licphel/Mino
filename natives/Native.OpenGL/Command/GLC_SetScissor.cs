#region
using Mino.Graphics.Hardware.Desc;
#endregion

namespace Mino.Native.OpenGL.Command;

public class GLC_SetScissor : GLC_Command {
	private ScissorDesc _desc;

	public GLC_SetScissor(ScissorDesc desc) {
		_desc = desc;
	}

	public void Execute(GLBackend backend) {
		backend.executeScissor(_desc);
	}
}
