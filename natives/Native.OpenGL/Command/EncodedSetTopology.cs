#region
using Mino.Graphics.Enum;
#endregion

namespace Mino.Native.OpenGL.Command;

public class EncodedSetTopology : EncodedCommand {
	private Topology _topo;

	public EncodedSetTopology(Topology topo) {
		_topo = topo;
	}

	public void Execute(GLExecutionContext ctx) {
		ctx.ExecuteSetTopology(_topo);
	}
}
