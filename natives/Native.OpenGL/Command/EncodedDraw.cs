namespace Mino.Native.OpenGL.Command;

public class EncodedDraw : EncodedCommand {
	private int _firstVertex;
	private int _vertexCount;

	public EncodedDraw(int vertexCount, int firstVertex) {
		_vertexCount = vertexCount;
		_firstVertex = firstVertex;
	}

	public void Execute(GLExecutionContext ctx) {
		ctx.ExecuteDraw(_vertexCount, _firstVertex);
	}
}
