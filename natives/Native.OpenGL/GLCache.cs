#region
using Mino.Mathematics;
using Silk.NET.OpenGL;
#endregion

namespace Mino.Native.OpenGL;

public class GLCache {
	private readonly GL _gl;

	// Cached GL states
	private uint _currentProgram;
	private Dictionary<GLEnum, uint> _currentBuffer = new Dictionary<GLEnum, uint>();
	private uint _currentVertexArray;
	private Dictionary<GLEnum, uint> _currentFramebuffer = new Dictionary<GLEnum, uint>();
	private Dictionary<(GLEnum, uint), uint> _currentTextures = new Dictionary<(GLEnum, uint), uint>();
	private uint[] _currentSamplers = new uint[16];
	private uint[] _currentUniformBuffers = new uint[16];
	private int[] _currentUniformBufferOffsets = new int[16];
	private int[] _currentUniformBufferSizes = new int[16];
	private Color _clearColor;
	private int _clearStencil;
	private double _clearDepth;

	// Enable/Disable states
	private bool _blendEnabled;
	private bool _depthTestEnabled;
	private bool _stencilTestEnabled;
	private bool _scissorTestEnabled;
	private bool _cullFaceEnabled;
	private bool _polygonOffsetFillEnabled;
	private float _polygonOffsetFillFactor;
	private float _polygonOffsetFillUnits;

	// Blend state
	private GLEnum _blendSrcColor = GLEnum.One;
	private GLEnum _blendDstColor = GLEnum.Zero;
	private GLEnum _blendSrcAlpha = GLEnum.One;
	private GLEnum _blendDstAlpha = GLEnum.Zero;
	private GLEnum _blendEquationColor = GLEnum.FuncAdd;
	private GLEnum _blendEquationAlpha = GLEnum.FuncAdd;
	private Color _blendConstant = Color.PureWhite;

	// Depth state
	private GLEnum _depthFunc = GLEnum.Less;
	private bool _depthMask = true;

	// Stencil state
	private uint _stencilMask = 0xFFFFFFFF;
	private GLEnum _stencilFrontFunc = GLEnum.Always;
	private GLEnum _stencilBackFunc = GLEnum.Always;
	private int _stencilFrontRef;
	private int _stencilBackRef;
	private uint _stencilReadMask = 0xFFFFFFFF;
	private GLEnum _stencilFrontFail = GLEnum.Keep;
	private GLEnum _stencilFrontZFail = GLEnum.Keep;
	private GLEnum _stencilFrontZPass = GLEnum.Keep;
	private GLEnum _stencilBackFail = GLEnum.Keep;
	private GLEnum _stencilBackZFail = GLEnum.Keep;
	private GLEnum _stencilBackZPass = GLEnum.Keep;

	// Rasterization state
	private GLEnum _polygonMode = GLEnum.Fill;
	private GLEnum _cullFace = GLEnum.Back;
	private GLEnum _frontFace = GLEnum.Ccw;

	// Scissor state
	private int _scissorX, _scissorY, _scissorWidth, _scissorHeight;

	// Viewport state
	private int _viewportX, _viewportY, _viewportWidth, _viewportHeight;

	// Active texture unit
	private uint _activeTextureUnit;

	public GLCache(GL gl) {
		_gl = gl;
	}

	public void SetProgram(uint program) {
		if (_currentProgram != program) {
			_currentProgram = program;
			_gl.UseProgram(_currentProgram);
		}
	}
	
	public void SetFramebuffer(GLEnum target, uint buffer) {
		_gl.BindFramebuffer(target, buffer);
	}

	public void SetBuffer(GLEnum target, uint buffer) {
		// Bug fixed: buffer should not be cached.
		
		//if (_currentBuffer.GetValueOrDefault(target) != buffer) {
			_currentBuffer[target] = buffer;
			_gl.BindBuffer(target, buffer);
		//}
	}

	public void SetVertexArray(uint vao) {
		if (_currentVertexArray != vao) {
			_currentVertexArray = vao;
			_gl.BindVertexArray(vao);
		}
	}

	public void SetActiveTexture(uint unit) {
		if (_activeTextureUnit != unit) {
			_activeTextureUnit = unit;
			_gl.ActiveTexture((TextureUnit) ((int) TextureUnit.Texture0 + unit));
		}
	}

