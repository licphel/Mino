using Mino.Desktop;
using Mino.Framework.Resource;
using Mino.Graphics;
using Mino.Native.OpenGL.Object;
using Mino.Utility.Logging;
using Silk.NET.OpenGL;
using Sampler = Mino.Graphics.Sampler;
using Texture = Mino.Graphics.Texture;

namespace Mino.Native.OpenGL;

public class GLContext : AbstractThreadContext {
	public GL _gl = null!;
	public Window _window = null!;
	public GLExecutionContext _exeCtx = null!;
	public GLCache _cache = null!;
	
	public override void PollEvents() {
		// Send a pending event.
		Pend(() => {
			GLEnum err;
			while ((err = _gl.GetError()) != GLEnum.NoError) {
				Log.Warn($"OpenGL error raised: '{err}'");
			}
		});
	}

	protected override void OnInit() {
		Factory.RegisterInterface<BufferObject, GLBufferObject>(injector);
		Factory.RegisterInterface<Texture, GLTexture>(injector);
		Factory.RegisterInterface<ShaderModule, GLShaderModule>(injector);
		Factory.RegisterInterface<ShaderProgram, GLShaderProgram>(injector);
		Factory.RegisterInterface<Sampler, GLSampler>(injector);
		Factory.RegisterInterface<RenderPipe, GLRenderPipe>(injector);
		Factory.RegisterInterface<Encoder, GLEncoder>(injector);
		Factory.RegisterInterface<ResourceSet, GLResourceSet>(injector);
		Factory.RegisterInterface<RenderTarget, GLRenderTarget>(injector);
		return;
		
		void injector(ThreadContextHolder h) {
			h.Listen(this);
		}
	}
	
	protected override void OnContextStart() {
		_window = RenderSystem.GetWindow();
		_window.MakeContextCurrent();
		
		_gl = GL.GetApi(_window.GetProcAddress().GetProcAddress);
		_cache = new GLCache(_gl);
		_exeCtx = new GLExecutionContext(_gl, _window, _cache);
		
		if (_window.Debug) {
			GLDebug.Enable(_gl);
		}

		// Initial NDC args.
		// Default 0 near 1 far.
		_gl.DepthRange(0.0, 1.0);
		
		Log.Info("OpenGL context was successfully initialized");
	}
	
	protected override void OnContextStop() {
		// Do nothing.
	}
	
	protected override void OnDispose() {
		_gl.Dispose();
		_window.Dispose();
	}
}
