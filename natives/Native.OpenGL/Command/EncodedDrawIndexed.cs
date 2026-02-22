namespace Mino.Native.OpenGL.Command;

public class EncodedDrawIndexed : EncodedCommand {
	private int _firstIndex;
	private int _indexCount;

	public EncodedDrawIndexed(int indexCount, int firstIndex) {
		_indexCount = indexCount;
		_firstIndex = firstIndex;
	}

	public void Execute(GLExecutionContext ctx) {
		ctx.ExecuteDrawIndexed(_indexCount, _firstIndex);
	}
}