	public void SetTexture(GLEnum target, uint unit, uint texture) {
		var key = (target, unit);
		
		if (_currentTextures.GetValueOrDefault(key) != texture) {
			_currentTextures[key] = texture;
			SetActiveTexture(unit);
			_gl.BindTexture(target, texture);
		}
	}

	public void SetSampler(uint unit, uint sampler) {
		if (_currentSamplers[unit] != sampler) {
			_currentSamplers[unit] = sampler;
			_gl.BindSampler(unit, sampler);
		}
	}

	public void SetUniformBuffer(uint index, uint buffer, int offset, int size) {
		if (_currentUniformBuffers[index] != buffer ||
		_currentUniformBufferOffsets[index] != offset ||
		_currentUniformBufferSizes[index] != size) {
			_currentUniformBuffers[index] = buffer;
			_currentUniformBufferOffsets[index] = offset;
			_currentUniformBufferSizes[index] = size;
			_gl.BindBufferRange(GLEnum.UniformBuffer, index, buffer, offset, (uint) size);
		}
	}

	public void SetBlendEnabled(bool enabled) {
		if (_blendEnabled != enabled) {
			_blendEnabled = enabled;
			if (enabled) {
				_gl.Enable(EnableCap.Blend);
			} else {
				_gl.Disable(EnableCap.Blend);
			}
		}
	}

	public void SetDepthTestEnabled(bool enabled) {
		if (_depthTestEnabled != enabled) {
			_depthTestEnabled = enabled;
			if (enabled) {
				_gl.Enable(EnableCap.DepthTest);
			} else {
				_gl.Disable(EnableCap.DepthTest);
			}
		}
	}

	public void SetStencilTestEnabled(bool enabled) {
		if (_stencilTestEnabled != enabled) {
			_stencilTestEnabled = enabled;
			if (enabled) {
				_gl.Enable(EnableCap.StencilTest);
			} else {
				_gl.Disable(EnableCap.StencilTest);
			}
		}
	}

	public void SetScissorTestEnabled(bool enabled) {
		if (_scissorTestEnabled != enabled) {
			_scissorTestEnabled = enabled;
			if (enabled) {
				_gl.Enable(EnableCap.ScissorTest);
			} else {
				_gl.Disable(EnableCap.ScissorTest);
			}
		}
	}

	public void SetCullFaceEnabled(bool enabled) {
		if (_cullFaceEnabled != enabled) {
			_cullFaceEnabled = enabled;
			if (enabled) {
				_gl.Enable(EnableCap.CullFace);
			} else {
				_gl.Disable(EnableCap.CullFace);
			}
		}
	}

	public void SetPolygonOffsetFillEnabled(bool enabled) {
		if (_polygonOffsetFillEnabled != enabled) {
			_polygonOffsetFillEnabled = enabled;
			if (enabled) {
				_gl.Enable(EnableCap.PolygonOffsetFill);
			} else {
				_gl.Disable(EnableCap.PolygonOffsetFill);
			}
		}
	}

	public void SetBlendFunc(GLEnum srcColor, GLEnum dstColor, GLEnum srcAlpha, GLEnum dstAlpha) {
		if (_blendSrcColor != srcColor || _blendDstColor != dstColor ||
		_blendSrcAlpha != srcAlpha || _blendDstAlpha != dstAlpha) {
			_blendSrcColor = srcColor;
			_blendDstColor = dstColor;
			_blendSrcAlpha = srcAlpha;
			_blendDstAlpha = dstAlpha;
			_gl.BlendFuncSeparate(srcColor, dstColor, srcAlpha, dstAlpha);
		}
	}

	public void SetBlendEquation(GLEnum color, GLEnum alpha) {
		if (_blendEquationColor != color || _blendEquationAlpha != alpha) {
			_blendEquationColor = color;
			_blendEquationAlpha = alpha;
			_gl.BlendEquationSeparate(color, alpha);
		}
	}

	public void SetBlendColor(Color color) {
		if (_blendConstant != color) {
			_blendConstant = color;
			_gl.BlendColor(color.Red, color.Green, color.Blue, color.Alpha);
		}
	}

	public void SetDepthFunc(GLEnum func) {
		if (_depthFunc != func) {
			_depthFunc = func;
			_gl.DepthFunc(func);
		}
	}

