using Mino.Graphics.RHI.Desc;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public class GLShaderModule {
	public ShaderModuleDesc _desc;
	public GL _gl;
	public uint _handle;

	public GLShaderModule(GL gl, uint handle) {
		_gl = gl;
		_handle = handle;
	}

	public void OnShaderModuleData(in ShaderModuleDesc desc) {
		// ** Delayed handle gen ** //
		_handle = _gl.CreateShader(GLEnumC.Cast(desc.Type));
		// Set userdata.
		_desc = desc;

		_gl.ShaderSource(_handle, desc.Code);
		_gl.CompileShader(_handle);

		_gl.GetShader(_handle, ShaderParameterName.CompileStatus, out int status);
		if (status == 0) {
			_gl.GetShaderInfoLog(_handle, out string txt);
			throw new Error($"shader compilation failed: {txt}");
		}
	}
}
