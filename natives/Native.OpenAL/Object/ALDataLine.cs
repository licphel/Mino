#region
using Mino.Audio;
using Mino.Audio.Desc;
using Mino.Framework.Resource;
using Silk.NET.OpenAL;
#endregion

namespace Mino.Native.OpenAL.Object;

public sealed class ALDataLine : DataLine {
	public AL _al = null!;
	public ALContext _ctx = null!;
	public uint _handle;
	public bool _disposed;
	
	public DataLineDesc _desc;
	
	[ResourceCreation]
	public ALDataLine(in DataLineDesc desc) {
		_desc = desc;
	} 

	public DataLineDesc Desc {
		get => _desc;
	}
	
	public bool TryGetThreadContext(out ThreadContext ctx) {
		ctx = _ctx;
		return true;
	}
	
	public void Listen(ThreadContext ctx) {
		_ctx = (ALContext) ctx;
		_al = _ctx._al;
		
		_ctx.Pend(() => {
			_handle = _al.GenBuffer();
			_al.BufferData(_handle, ALEnumC.Cast(_desc.Format), _desc.Data, _desc.SampleRate);
		});
	}
	
	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		
		_ctx.Pend(() => {
			_al.DeleteSource(_handle);
		});
	}
}
