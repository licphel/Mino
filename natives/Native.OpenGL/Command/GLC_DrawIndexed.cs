namespace Mino.Native.OpenGL.Command;

public class GLC_DrawIndexed : GLC_Command {
	private int _firstIndex;
	private int _indexCount;

	public GLC_DrawIndexed(int indexCount, int firstIndex) {
		_indexCount = indexCount;
		_firstIndex = firstIndex;
	}

	public void Execute(GLBackend backend) {
		backend.executeDrawIndexed(_indexCount, _firstIndex);
	}
}
