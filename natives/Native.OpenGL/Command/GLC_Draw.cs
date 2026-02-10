namespace Mino.Native.OpenGL.Command;

public class GLC_Draw : GLC_Command {
	private int _firstVertex;
	private int _vertexCount;

	public GLC_Draw(int vertexCount, int firstVertex) {
		_vertexCount = vertexCount;
		_firstVertex = firstVertex;
	}

	public void Execute(GLBackend backend) {
		backend.executeDraw(_vertexCount, _firstVertex);
	}
}
