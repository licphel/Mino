using Mino.Algorithm.Random;
using Mino.Framework;
using Mino.Framework.XPlatform;
using Mino.Graphics.Desktop;
using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Mino.Native.OpenGL.Command;
using Mino.Native.OpenGL.Object;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL;

public unsafe class GLBackend : RenderBackend, ServiceProvider {
	private bool _disposed;
	private GL _gl = null!;
	private bool _init;
	private Window _window = null!;

	public void Init(Window window) {
		if (_init) {
			return;
		}
		_init = true;
		_gl = GL.GetApi(window.GetOpaqueContext().GetProcAddress);
		_window = window;

		if (_window.Debug) {
			GLDbg.Enable(_gl);
		}

		// Initial NDC args.
		_gl.DepthRange(1.0, 0.0);

		if (window.Debug) {
			// Disturbs GL ids to let it differ from our handles.
			// This may expose some errors.

			// _disturbGLIDs();
		}
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_window.Dispose();
		GC.SuppressFinalize(this);
	}

	public void PollEvents() {
		if (_window.Debug) {
			// Handled elsewhere.
			return;
		}
		GLEnum err;
		while ((err = _gl.GetError()) != GLEnum.NoError) {
			throw new Error($"gl error raised '{err}'");
		}
	}

	public void FrameBegin() {
		// Do nothing.
	}

	public void FrameEnd() {
		_window.Present();
	}

	public uint GetUltimateRenderTarget() {
		return 0;
	}

	public uint BufferGen() {
		return _bufferHeap.Allocate(new GLBuffer(_gl, _gl.GenBuffer()));
	}

	public void BufferDelete(uint buffer) {
		uint handle = _bufferHeap.GetData(buffer)._handle;

		_gl.DeleteBuffer(handle);
		_bufferHeap.Free(buffer);
	}

	public void BufferAlloc<T>(uint buffer, in BufferDesc desc, ReadOnlySpan<T> data, int capacity)
		where T : unmanaged {
		_bufferHeap.GetData(buffer).OnBufferAlloc(desc, data, capacity);
	}

	public void BufferSubmit<T>(uint buffer, ReadOnlySpan<T> data, int offset) where T : unmanaged {
		_bufferHeap.GetData(buffer).OnBufferSubmit(data, offset);
	}

	public uint TextureGen() {
		return _textureHeap.Allocate(new GLTexture(_gl, _gl.GenTexture()));
	}

	public void TextureDelete(uint texture) {
		uint handle = _textureHeap.GetData(texture)._handle;

		_gl.DeleteTexture(handle);
		_textureHeap.Free(texture);
	}

	public void TextureData(uint texture, in TextureDesc desc) {
		_textureHeap.GetData(texture).OnTextureData(desc);
	}
	
	public void TextureSubmit(uint texture, in TextureSubmission submission) {
		_textureHeap.GetData(texture).OnTextureSubmit(submission);
	}

	public void TextureBlit(uint src, int srcX, int srcY, int srcW, int srcH, uint dst, int dstX, int dstY, int dstW,
		int dstH,
		TextureFilter filter) {
		ref GLTexture srcTex = ref _textureHeap.GetData(src);
		ref GLTexture dstTex = ref _textureHeap.GetData(dst);
		uint srcFBO = _gl.GenFramebuffer();
		uint dstFBO = _gl.GenFramebuffer();

		try {
			_gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, srcFBO);
			_gl.FramebufferTexture2D(
				FramebufferTarget.ReadFramebuffer,
				GLEnum.ColorAttachment0,
				srcTex._target,
				srcTex._handle,
				0
			);

			_gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, dstFBO);
			_gl.FramebufferTexture2D(
				FramebufferTarget.DrawFramebuffer,
				GLEnum.ColorAttachment0,
				dstTex._target,
				dstTex._handle,
				0
			);

