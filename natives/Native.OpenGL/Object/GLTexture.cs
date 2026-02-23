using System.Runtime.InteropServices;
using Mino.Framework.Resource;
using Mino.Graphics;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
using Silk.NET.OpenGL;
using Texture = Mino.Graphics.Texture;

namespace Mino.Native.OpenGL.Object;

public unsafe sealed class GLTexture : Texture {
	public GL _gl = null!;
	public GLContext _ctx = null!;
	public uint _handle;
	public bool _disposed;
	
	public TextureDesc _desc;
	public GLEnum _iFormat;
	public GLEnum _target;
	public GLEnum _pixFormat;
	public GLEnum _pixType;

	[ResourceCreation]
	public GLTexture(in TextureDesc desc) {
		_desc = desc;
	}

	public TextureDesc Desc {
		get => _desc;
	}

	public int Capacity { get; set; }

	// By default, we allow expansion.
	public bool CanExpand { get; set; } = true;
	
	public void Submit(in TextureSubmission submission) {
		int x = (int) submission.Region.MinX;
		int y = (int) submission.Region.MinY;
		int z = (int) submission.Region.MinZ;
		int width = (int) submission.Region.Width;
		int height = (int) submission.Region.Height;
		int depth = (int) submission.Region.Depth;
		byte[]? rawData = submission.Bytes;
		
		_ctx.Pend(() => {
			GLCache c = _ctx._cache;
			
			c.SetTexture(_target, 0, _handle);

			// Flip-y for gl usage.
			if (_desc.Type == TextureType.Texture2D) {
				// Buf fixed: Somehow the texture is flipped,
				// I don't know which part has done it,
				// But it does. Maybe silk.NET?
			}

			byte* dataPtr = null;
			GCHandle? gch = null;
			if (rawData != null) {
				gch = GCHandle.Alloc(rawData, GCHandleType.Pinned);
				dataPtr = (byte*) gch.Value.AddrOfPinnedObject();
			}

			switch (_desc.Type) {
				case TextureType.Texture1D:
					_gl.TexSubImage1D(_target, 0, x, (uint) width, _pixFormat, _pixType, dataPtr);
					break;
				case TextureType.Texture2D:
					_gl.TexSubImage2D(
						_target, 0, x, y, (uint) width, (uint) height, _pixFormat, _pixType, dataPtr);
					break;
				case TextureType.Texture3D:
					_gl.TexSubImage3D(
						_target, 0, x, y, z, (uint) width, (uint) height, (uint) depth, _pixFormat, _pixType,
						dataPtr);
					break;
				default:
					gch?.Free();
					throw new Error("no support");
			}

			gch?.Free();

			// Regen mipmap.
			if (_desc.MipLevels > 0) {
				int maxLevel = _desc.MipLevels - 1;
				_gl.TexParameterI(_target, TextureParameterName.TextureMaxLevel, in maxLevel);
				_gl.GenerateMipmap(_target);
			}
		});
	}
	
	public void Blit(Texture to, int x, int y, int w, int h, int srcX, int srcY, int srcW, int srcH,
		TextureFilter filter = TextureFilter.Nearest) {
		_ctx.Pend(() => {
			GLCache c = _ctx._cache;
			
			GLTexture srcTex = this;
			GLTexture dstTex = (GLTexture) to;
			uint srcFBO = _gl.GenFramebuffer();
			uint dstFBO = _gl.GenFramebuffer();
			
			try {
				c.SetFramebuffer(GLEnum.ReadFramebuffer, srcFBO);
				_gl.FramebufferTexture2D(
					FramebufferTarget.ReadFramebuffer,
					GLEnum.ColorAttachment0,
					srcTex._target,
					srcTex._handle,
					0
				);
				
				c.SetFramebuffer(GLEnum.DrawFramebuffer, dstFBO);
				_gl.FramebufferTexture2D(
					FramebufferTarget.DrawFramebuffer,
					GLEnum.ColorAttachment0,
					dstTex._target,
					dstTex._handle,
					0
				);
				
				_gl.BlitFramebuffer(
					srcX, srcY, srcX + srcW, srcY + srcH,
					x, y, x + w, y + h,
					ClearBufferMask.ColorBufferBit,
					(GLEnum) GLEnumC.Cast(filter)
				);
			} finally {
				_gl.DeleteFramebuffer(srcFBO);
				_gl.DeleteFramebuffer(dstFBO);
			}

			// Regenerate mipmaps.
			c.SetTexture(GLEnum.Texture2D, 0, dstTex._handle);
			_gl.GenerateMipmap(GLEnum.Texture2D);
		});
	}
	
	public Texture Pin() {
		return this;
	}
	
	public bool TryGetThreadContext(out ThreadContext ctx) {
		ctx = _ctx;
		return true;
	}
	
	public void Listen(ThreadContext ctx) {
		_ctx = (GLContext) ctx;
		_gl = _ctx._gl;
		
		_ctx.Pend(() => {
			GLCache c = _ctx._cache;
			
			_handle = _gl.GenTexture();
			
			// Cache enums.
			_target = GLEnumC.Cast(_desc.Type);
			(_iFormat, _pixFormat, _pixType) = GLEnumC.Cast(_desc.Format);

			int width = _desc.Width;
			int height = _desc.Height;
			int depth = _desc.Depth;
			byte[]? data = _desc.InitialBytes;
			
			c.SetTexture(_target, 0, _handle);

			// Flip-y for gl usage.
			if (_desc.Type == TextureType.Texture2D) {
				// Buf fixed: Somehow the texture is flipped,
				// I don't know which part has done it,
				// But it does. Maybe silk.NET?
			}

			byte* dataPtr = null;
			GCHandle? gch = null;
			if (data != null) {
				gch = GCHandle.Alloc(data, GCHandleType.Pinned);
				dataPtr = (byte*) gch.Value.AddrOfPinnedObject();
			}

			switch (_desc.Type) {
				case TextureType.Texture1D:
					_gl.TexImage1D(_target, 0, (int) _iFormat, (uint) width, 0, _pixFormat, _pixType, dataPtr);
					break;
				case TextureType.Texture2D:
					_gl.TexImage2D(
						_target, 0, (int) _iFormat, (uint) width, (uint) height, 0, _pixFormat, _pixType, dataPtr);
					break;
				case TextureType.Texture3D:
					_gl.TexImage3D(
						_target, 0, (int) _iFormat, (uint) width, (uint) height, (uint) depth, 0, _pixFormat, _pixType,
						dataPtr);
					break;
				default:
					gch?.Free();
					throw new Error("no support");
			}

			gch?.Free();

			if (_desc.MipLevels > 0) {
				int maxLevel = _desc.MipLevels - 1;
				_gl.TexParameterI(_target, TextureParameterName.TextureMaxLevel, in maxLevel);
				_gl.GenerateMipmap(_target);
			}
		});
	}
	
	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		
		_ctx.Pend(() => {
			_gl.DeleteTexture(_handle);
		});
	}
}
