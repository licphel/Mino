namespace Mino.Native.OpenGL.Command;

public class EncodedSetViewport : EncodedCommand {
	private int _x, _y, _width, _height;

	public EncodedSetViewport(int x, int y, int width, int height) {
		_x = x;
		_y = y;
		_width = width;
		_height = height;
	}

	public void Execute(GLExecutionContext ctx) {
		ctx.ExecuteViewport(_x, _y, _width, _height);
	}
}