			_gl.BlitFramebuffer(
				srcX, srcY, srcX + srcW, srcY + srcH,
				dstX, dstY, dstX + dstW, dstY + dstH,
				ClearBufferMask.ColorBufferBit,
				(GLEnum) GLEnumC.Cast(filter)
			);
		} finally {
			_gl.DeleteFramebuffer(srcFBO);
			_gl.DeleteFramebuffer(dstFBO);
			_gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
			_gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
		}

		// Regenerate mipmaps.
		_gl.BindTexture(GLEnum.Texture2D, dstTex._handle);
		_gl.GenerateMipmap(GLEnum.Texture2D);
	}

	public uint SamplerGen() {
		return _samplerHeap.Allocate(new GLSampler(_gl, _gl.GenSampler()));
	}

	public void SamplerDelete(uint sampler) {
		uint handle = _samplerHeap.GetData(sampler)._handle;

		_gl.DeleteSampler(handle);
		_samplerHeap.Free(sampler);
	}

	public void SamplerData(uint sampler, in SamplerDesc desc) {
		_samplerHeap.GetData(sampler).OnSamplerData(desc);
	}

	public uint ShaderModuleGen() {
		// ** Delayed handle gen ** //
		return _moduleHeap.Allocate(new GLShaderModule(_gl, 0));
	}

	public void ShaderModuleDelete(uint module) {
		uint handle = _moduleHeap.GetData(module)._handle;

		_gl.DeleteShader(handle);
		_moduleHeap.Free(module);
	}

	public void ShaderModuleCompile(uint module, in ShaderModuleDesc desc) {
		_moduleHeap.GetData(module).OnShaderModuleData(desc);
	}

	public uint ShaderProgramGen() {
		return _programHeap.Allocate(new GLShaderProgram(_gl, _gl.CreateProgram()));
	}

	public void ShaderProgramDelete(uint program) {
		uint handle = _programHeap.GetData(program)._handle;

		_gl.DeleteProgram(handle);
		_programHeap.Free(program);
	}

	public void ShaderProgramLink(uint program, in ShaderProgramDesc desc) {
		_programHeap.GetData(program).OnShaderProgramLink(desc, this);
	}

	public uint UniformGen(uint program, string name) {
		return (uint) _gl.GetUniformLocation(program, name);
	}

	public void UniformData<T>(uint program, uint uniform, in T data) where T : unmanaged {
		uint nHandle = _programHeap.GetData(program)._handle;

		_gl.UseProgram(nHandle);
		_gl.OnUniformData((int) uniform, data); // (Extension method)
	}

	public void UniformData<T>(uint program, uint uniform, ReadOnlySpan<T> data) where T : unmanaged {
		uint nHandle = _programHeap.GetData(program)._handle;

		_gl.UseProgram(nHandle);
		_gl.OnUniformData((int) uniform, data); // (Extension method)
	}

	public uint RenderTargetGen() {
		return _renderTargetHeap.Allocate(new GLRenderTarget(_gl, _gl.GenFramebuffer()));
	}

	public void RenderTargetDelete(uint renderTarget) {
		uint handle = _renderTargetHeap.GetData(renderTarget)._handle;

		_gl.DeleteFramebuffer(handle);
		_renderTargetHeap.Free(renderTarget);
	}

	public void RenderTargetData(uint renderTarget, in RenderTargetDesc desc) {
		_renderTargetHeap.GetData(renderTarget).OnRenderTargetData(desc);
	}

	public void RenderTargetBlit(uint src, int srcX, int srcY, int srcW, int srcH, uint dst, int dstX, int dstY,
		int dstW,
		int dstH, TextureFilter filter) {
		uint srcFBO = src == 0 ? 0 : _renderTargetHeap.GetData(src)._handle;
		uint dstFBO = dst == 0 ? 0 : _renderTargetHeap.GetData(dst)._handle;

		_gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, srcFBO);
		_gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, dstFBO);

		_gl.BlitFramebuffer(
			srcX, srcY, srcX + srcW, srcY + srcH,
			dstX, dstY, dstX + dstW, dstY + dstH,
			ClearBufferMask.ColorBufferBit,
			(GLEnum) GLEnumC.Cast(filter)
		);

		_gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
		_gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
	}

	public void RenderPassBegin(uint renderTarget, in RenderPassDesc desc) {
		if (renderTarget == 0 && _curFBO != 0) {
			_gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			_curFBO = 0;
		} else if (_renderTargetHeap.IsValid(renderTarget)) {
			ref GLRenderTarget rt = ref _renderTargetHeap.GetData(renderTarget);

			if (_curFBO != rt._handle) {
				_gl.BindFramebuffer(FramebufferTarget.Framebuffer, _curFBO = rt._handle);
				// Do not viewport here.
			}
		}

		uint clearMask = 0;

		if ((desc.Clear & ClearMask.Color) != 0) {
			clearMask |= (uint) GLEnum.ColorBufferBit;
			_gl.ClearColor(desc.ClearColor.Red, desc.ClearColor.Green, desc.ClearColor.Blue, desc.ClearColor.Alpha);
		}

		if ((desc.Clear & ClearMask.Depth) != 0) {
			clearMask |= (uint) GLEnum.DepthBufferBit;
			_gl.ClearDepth(desc.ClearDepth);
		}

		if ((desc.Clear & ClearMask.Stencil) != 0) {
			clearMask |= (uint) GLEnum.StencilBufferBit;
			_gl.ClearStencil(desc.ClearStencil);
		}

		if (clearMask != 0) {
			_gl.Clear(clearMask);
		}
	}

	public void RenderPassEnd() {
		if (_curFBO != 0) {
			_gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			_curFBO = 0;
		}
	}

	public uint RenderPipeGen() {
		return _pipeHeap.Allocate(new GLRenderPipe(_gl));
	}

	public void RenderPipeDelete(uint pipe) {
		// No according gl object.
		_pipeHeap.Free(pipe);
	}

	public void RenderPipeCompile(uint pipe, in RenderPipeDesc desc) {
		_pipeHeap.GetData(pipe).OnRenderPipeData(desc);
	}

	public uint ResourceSetGen() {
		return _resourceSetHeap.Allocate(new GLResourceSet(_gl));
	}

	public void ResourceSetDelete(uint set) {
		_resourceSetHeap.Free(set);
	}

	public void ResourceSetLayout(uint set, in ResourceSetLayout layout) {
		_resourceSetHeap.GetData(set).OnResourceSetLayout(layout);
	}

	public void ResourceSetBindBuffer(uint set, int slot, ResourceType type, uint buffer, int offset, int size) {
		_resourceSetHeap.GetData(set)._bounds.Add(new GLResourceSet.Bound(type, slot, [buffer], offset, size));
	}

	public void ResourceSetBindTexture(uint set, int slot, uint texture, uint sampler) {
		_resourceSetHeap.GetData(set)._bounds
			.Add(new GLResourceSet.Bound(ResourceType.Texture, slot, [texture, sampler]));
	}

	public uint EncoderGen() {
		return _encoderHeap.Allocate(new GLEncoder());
	}

	public void EncoderDelete(uint encoder) {
		// No according gl object.
		_encoderHeap.Free(encoder);
	}

	public void EncoderCompile(uint encoder, in EncoderDesc desc) {
		ref GLEncoder enc = ref _encoderHeap.GetData(encoder);
		// Set userdata.
		enc._desc = desc;
	}

	public void EncoderReset(uint encoder) {
		ref GLEncoder enc = ref _encoderHeap.GetData(encoder);
		// Clear commands for next record.
		enc._commands.Clear();
	}

	public void EncoderQueuedExecute(uint encoder) {
		List<GLC_Command> cmdList = _encoderHeap.GetData(encoder)._commands;
		// Just execute all commands.
		foreach (GLC_Command cmd in cmdList) {
			cmd.Execute(this);
		}
	}

	public void EncoderTopology(uint encoder, Topology topology) {
		_encoderHeap.GetData(encoder)._commands.Add(new GLC_SetTopology(topology));
	}

	public void EncoderBuffer(uint encoder, BufferType type, uint buffer) {
		_encoderHeap.GetData(encoder)._commands.Add(new GLC_BindBuffer(type, buffer));
	}

	public void EncoderResourceSet(uint encoder, int slot, uint set) {
		_encoderHeap.GetData(encoder)._commands.Add(new GLC_SetResourceSet(slot, set));
	}

	public void EncoderDraw(uint encoder, int vertexCount, int firstVertex) {
		_encoderHeap.GetData(encoder)._commands.Add(new GLC_Draw(vertexCount, firstVertex));
	}

	public void EncoderDrawIndexed(uint encoder, int indexCount, int firstIndex) {
		_encoderHeap.GetData(encoder)._commands.Add(new GLC_DrawIndexed(indexCount, firstIndex));
	}

	public void EncoderDispatch(uint encoder, uint x, uint y, uint z) {
		_encoderHeap.GetData(encoder)._commands.Add(new GLC_Dispatch(x, y, z));
	}

	public void EncoderViewport(uint encoder, int x, int y, int width, int height) {
		_encoderHeap.GetData(encoder)._commands.Add(new GLC_SetViewport(x, y, width, height));
	}

	public void EncoderScissor(uint encoder, in ScissorDesc desc) {
		_encoderHeap.GetData(encoder)._commands.Add(new GLC_SetScissor(desc));
	}

	public void EncoderRenderPipe(uint encoder, uint pipe) {
		_encoderHeap.GetData(encoder)._commands.Add(new GLC_SetRenderPipe(pipe));
	}

	private void _disturbGLIDs() {
		// Random disturbing levels.
		int lvl = RandomGenerator.Default.NextInt(5, 20);
		for (int i = 0; i < lvl; i++) {
			_gl.GenTexture();
			_gl.GenBuffer();
			_gl.GenFramebuffer();
			_gl.GenSampler();
			_gl.CreateShader(GLEnum.VertexShader);
			_gl.CreateProgram();
			_gl.GenVertexArray();
		}
	}

	private void preDraw(GLRenderPipe _p) {
		for (int i = 0; i < _p._desc.ResourceLayouts.Length; i++) {
			uint rs = _boundResourceSets[i];
			ref GLResourceSet _s = ref _resourceSetHeap.GetData(rs);
			_s.Rearrange(this);
			_s.Apply(this, _p);
		}
		// Reset for next arrangement.
		_texId = _ubId = 0;
	}

	#region HEAPS
	internal Heap<GLBuffer> _bufferHeap = new Heap<GLBuffer>();
	internal Heap<GLRenderPipe> _pipeHeap = new Heap<GLRenderPipe>();
	internal Heap<GLShaderProgram> _programHeap = new Heap<GLShaderProgram>();
	internal Heap<GLRenderTarget> _renderTargetHeap = new Heap<GLRenderTarget>();
	internal Heap<GLResourceSet> _resourceSetHeap = new Heap<GLResourceSet>();
	internal Heap<GLSampler> _samplerHeap = new Heap<GLSampler>();
	internal Heap<GLTexture> _textureHeap = new Heap<GLTexture>();
	internal Heap<GLShaderModule> _moduleHeap = new Heap<GLShaderModule>();
	internal Heap<GLEncoder> _encoderHeap = new Heap<GLEncoder>();
	#endregion

	#region STATES
	internal uint[] _boundBuffers = new uint[8];
	internal uint[] _boundResourceSets = new uint[8];
	internal uint _boundRenderPipe;
	internal GLEnum _currentPrimitive = GLEnum.Triangles;
	internal uint _texId = 0;
	internal uint _ubId = 0;
	private uint _curFBO = 0;
	#endregion

	#region ENCODER_EXECUTION
	internal void executeSetTopology(Topology topology) {
		_currentPrimitive = GLEnumC.Cast(topology);
	}

	internal void executeBindBuffer(BufferType type, uint buffer) {
		// Bug fixed: use gl handle.
		uint bHandle = _bufferHeap.GetData(buffer)._handle;
		_boundBuffers[(int) type] = bHandle;
	}

	internal void executeBindRenderPipe(uint pipe) {
		ref GLRenderPipe _p = ref _pipeHeap.GetData(pipe);
		_p.Apply(this);
		_boundRenderPipe = pipe;
	}

	internal void executeBindResourceSet(int slot, uint set) {
		_boundResourceSets[slot] = set;
	}

	internal void executeDraw(int vertexCount, int firstVertex) {
		// Bind pipe:
		// 1. States
		// 2. VAO
		// 3. VBO
		ref GLRenderPipe _p = ref _pipeHeap.GetData(_boundRenderPipe);
		uint vbo = _boundBuffers[(int) BufferType.Vertex];
		_gl.BindVertexArray(_p.FindVao(vbo));

		// Apply resources
		preDraw(_p);

		_gl.DrawArrays(
			_currentPrimitive,
			firstVertex,
			(uint) vertexCount
		);
	}

	internal void executeDrawIndexed(int indexCount, int firstIndex) {
		// Bind pipe:
		// 1. States
		// 2. VAO
		// 3. VBO
		// 4. EBO
		ref GLRenderPipe _p = ref _pipeHeap.GetData(_boundRenderPipe);
		uint vbo = _boundBuffers[(int) BufferType.Vertex];
		uint ebo = _boundBuffers[(int) BufferType.Index];
		_gl.BindVertexArray(_p.FindVao(vbo, ebo));
		/*
		 * Bug fixed: ebo binding
		 * I guess OpenGL do not cache ebo in vao? who knows.
		 */
		_gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

		// Apply resources
		preDraw(_p);

		_gl.DrawElements(
			_currentPrimitive,
			(uint) indexCount,
			GLEnum.UnsignedInt,
			(void*) (firstIndex * sizeof(uint))
		);
	}

	internal void executeDispatch(uint x, uint y, uint z) {
		// Bind pipe
		ref GLRenderPipe _p = ref _pipeHeap.GetData(_boundRenderPipe);

		// Apply resources
		preDraw(_p);

		_gl.DispatchCompute(x, y, z);
		_gl.MemoryBarrier(MemoryBarrierMask.AllBarrierBits);
	}

	internal void executeViewport(int x, int y, int width, int height) {
		_gl.Viewport(x, y, (uint) width, (uint) height);
	}

	internal void executeScissor(ScissorDesc desc) {
		if (desc.Enable) {
			_gl.Enable(EnableCap.ScissorTest);
			_gl.Scissor(desc.X, desc.Y, (uint) desc.Width, (uint) desc.Height);
		} else {
			_gl.Disable(EnableCap.ScissorTest);
		}
	}
	#endregion
}
