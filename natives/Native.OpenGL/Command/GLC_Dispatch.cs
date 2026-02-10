namespace Mino.Native.OpenGL.Command;

public class GLC_Dispatch : GLC_Command {
	private uint _x, _y, _z;

	public GLC_Dispatch(uint x, uint y, uint z) {
		_x = x;
		_y = y;
		_z = z;
	}

	public void Execute(GLBackend backend) {
		backend.executeDispatch(_x, _y, _z);
	}
}
