using Mino.Framework.Resource;
using Mino.Graphics;
using Mino.Graphics.Enum;
using Silk.NET.OpenGL;
using Sampler = Mino.Graphics.Sampler;
using Texture = Mino.Graphics.Texture;

namespace Mino.Native.OpenGL.Object;

public sealed class GLResourceSet : ResourceSet {
	public GL _gl = null!;
	public GLContext _ctx = null!;
	public bool _disposed;
	
	public ResourceSetLayout _layout;
	// Bug fixed: resource set List<Bound> behaves wrongly.
	// Use array instead.
	public Bound[] _bounds;
	
	[ResourceCreation]
	public GLResourceSet(in ResourceSetLayout layout) {
		_layout = layout;
		
		foreach (ResourceSetLayout.Slot slot in layout.Slots) {
			if (string.IsNullOrEmpty(slot.Name)) {
				throw new Error($"null name at slot {slot.Binding}");
			}
		}
		// Init bounds.
		_bounds = new Bound[layout.Slots.Length];
	}

	public ResourceSetLayout Layout {
		get => _layout;
	}

	public void BindTexture(int slot, Texture texture, Sampler sampler) {
		_ctx.Pend(() => {
			_bounds[slot] = new Bound(ResourceType.Texture, slot, [texture, sampler]);
		});
	}
	
	public void BindUniform(int slot, BufferObject buffer, int size, int offset = 0) {
		_ctx.Pend(() => {
			_bounds[slot] = new Bound(ResourceType.UniformBuffer, slot, [buffer], offset, size);
		});
	}
	
	public void ApplyDx(GLRenderPipe pipe) {
		if (pipe._desc.ShaderProgram is not GLShaderProgram sp) {
			return;
		}
		uint program = sp._handle;

		foreach (Bound b in _bounds) {
			switch (b.Type) {
				case ResourceType.UniformBuffer:
					uint uniformIndex = getUniformBlock(program, b);
					_gl.UniformBlockBinding(program, uniformIndex, b._glUnits);
					GLBufferObject buf = (GLBufferObject) b.Resources[0];
					_gl.BindBufferRange(GLEnum.UniformBuffer, b._glUnits, buf._handle, b.Offset, (uint) b.Size);
					break;
				case ResourceType.Texture:
					_gl.ActiveTexture((TextureUnit) ((int) TextureUnit.Texture0 + b._glUnits));
					GLTexture tex = (GLTexture) b.Resources[0];
					_gl.BindTexture(tex._target, tex._handle);
					GLSampler samp = (GLSampler) b.Resources[1];
					_gl.BindSampler(b._glUnits, samp._handle);

					// Bug fixed: use gl handle.
					int uniformLocation = getUniform(program, b);
					_gl.Uniform1(uniformLocation, (int) b._glUnits);
					break;
				default:
					throw new Error("invalid arg: " + nameof(b));
			}

		}
	}

	public void RearrangeDx(GLExecutionContext ctx) {
		foreach (Bound b in _bounds) {
			b._glUnits = b.Type switch {
				ResourceType.UniformBuffer => ctx._ubId++,
				ResourceType.Texture => ctx._texId++,
				_ => throw new Error("invalid arg: " + nameof(b))
			};
		}
	}

	private int getUniform(uint programNative, in Bound b) {
		return _gl.GetUniformLocation(programNative, _layout.Slots[b.Slot].Name);
	}

	private uint getUniformBlock(uint programNative, in Bound b) {
		return _gl.GetUniformBlockIndex(programNative, _layout.Slots[b.Slot].Name);
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
	
	public class Bound {
		public readonly int Offset;
		public readonly object[] Resources;
		public readonly int Size;
		public readonly int Slot;
		public readonly ResourceType Type;
		public uint _glUnits;

		public Bound(ResourceType type, int slot, object[] resources, int offset = 0, int size = 0) {
			Type = type;
			Slot = slot;
			Resources = resources;
			Offset = offset;
			Size = size;
		}
	}
}