	public void SetDepthMask(bool write) {
		if (_depthMask != write) {
			_depthMask = write;
			_gl.DepthMask(write);
		}
	}

	public void SetStencilMask(uint mask) {
		if (_stencilMask != mask) {
			_stencilMask = mask;
			_gl.StencilMask(mask);
		}
	}

	public void SetStencilFunc(GLEnum frontFunc, GLEnum backFunc, int frontRef, int backRef, uint readMask) {
		if (_stencilFrontFunc != frontFunc || _stencilBackFunc != backFunc ||
		_stencilFrontRef != frontRef || _stencilBackRef != backRef ||
		_stencilReadMask != readMask) {
			_stencilFrontFunc = frontFunc;
			_stencilBackFunc = backFunc;
			_stencilFrontRef = frontRef;
			_stencilBackRef = backRef;
			_stencilReadMask = readMask;

			_gl.StencilFuncSeparate(TriangleFace.Front, frontFunc, frontRef, readMask);
			_gl.StencilFuncSeparate(TriangleFace.Back, backFunc, backRef, readMask);
		}
	}

	public void SetStencilOp(GLEnum frontFail, GLEnum frontZFail, GLEnum frontZPass,
		GLEnum backFail, GLEnum backZFail, GLEnum backZPass) {
		if (_stencilFrontFail != frontFail || _stencilFrontZFail != frontZFail ||
		_stencilFrontZPass != frontZPass || _stencilBackFail != backFail ||
		_stencilBackZFail != backZFail || _stencilBackZPass != backZPass) {
			_stencilFrontFail = frontFail;
			_stencilFrontZFail = frontZFail;
			_stencilFrontZPass = frontZPass;
			_stencilBackFail = backFail;
			_stencilBackZFail = backZFail;
			_stencilBackZPass = backZPass;

			_gl.StencilOpSeparate(TriangleFace.Front, frontFail, frontZFail, frontZPass);
			_gl.StencilOpSeparate(TriangleFace.Back, backFail, backZFail, backZPass);
		}
	}

	public void SetPolygonMode(GLEnum mode) {
		if (_polygonMode != mode) {
			_polygonMode = mode;
			_gl.PolygonMode(TriangleFace.FrontAndBack, mode);
		}
	}

	public void SetPolygonOffsetFill(float factor, float units) {
		if (Math.Abs(_polygonOffsetFillFactor - factor) > 10E-6 || Math.Abs(_polygonOffsetFillUnits - units) > 10E-6) {
			_polygonOffsetFillFactor = factor;
			_polygonOffsetFillUnits = units;
			_gl.PolygonOffset(factor, units);
		}
	}

	public void SetCullFace(GLEnum face) {
		if (_cullFace != face) {
			_cullFace = face;
			_gl.CullFace(face);
		}
	}

	public void SetFrontFace(GLEnum face) {
		if (_frontFace != face) {
			_frontFace = face;
			_gl.FrontFace(face);
		}
	}

	public void SetScissor(int x, int y, int width, int height) {
		if (_scissorX != x || _scissorY != y || _scissorWidth != width || _scissorHeight != height) {
			_scissorX = x;
			_scissorY = y;
			_scissorWidth = width;
			_scissorHeight = height;
			_gl.Scissor(x, y, (uint) width, (uint) height);
		}
	}

	public void SetViewport(int x, int y, int width, int height) {
		if (_viewportX != x || _viewportY != y || _viewportWidth != width || _viewportHeight != height) {
			_viewportX = x;
			_viewportY = y;
			_viewportWidth = width;
			_viewportHeight = height;
			_gl.Viewport(x, y, (uint) width, (uint) height);
		}
	}

	public void ClearColor(float r, float g, float b, float a) {
		Color clearColor = new Color(r, g, b, a);
		if (_clearColor != clearColor) {
			_clearColor = clearColor;
			_gl.ClearColor(r, g, b, a);
		}
	}

	public void ClearDepth(double depth) {
		if (Math.Abs(_clearDepth - depth) > 10E-8F) {
			_clearDepth = depth;
			_gl.ClearDepth(depth);
		}
	}

	public void ClearStencil(int stencil) {
		if (_clearStencil != stencil) {
			_clearStencil = stencil;
			_gl.ClearStencil(stencil);
		}
	}
}
