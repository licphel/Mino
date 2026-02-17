#region
using Mino.Graphics.Hardware.Desc;
using Silk.NET.OpenGL;
#endregion

namespace Mino.Native.OpenGL.Object;

public class GLSampler {
	public GL _gl;
	public uint _handle;
	public SamplerDesc _desc;

	public GLSampler(GL gl, uint handle) {
		_gl = gl;
		_handle = handle;
	}

	public void OnSamplerData(in SamplerDesc desc) {
		// Set userdata.
		_desc = desc;

		_gl.SamplerParameter(_handle, GLEnum.TextureWrapS, GLEnumC.Cast(desc.WrapX));
		_gl.SamplerParameter(_handle, GLEnum.TextureWrapT, GLEnumC.Cast(desc.WrapY));
		_gl.SamplerParameter(_handle, GLEnum.TextureWrapR, GLEnumC.Cast(desc.WrapZ));

		_gl.SamplerParameter(_handle, GLEnum.TextureMinFilter, GLEnumC.Cast(desc.MinFilter));
		_gl.SamplerParameter(_handle, GLEnum.TextureMagFilter, GLEnumC.Cast(desc.MagFilter));

		if (desc.AnisotropyLevel > 1.0F) {
			_gl.SamplerParameter(_handle, GLEnum.TextureMaxAnisotropy, desc.AnisotropyLevel);
		}

		_gl.SamplerParameter(_handle, GLEnum.TextureMinLod, desc.MinLod);
		_gl.SamplerParameter(_handle, GLEnum.TextureMaxLod, desc.MaxLod);
		_gl.SamplerParameter(_handle, GLEnum.TextureLodBias, desc.LodBias);

		_gl.SamplerParameter(
			_handle, GLEnum.TextureBorderColor, [
				desc.WrapBorderColor.Red,
				desc.WrapBorderColor.Green,
				desc.WrapBorderColor.Blue,
				desc.WrapBorderColor.Alpha
			]
		);
	}
}
