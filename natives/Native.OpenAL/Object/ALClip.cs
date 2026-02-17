#region
using Mino.Audio.Hardware.Desc;
using Silk.NET.OpenAL;
#endregion

namespace Mino.Native.OpenAL.Object;

public class ALClip {
	public AL _al;
	public uint _handle;
	public ClipDesc _desc;

	public ALClip(AL al, uint handle) {
		_al = al;
		_handle = handle;
	}
}
