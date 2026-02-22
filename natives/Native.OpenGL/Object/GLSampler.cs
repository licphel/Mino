using Mino.Framework.Resource;
using Mino.Graphics.Desc;
using Silk.NET.OpenGL;
using Sampler = Mino.Graphics.Sampler;

namespace Mino.Native.OpenGL.Object;

public sealed class GLSampler : Sampler {
	public GL _gl = null!;
	public GLContext _ctx = null!;
	public uint _handle;
	public bool _disposed;
	
	public SamplerDesc _desc;
	
	[ResourceCreation]
	public GLSampler(in SamplerDesc desc) {
		_desc = desc;
	}

	public SamplerDesc Desc {
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
			_handle = _gl.GenSampler();
			
			_gl.SamplerParameter(_handle, GLEnum.TextureWrapS, GLEnumC.Cast(_desc.WrapX));
			_gl.SamplerParameter(_handle, GLEnum.TextureWrapT, GLEnumC.Cast(_desc.WrapY));
			_gl.SamplerParameter(_handle, GLEnum.TextureWrapR, GLEnumC.Cast(_desc.WrapZ));

			_gl.SamplerParameter(_handle, GLEnum.TextureMinFilter, GLEnumC.Cast(_desc.MinFilter));
			_gl.SamplerParameter(_handle, GLEnum.TextureMagFilter, GLEnumC.Cast(_desc.MagFilter));

			if (_desc.AnisotropyLevel > 1.0F) {
				_gl.SamplerParameter(_handle, GLEnum.TextureMaxAnisotropy, _desc.AnisotropyLevel);
			}

			_gl.SamplerParameter(_handle, GLEnum.TextureMinLod, _desc.MinLod);
			_gl.SamplerParameter(_handle, GLEnum.TextureMaxLod, _desc.MaxLod);
			_gl.SamplerParameter(_handle, GLEnum.TextureLodBias, _desc.LodBias);

			_gl.SamplerParameter(
				_handle, GLEnum.TextureBorderColor, [
					_desc.WrapBorderColor.Red,
					_desc.WrapBorderColor.Green,
					_desc.WrapBorderColor.Blue,
					_desc.WrapBorderColor.Alpha
				]
			);
		});
	}
	
	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		
		_ctx.Pend(() => {
			_gl.DeleteSampler(_handle);
		});
	}
}
