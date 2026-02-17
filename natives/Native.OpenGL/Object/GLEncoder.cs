#region
using Mino.Graphics.Hardware.Desc;
using Mino.Native.OpenGL.Command;
#endregion

namespace Mino.Native.OpenGL.Object;

public class GLEncoder {
	public List<GLC_Command> _commands = new List<GLC_Command>();
	public EncoderDesc _desc = default;
}
