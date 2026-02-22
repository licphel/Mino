using Mino.Framework.Resource;
using Mino.Graphics;
using Mino.Graphics.Desc;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public sealed class GLShaderModule : ShaderModule {
	public GL _gl = null!;
	public GLContext _ctx = null!;
	public uint _handle;
	public bool _disposed;
	
	public ShaderModuleDesc _desc;
	public GLEnum _type;
	
	[ResourceCreation]
	public GLShaderModule(in ShaderModuleDesc desc) {
		_desc = desc;
	}

	public ShaderModuleDesc Desc {
		get => _desc;
	}
	
	public bool TryGetThreadContext(out ThreadContext ctx) {
		ctx = _ctx;
		return true;
	}
	
	public void Listen(ThreadContext ctx) {
		_ctx = (GLContext) ctx;
		_gl = _ctx._gl;

		_ctx.Pend(() => {
			_type = GLEnumC.Cast(_desc.Type);
			
			_handle = _gl.CreateShader(_type);
			
			_gl.ShaderSource(_handle, _desc.Code);
			_gl.CompileShader(_handle);

			_gl.GetShader(_handle, ShaderParameterName.CompileStatus, out int status);
			if (status == 0) {
				_gl.GetShaderInfoLog(_handle, out string txt);
				throw new Error($"shader compilation failed: {txt}");
			}
		});
	}
	
	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		
		_ctx.Pend(() => {
			_gl.DeleteShader(_handle);
		});
	}
}
