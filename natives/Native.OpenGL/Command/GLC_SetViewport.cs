namespace Mino.Native.OpenGL.Command;

public class GLC_SetViewport : GLC_Command {
	private int _x, _y, _width, _height;

	public GLC_SetViewport(int x, int y, int width, int height) {
		_x = x;
		_y = y;
		_width = width;
		_height = height;
	}

	public void Execute(GLBackend backend) {
		backend.executeViewport(_x, _y, _width, _height);
	}
}
