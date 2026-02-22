using Mino.Framework.Resource;
using Mino.Graphics;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public sealed class GLRenderTarget : RenderTarget {
	public GL _gl = null!;
	public GLContext _ctx = null!;
	public uint _handle;
	public bool _disposed;

	public RenderTargetDesc _desc;

	[ResourceCreation]
	public GLRenderTarget(in RenderTargetDesc desc) {
		_desc = desc;
	}

	public RenderTargetDesc Desc {
		get => _desc;
	}
	
	public void Acquire(in RenderPassDesc? desc = null) {
		RenderPassDesc nrp = desc.GetValueOrDefault(RenderPassDesc.Default);
		
		_ctx.Pend(() => {
			_gl.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
		
			uint clearMask = 0;
			
			if ((nrp.Clear & ClearMask.Color) != 0) {
				clearMask |= (uint) GLEnum.ColorBufferBit;
				_gl.ClearColor(nrp.ClearColor.Red, nrp.ClearColor.Green, nrp.ClearColor.Blue, nrp.ClearColor.Alpha);
			}

			if ((nrp.Clear & ClearMask.Depth) != 0) {
				clearMask |= (uint) GLEnum.DepthBufferBit;
				_gl.ClearDepth(nrp.ClearDepth);
			}

			if ((nrp.Clear & ClearMask.Stencil) != 0) {
				clearMask |= (uint) GLEnum.StencilBufferBit;
				_gl.ClearStencil(nrp.ClearStencil);
			}

			if (clearMask != 0) {
				_gl.Clear(clearMask);
			}
		});
	}
	
	public void Present() {
		_ctx.Pend(() => {
			_gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

			if (_handle == 0) {
				_ctx._window.Present();
			}
		});
	}
	
	public void Blit(RenderTarget to, int x, int y, int w, int h, int srcX, int srcY, int srcW, int srcH,
		TextureFilter filter = TextureFilter.Nearest) {
		_ctx.Pend(() => {
			GLRenderTarget srcRT = this;
			GLRenderTarget dstRT = (GLRenderTarget) to;
			uint srcFBO = srcRT._handle;
			uint dstFBO = dstRT._handle;

			_gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, srcFBO);
			_gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, dstFBO);

			_gl.BlitFramebuffer(
				srcX, srcY, srcX + srcW, srcY + srcH,
				x, y, x + w, y + h,
				ClearBufferMask.ColorBufferBit,
				(GLEnum) GLEnumC.Cast(filter)
			);

			_gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
			_gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
		});
	}
	
	public bool TryGetThreadContext(out ThreadContext ctx) {
		ctx = _ctx;
		return true;
	}
	
	public void Listen(ThreadContext ctx) {
		_ctx = (GLContext) ctx;
		_gl = _ctx._gl;
		
		_ctx.Pend(() => {
			if (_desc.IsUltimate) {
				_handle = 0; // GL Ultimate fbo is 0.
				return;
			}
			
			_handle = _gl.GenFramebuffer();
			_gl.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);

			for (int i = 0; i < _desc.ColorAttachments.Length; i++) {
				uint texture = _gl.GenTexture();
				_gl.FramebufferTexture2D(
					FramebufferTarget.Framebuffer,
					(GLEnum) ((int) GLEnum.ColorAttachment0 + (uint) i),
					TextureTarget.Texture2D,
					texture,
					0
				);
			}

			if (_desc.DepthStencilAttachment.HasValue) {
				TextureFormat format = _desc.DepthStencilAttachment.Value;
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
		});
	}
	
	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		
		_ctx.Pend(() => {
			_gl.DeleteFramebuffer(_handle);
		});
	}
}
