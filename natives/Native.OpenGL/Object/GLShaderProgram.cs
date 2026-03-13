using Mino.Framework.Resource;
using Mino.Graphics;
using Mino.Graphics.Desc;
using Mino.Utility;
using Silk.NET.OpenGL;
using ShaderType = Mino.Graphics.Enum.ShaderType;

namespace Mino.Native.OpenGL.Object;

public sealed class GLShaderProgram : ShaderProgram {
	public GL _gl = null!;
	public GLContext _ctx = null!;
	public uint _handle;
	public bool _disposed;
	
	public ShaderProgramDesc _desc;
	
	[ResourceCreation]
	public GLShaderProgram(in ShaderProgramDesc desc) {
		_desc = desc;
	}

	public ShaderProgramDesc Desc {
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
			_handle = _gl.CreateProgram();
			
			ShaderModule[] modules = _desc.Modules;

			foreach (ShaderModule rsm in modules) {
				GLShaderModule sm = (GLShaderModule) rsm;
				_gl.AttachShader(_handle, sm._handle);

				// Only frag shader has MRT.
				if (sm._desc.Type == ShaderType.Fragment) {
					for (int i = 0; i < sm._desc.Targets.Length; i++) {
						_gl.BindFragDataLocation(_handle, (uint) i, sm._desc.Targets[i]);
					}
				}
			}

			_gl.LinkProgram(_handle);
			_gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int linkStatus);
			if (linkStatus == 0) {
				_gl.GetProgramInfoLog(_handle, out string linkLog);
				throw new Crash($"Shader linking failed: {linkLog}");
			}

			foreach (ShaderModule rsm in modules) {
				GLShaderModule sm = (GLShaderModule) rsm;
				_gl.DetachShader(_handle, sm._handle);
			}

			_gl.ValidateProgram(_handle);
			_gl.GetProgramInfoLog(_handle, out string validateLog);

			if (!string.IsNullOrEmpty(validateLog)) {
				throw new Crash($"Shader validation failed :{validateLog}");
			}
		});
	}
	
	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		
		_ctx.Pend(() => {
			_gl.DeleteProgram(_handle);
		});
	}
}
