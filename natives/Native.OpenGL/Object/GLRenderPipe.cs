using Mino.Framework.Resource;
using Mino.Graphics;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public unsafe sealed class GLRenderPipe : RenderPipe {
	public GL _gl = null!;
	public GLContext _ctx = null!;
	public bool _disposed;
	
	public RenderPipeDesc _desc;
	public Dictionary<(uint, uint), uint> _vaoCache = new Dictionary<(uint, uint), uint>();
	
	[ResourceCreation]
	public GLRenderPipe(in RenderPipeDesc desc) {
		_desc = desc;
	}

	public RenderPipeDesc Desc {
		get => _desc;
	}
	
	public uint FindVaoDx(GLCache c, uint vbo, uint ebo = 0) {
		(uint vbo, uint ebo) key = (vbo, ebo);

		if (!_vaoCache.TryGetValue(key, out uint vao)) {
			vao = _gl.GenVertexArray();
			c.SetVertexArray(vao);
			c.SetBuffer(GLEnum.ArrayBuffer, vbo);
			foreach (VertexLayout.Attr attr in _desc.VertexLayout.Attrs) {
				_gl.EnableVertexAttribArray((uint) attr.Location);
				_gl.VertexAttribPointer(
					(uint) attr.Location,
					attr.Components,
					GLEnumC.Cast(attr.Type),
					attr.Normalized,
					(uint) _desc.VertexLayout.Stride,
					(void*) attr.Offset
				);
			}

			if (ebo != 0) {
				c.SetBuffer(GLEnum.ElementArrayBuffer, ebo);
			}
			
			_vaoCache[key] = vao;
		}

		return vao;
	}

	public void ApplyDx() {
		GLCache c = _ctx._cache;
		
		if (_desc.ShaderProgram != null) {
			GLShaderProgram sp = (GLShaderProgram) _desc.ShaderProgram;
			c.SetProgram(sp._handle);
		}

		if (_desc.Blend.Enable) {
			c.SetBlendEnabled(true);
			c.SetBlendFunc(
				GLEnumC.Cast(_desc.Blend.SrcColor),
				GLEnumC.Cast(_desc.Blend.DstColor),
				GLEnumC.Cast(_desc.Blend.SrcAlpha),
				GLEnumC.Cast(_desc.Blend.DstAlpha)
			);
			c.SetBlendEquation(
				GLEnumC.Cast(_desc.Blend.ColorFunc),
				GLEnumC.Cast(_desc.Blend.AlphaFunc)
			);
			c.SetBlendColor(_desc.Blend.Constant);
		} else {
			c.SetBlendEnabled(false);
		}

		if (_desc.Depth.DepthTest) {
			c.SetDepthTestEnabled(true);
			c.SetDepthFunc(GLEnumC.Cast(_desc.Depth.DepthCompare));
		} else {
			c.SetDepthTestEnabled(false);
		}
		
		c.SetDepthMask(_desc.Depth.DepthWrite);

		if (_desc.Stencil.StencilTest) {
			c.SetStencilTestEnabled(true);
			c.SetStencilMask(_desc.Stencil.StencilWriteMask);
			c.SetStencilFunc(
				GLEnumC.Cast(_desc.Stencil.Front.CompareOp),
				GLEnumC.Cast(_desc.Stencil.Back.CompareOp),
				0,
				0,
				_desc.Stencil.StencilReadMask
			);
			c.SetStencilOp(
				GLEnumC.Cast(_desc.Stencil.Front.FailFunc),
				GLEnumC.Cast(_desc.Stencil.Front.DepthFailFunc),
				GLEnumC.Cast(_desc.Stencil.Front.PassFunc),
				GLEnumC.Cast(_desc.Stencil.Back.FailFunc),
				GLEnumC.Cast(_desc.Stencil.Back.DepthFailFunc),
				GLEnumC.Cast(_desc.Stencil.Back.PassFunc)
			);
		} else {
			c.SetStencilTestEnabled(false);
		}

		c.SetPolygonMode(GLEnumC.Cast(_desc.Rasterization.PolygonMode));

		if (_desc.Rasterization.CullMode != CullMode.None) {
			c.SetCullFaceEnabled(true);
			c.SetCullFace(GLEnumC.Cast(_desc.Rasterization.CullMode));
		} else {
			c.SetCullFaceEnabled(false);
		}

		c.SetFrontFace(_desc.Rasterization.FrontFace == FrontFace.Clockwise ? GLEnum.CW : GLEnum.Ccw);

		if (_desc.Rasterization.DepthBiasEnable) {
			c.SetPolygonOffsetFillEnabled(true);
			c.SetPolygonOffsetFill(
				_desc.Rasterization.DepthBiasSlopeFactor,
				_desc.Rasterization.DepthBiasConstantFactor
			);
		} else {
			c.SetPolygonOffsetFillEnabled(false);
		}
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
