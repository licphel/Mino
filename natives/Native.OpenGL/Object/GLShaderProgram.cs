using Mino.Graphics.RHI.Desc;
using Silk.NET.OpenGL;
using ShaderType = Mino.Graphics.RHI.Enum.ShaderType;

namespace Mino.Native.OpenGL.Object;

public class GLShaderProgram {
	public GL _gl;
	public uint _handle;
	public ShaderProgramDesc _desc;

	public GLShaderProgram(GL gl, uint handle) {
		_gl = gl;
		_handle = handle;
	}

	public void OnShaderProgramLink(in ShaderProgramDesc desc, GLBackend backend) {
		// Set userdata.
		_desc = desc;
		uint[] modules = desc.Modules;

		foreach (uint m in modules) {
			// Convert to gl handle.
			ref GLShaderModule mi = ref backend._moduleHeap.GetData(m);

			_gl.AttachShader(_handle, mi._handle);

			// Only frag shader has MRT.
			if (mi._desc.Type == ShaderType.Fragment) {
				for (int i = 0; i < mi._desc.Targets.Length; i++) {
					_gl.BindFragDataLocation(_handle, (uint) i, mi._desc.Targets[i]);
				}
			}
		}

		_gl.LinkProgram(_handle);
		_gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int linkStatus);
		if (linkStatus == 0) {
			_gl.GetProgramInfoLog(_handle, out string linkLog);
			throw new Error($"shader linking failed '{linkLog}'");
		}

		foreach (uint m in modules) {
			// Convert to gl handle.
			ref GLShaderModule mi = ref backend._moduleHeap.GetData(m);

			_gl.DetachShader(_handle, mi._handle);
		}

		_gl.ValidateProgram(_handle);
		_gl.GetProgramInfoLog(_handle, out string validateLog);

		if (!string.IsNullOrEmpty(validateLog)) {
			throw new Error($"shader validation failed '{validateLog}'");
		}
	}
}
