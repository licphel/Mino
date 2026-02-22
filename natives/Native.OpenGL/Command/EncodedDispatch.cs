namespace Mino.Native.OpenGL.Command;

public class EncodedDispatch : EncodedCommand {
	private uint _x, _y, _z;

	public EncodedDispatch(uint x, uint y, uint z) {
		_x = x;
		_y = y;
		_z = z;
	}

	public void Execute(GLExecutionContext ctx) {
		ctx.ExecuteDispatch(_x, _y, _z);
	}
}
