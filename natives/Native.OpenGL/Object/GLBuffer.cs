using System.Runtime.CompilerServices;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public unsafe class GLBuffer {
	public int _capacity;
	public BufferDesc _desc;
	public GL _gl;
	public uint _handle;
	public GLEnum _hint;
	public GLEnum _target;

	public GLBuffer(GL gl, uint handle) {
		_gl = gl;
		_handle = handle;
	}

	public void OnBufferAlloc<T>(in BufferDesc desc, ReadOnlySpan<T> data, int capacity) where T : unmanaged {
		// Set userdata.
		_desc = desc;
		// Cache enum value.
		_target = GLEnumC.Cast(desc.Type);
		_hint = GLEnumC.Cast(desc.Usage, desc.Frequency);

		_gl.BindBuffer(_target, _handle);
		_gl.BufferData(_target, (uint) capacity, data, _hint);
		_gl.BindBuffer(_target, 0);

		_capacity = capacity;
	}

	public void OnBufferSubmit<T>(ReadOnlySpan<T> data, int offset) where T : unmanaged {
		if (data.IsEmpty) {
			return;
		}

		int elementSize = Unsafe.SizeOf<T>();
		int byteCount = data.Length * elementSize;
		int byteOffset = offset * elementSize;

		// We do not consider buffer capacity here.
		// Upper layer will do it.

		_gl.BindBuffer(_target, _handle);
		fixed (void* ptr = data) {
			bool shouldOrphan = _desc.Frequency == BufferFrequency.Stream ||
				_desc.Frequency == BufferFrequency.Dynamic && byteCount > _capacity / 4;
			if (shouldOrphan) {
				// Orphaning.
				_gl.BufferData(_target, (UIntPtr) _capacity, null, _hint);
				_gl.BufferSubData(_target, byteOffset, (UIntPtr) byteCount, ptr);
			} else if (byteOffset == 0 && byteCount == _capacity) {
				// Wholely cover.
				_gl.BufferData(_target, (UIntPtr) byteCount, ptr, _hint);
			} else {
				// Sub data.
				_gl.BufferSubData(_target, byteOffset, (UIntPtr) byteCount, ptr);
			}
		}
		_gl.BindBuffer(_target, 0);
	}
}
