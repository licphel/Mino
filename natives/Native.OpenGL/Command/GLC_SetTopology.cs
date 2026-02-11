using Mino.Graphics.RHI.Enum;

namespace Mino.Native.OpenGL.Command;

public class GLC_SetTopology : GLC_Command {
	private Topology _topo;

	public GLC_SetTopology(Topology topo) {
		_topo = topo;
	}

	public void Execute(GLBackend backend) {
		backend.executeSetTopology(_topo);
	}
}
