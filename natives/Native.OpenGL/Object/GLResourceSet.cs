using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public class GLResourceSet {
	public List<Bound> _bounds = new List<Bound>();
	public GL _gl;
	public ResourceSetLayout _layout = ResourceSetLayout.Bake();

	public GLResourceSet(GL gl) {
		_gl = gl;
	}

	public void OnResourceSetLayout(in ResourceSetLayout layout) {
		// Set userdata.
		_layout = layout;
		// In gl, we do nothing but validate the layout.
		foreach (ResourceSetLayout.Slot slot in layout.Slots) {
			if (string.IsNullOrEmpty(slot.Name)) {
				throw new Error($"null name at slot {slot.Binding}");
			}
		}
	}

	public void Apply(GLBackend backend, GLPipeline pipeline) {
		uint program = pipeline._desc.ShaderProgram;
		uint nHandle = backend._programHeap.GetData(program)._handle;

		foreach (Bound b in _bounds) {
			switch (b.Type) {
				case ResourceType.UniformBuffer:
					ref GLBuffer _b = ref backend._bufferHeap.GetData(b.Resources[0]);

					uint uniformIndex = getUniformBlock(nHandle, b);
					_gl.UniformBlockBinding(nHandle, uniformIndex, b._glUnits);
					_gl.BindBufferRange(GLEnum.UniformBuffer, b._glUnits, _b._handle, b.Offset, (uint) b.Size);
					break;
				case ResourceType.Texture:
					_gl.ActiveTexture((TextureUnit) ((int) TextureUnit.Texture0 + b._glUnits));
					ref GLTexture _t = ref backend._textureHeap.GetData(b.Resources[0]);
					_gl.BindTexture(_t._target, _t._handle);
					ref GLSampler _s = ref backend._samplerHeap.GetData(b.Resources[1]);
					_gl.BindSampler(b._glUnits, _s._handle);

					// Bug fixed: use gl handle.
					int uniformLocation = getUniform(nHandle, b);
					_gl.OnUniformData(uniformLocation, (int) b._glUnits);
					break;
				default:
					throw new Error("invalid arg: " + nameof(b));
			}

		}
	}

	public void Rearrange(GLBackend backend) {
		foreach (Bound b in _bounds) {
			b._glUnits = b.Type switch {
				ResourceType.UniformBuffer => backend._ubId++,
				ResourceType.Texture => backend._texId++,
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

	public class Bound {
		public readonly int Offset;
		public readonly uint[] Resources;
		public readonly int Size;
		public readonly int Slot;
		public readonly ResourceType Type;
		public uint _glUnits;

		public Bound(ResourceType type, int slot, uint[] resources, int offset = 0, int size = 0) {
			Type = type;
			Slot = slot;
			Resources = resources;
			Offset = offset;
			Size = size;
		}
	}
}
