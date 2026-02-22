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
	
	public uint FindVaoDx(uint vbo, uint ebo = 0) {
		(uint vbo, uint ebo) key = (vbo, ebo);

		if (!_vaoCache.TryGetValue(key, out uint vao)) {
			vao = _gl.GenVertexArray();
			_gl.BindVertexArray(vao);

			_gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
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
				_gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
			}

			_gl.BindVertexArray(0);
			_vaoCache[key] = vao;
		}

		return vao;
	}

	public void ApplyDx() {
		if (_desc.ShaderProgram != null) {
			GLShaderProgram sp = (GLShaderProgram) _desc.ShaderProgram;
			_gl.UseProgram(sp._handle);
		}

		if (_desc.Blend.Enable) {
			_gl.Enable(EnableCap.Blend);
			_gl.BlendFuncSeparate(
				GLEnumC.Cast(_desc.Blend.SrcColor),
				GLEnumC.Cast(_desc.Blend.DstColor),
				GLEnumC.Cast(_desc.Blend.SrcAlpha),
				GLEnumC.Cast(_desc.Blend.DstAlpha)
			);
			_gl.BlendEquationSeparate(
				GLEnumC.Cast(_desc.Blend.ColorFunc),
				GLEnumC.Cast(_desc.Blend.AlphaFunc)
			);
			_gl.BlendColor(
				_desc.Blend.Constant.Red,
				_desc.Blend.Constant.Green,
				_desc.Blend.Constant.Blue,
				_desc.Blend.Constant.Alpha
			);
		} else {
			_gl.Disable(EnableCap.Blend);
		}

		if (_desc.Depth.DepthTest) {
			_gl.Enable(EnableCap.DepthTest);
			_gl.DepthFunc(GLEnumC.Cast(_desc.Depth.DepthCompare));
			_gl.DepthMask(_desc.Depth.DepthWrite);
		} else {
			_gl.Disable(EnableCap.DepthTest);
			_gl.DepthMask(false);
		}

		if (_desc.Stencil.StencilTest) {
			_gl.Enable(EnableCap.StencilTest);
			_gl.StencilMask(_desc.Stencil.StencilWriteMask);
			_gl.StencilFuncSeparate(
				TriangleFace.Front,
				GLEnumC.Cast(_desc.Stencil.Front.CompareOp),
				0,
				_desc.Stencil.StencilReadMask
			);
			_gl.StencilFuncSeparate(
				TriangleFace.Back,
				GLEnumC.Cast(_desc.Stencil.Back.CompareOp),
				0,
				_desc.Stencil.StencilReadMask
			);
			_gl.StencilOpSeparate(
				TriangleFace.Front,
				GLEnumC.Cast(_desc.Stencil.Front.FailFunc),
				GLEnumC.Cast(_desc.Stencil.Front.DepthFailFunc),
				GLEnumC.Cast(_desc.Stencil.Front.PassFunc)
			);
			_gl.StencilOpSeparate(
				TriangleFace.Back,
				GLEnumC.Cast(_desc.Stencil.Back.FailFunc),
				GLEnumC.Cast(_desc.Stencil.Back.DepthFailFunc),
				GLEnumC.Cast(_desc.Stencil.Back.PassFunc)
			);
		} else {
			_gl.Disable(EnableCap.StencilTest);
		}

		_gl.PolygonMode(TriangleFace.FrontAndBack, GLEnumC.Cast(_desc.Rasterization.PolygonMode));

		if (_desc.Rasterization.CullMode != CullMode.None) {
			_gl.Enable(EnableCap.CullFace);
			_gl.CullFace(GLEnumC.Cast(_desc.Rasterization.CullMode));
		} else {
			_gl.Disable(EnableCap.CullFace);
		}

		_gl.FrontFace(
			_desc.Rasterization.FrontFace == FrontFace.Clockwise ? FrontFaceDirection.CW : FrontFaceDirection.Ccw);

		if (_desc.Rasterization.DepthBiasEnable) {
			_gl.Enable(EnableCap.PolygonOffsetFill);
			_gl.PolygonOffset(
				_desc.Rasterization.DepthBiasSlopeFactor,
				_desc.Rasterization.DepthBiasConstantFactor
			);
		} else {
			_gl.Disable(EnableCap.PolygonOffsetFill);
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
