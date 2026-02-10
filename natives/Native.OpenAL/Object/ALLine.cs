using Mino.Audio.AHI.Desc;
using Silk.NET.OpenAL;

namespace Mino.Native.OpenAL.Object;

public class ALLine {
	public AL _al;
	public LineDesc _desc;
	public uint _handle;

	public ALLine(AL al, uint handle) {
		_al = al;
		_handle = handle;
	}
}
