using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public class GLRenderTarget {
	public uint _handle;
	public GL _gl;
	public RenderTargetDesc _desc;
	
	public GLRenderTarget(GL gl, uint handle) {
		_gl = gl;
		_handle = handle;
	}

	public void OnRenderTargetData(in RenderTargetDesc desc) {
		// Set userdata.
		_desc = desc;
		
		_gl.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
		
		for (int i = 0; i < desc.ColorAttachments.Length; i++) {
			uint texture = _gl.GenTexture();
			_gl.FramebufferTexture2D(
				FramebufferTarget.Framebuffer,
				(GLEnum) ((int) GLEnum.ColorAttachment0 + (uint) i),
				TextureTarget.Texture2D,
				texture,
				0
			);
		}

		if (desc.DepthStencilAttachment.HasValue) {
			TextureFormat format = desc.DepthStencilAttachment.Value;
			uint texture = _gl.GenTexture();

			GLEnum attachment = GLEnum.DepthAttachment;
			if (format == TextureFormat.Depth24Stencil8) {
				attachment = GLEnum.DepthStencilAttachment;
			}

			_gl.FramebufferTexture2D(
				FramebufferTarget.Framebuffer,
				attachment,
				TextureTarget.Texture2D,
				texture,
				0
			);
		}

		GLEnum status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
		if (status != GLEnum.FramebufferComplete) {
			throw new Error($"framebuffer incomplete '{status}'");
		}

		_gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}
}
