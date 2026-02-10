namespace Mino.Native.OpenGL.Command;

public class GLC_SetPipeline : GLC_Command {
	private uint _pipelineId;

	public GLC_SetPipeline(uint pipelineId) {
		_pipelineId = pipelineId;
	}

	public void Execute(GLBackend backend) {
		backend.executeBindPipeline(_pipelineId);
	}
}
