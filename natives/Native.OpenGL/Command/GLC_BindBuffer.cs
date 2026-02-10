using Mino.Graphics.RHI.Enum;

namespace Mino.Native.OpenGL.Command;

public class GLC_BindBuffer : GLC_Command {
	private BufferType _type;
	private uint _buffer;
	
	public GLC_BindBuffer(BufferType type, uint buffer) {
		_type = type;
		_buffer = buffer;
	}

	public void Execute(GLBackend backend) {
		backend.executeBindBuffer(_type, _buffer);
	}
}
