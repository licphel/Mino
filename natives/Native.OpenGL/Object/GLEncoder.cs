using Mino.Framework.Resource;
using Mino.Graphics;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
using Mino.Native.OpenGL.Command;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public sealed class GLEncoder : Encoder {
	public GL _gl = null!;
	public GLContext _ctx = null!;
	public bool _disposed;
	
	public EncoderDesc _desc;
	public List<EncodedCommand> _commands = new List<EncodedCommand>();
	
	[ResourceCreation]
	public GLEncoder(in EncoderDesc desc) {
		_desc = desc;
	}

	public EncoderDesc Desc {
		get => _desc;
	}

	public void Reset() {
		_commands.Clear();
	}
	
	public void QueuedExecute() {
		EncodedCommand[] cmdRunList = _commands.ToArray();
		
		_ctx.Pend(() => {
			foreach (EncodedCommand cmd in cmdRunList) {
				cmd.Execute(_ctx._exeCtx);
			}
		});
	}
	
	public void SetTopology(Topology topology) {
		_commands.Add(new EncodedSetTopology(topology));
	}
	
	public void SetBuffer(BufferObject buffer) {
		_commands.Add(new EncodedBindBuffer((GLBufferObject) buffer));
	}
	
	public void SetRenderPipe(RenderPipe pipe) {
		_commands.Add(new EncodedSetRenderPipe((GLRenderPipe) pipe));
	}
	
	public void SetViewport(int x, int y, int width, int height) {
		_commands.Add(new EncodedSetViewport(x, y, width, height));
	}
	
	public void SetScissor(in ScissorDesc desc) {
		_commands.Add(new EncodedSetScissor(desc));
	}
	
	public void SetResource(int slot, ResourceSet set) {
		_commands.Add(new EncodedSetResourceSet(slot, (GLResourceSet) set));
	}
	
	public void Draw(int vertexCount, int firstVertex) {
		_commands.Add(new EncodedDraw(vertexCount, firstVertex));
	}
	
	public void DrawIndexed(int indexCount, int firstIndex) {
		_commands.Add(new EncodedDrawIndexed(indexCount, firstIndex));
	}
	
	public void Dispatch(uint x, uint y, uint z) {
		_commands.Add(new EncodedDispatch(x, y, z));
	}
	
	public bool TryGetThreadContext(out ThreadContext ctx) {
		ctx = _ctx;
		return true;
	}
	
	public void Listen(ThreadContext ctx) {
		_ctx = (GLContext) ctx;
		_gl = _ctx._gl;
	}
	
	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
	}
}
