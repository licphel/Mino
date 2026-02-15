#region
using Mino.Audio.AHI.Desc;
using Silk.NET.OpenAL;
#endregion

namespace Mino.Native.OpenAL.Object;

public class ALLine {
	public AL _al;
	public uint _handle;
	public LineDesc _desc;

	public ALLine(AL al, uint handle) {
		_al = al;
		_handle = handle;
	}
}
