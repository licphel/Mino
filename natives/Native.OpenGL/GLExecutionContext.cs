using Mino.Desktop;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
using Mino.Native.OpenGL.Object;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL;

public unsafe sealed class GLExecutionContext {
	public GL _gl;
	public Window _window;
	public GLCache _c;
	public GLBufferObject[] _boundBuffers = new GLBufferObject[8];
	public GLResourceSet[] _boundResourceSets = new GLResourceSet[8];
	public GLRenderPipe? _boundPipe;
	public GLEnum _currentPrimitive = GLEnum.Triangles;
	public uint _texId = 0;
	public uint _ubId = 0;

	public GLExecutionContext(GL gl, Window window, GLCache cache) {
		_gl = gl;
		_window = window;
		_c = cache;
	}
	
	public void ExecuteSetTopology(Topology topology) {
		_currentPrimitive = GLEnumC.Cast(topology);
	}

	public void ExecuteBindBuffer(GLBufferObject buffer) {
		_boundBuffers[(int) buffer._desc.Type] = buffer;
	}

	public void ExecuteBindRenderPipe(GLRenderPipe pipe) {
		pipe.ApplyDx();
		_boundPipe = pipe;
	}

	public void ExecuteBindResourceSet(int slot, GLResourceSet set) {
		_boundResourceSets[slot] = set;
	}

	public void ExecuteDraw(int vertexCount, int firstVertex) {
		if (_boundPipe == null) {
			return;
		}
		// Bind pipe:
		// 1. States
		// 2. VAO
		// 3. VBO
		GLBufferObject vbo = _boundBuffers[(int) BufferType.Vertex];
		_c.SetVertexArray(_boundPipe.FindVaoDx(_c, vbo._handle));

		// Apply resources
		ResourceDx(_boundPipe);

		_gl.DrawArrays(
			_currentPrimitive,
			firstVertex,
			(uint) vertexCount
		);
	}

	public void ExecuteDrawIndexed(int indexCount, int firstIndex) {
		if (_boundPipe == null) {
			return;
		}
		// Bind pipe:
		// 1. States
		// 2. VAO
		// 3. VBO
		// 4. EBO
		GLBufferObject vbo = _boundBuffers[(int) BufferType.Vertex];
		GLBufferObject ebo = _boundBuffers[(int) BufferType.Index];
		_c.SetVertexArray(_boundPipe.FindVaoDx(_c, vbo._handle, ebo._handle));
		/*
		 * Bug fixed: ebo binding
		 * I guess OpenGL do not cache ebo in vao? who knows.
		 */
		_c.SetBuffer(GLEnum.ElementArrayBuffer, ebo._handle);

		// Apply resources
		ResourceDx(_boundPipe);
		
		_gl.DrawElements(
			_currentPrimitive,
			(uint) indexCount,
			GLEnum.UnsignedInt,
			(void*) (firstIndex * sizeof(uint))
		);
	}

	public void ExecuteDispatch(uint x, uint y, uint z) {
		if (_boundPipe == null) {
			return;
		}
		// Apply resources
		ResourceDx(_boundPipe);

		_gl.DispatchCompute(x, y, z);
		_gl.MemoryBarrier(MemoryBarrierMask.AllBarrierBits);
	}

	public void ExecuteViewport(int x, int y, int width, int height) {
		int newMinY = -height - y + (int) _window.Size.Y;
		_c.SetViewport(x, newMinY, width, height);
	}

	public void ExecuteScissor(ScissorDesc desc) {
		if (desc.Enable) {
			_c.SetScissorTestEnabled(true);
			int newMinY = -desc.Height - desc.Y + (int) _window.Size.Y;
			_c.SetScissor(desc.X, newMinY, desc.Width, desc.Height);
		} else {
			_c.SetScissorTestEnabled(false);
		}
	}
	
	public void ResourceDx(GLRenderPipe _p) {
		for (int i = 0; i < _p._desc.ResourceLayouts.Length; i++) {
			GLResourceSet rs = _boundResourceSets[i];
			rs.RearrangeDx(this);
			rs.ApplyDx(_p);
		}
		// Reset for next arrangement.
		_texId = _ubId = 0;
	}
}
