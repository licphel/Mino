using Mino.Audio.AHI.Desc;
using Silk.NET.OpenAL;

namespace Mino.Native.OpenAL.Object;

public class ALClip {
	public AL _al;
	public ClipDesc _desc;
	public uint _handle;

	public ALClip(AL al, uint handle) {
		_al = al;
		_handle = handle;
	}
}
