using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mino.Framework.Resource;
using Mino.Graphics;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public unsafe sealed class GLBufferObject : BufferObject {
	public GL _gl = null!;
	public GLContext _ctx = null!;
	public uint _handle;
	public bool _disposed;
	
	public BufferObjectDesc _desc;
	public GLEnum _hint;
	public GLEnum _target;

	[ResourceCreation]
	public GLBufferObject(in BufferObjectDesc desc) {
		_desc = desc;
	}

	public BufferObjectDesc Desc {
		get => _desc;
	}
	
	public int Capacity { get; set; }

	// By default, we allow expansion.
	public bool CanExpand { get; set; } = true;
	
	public void Allocate<T>(int capacity, ReadOnlySpan<T> data) where T : unmanaged {
		if (capacity < 0) {
			throw new Error("negative capacity");
		}
		
		int elementSize = Unsafe.SizeOf<T>();
		int byteCount = data.Length * elementSize;

		/*
		 * No extra copy buffer array
		 */
		byte[] buf = GC.AllocateUninitializedArray<byte>(byteCount);
		MemoryMarshal.Cast<T, byte>(data).CopyTo(buf.AsSpan(0, byteCount));
		
		_ctx.Pend(() => {
			_gl.BindBuffer(_target, _handle);
			_gl.BufferData<byte>(_target, (uint) capacity, buf, _hint);
			_gl.BindBuffer(_target, 0);

			Capacity = capacity;
		});
	}
	
	public void Submit<T>(ReadOnlySpan<T> data, int offset = 0) where T : unmanaged {
		if (data.IsEmpty) {
			return;
		}
		if (offset < 0) {
			throw new Error("negative offset");
		}

		int elementSize = Unsafe.SizeOf<T>();
		int byteCount = data.Length * elementSize;
		int byteOffset = offset * elementSize;
		
		// Need to expand.
		if (byteOffset + byteCount > Capacity) {
			if (!CanExpand) {
				throw new Error("expand disabled");
			}

			int newcap = Math.Max(byteOffset + byteCount, Capacity == 0 ? byteCount : Capacity * 2);
			// Reallocate.
			Allocate<byte>(newcap, null);
			Capacity = newcap;
		}
		
		/*
		 * No extra copy buffer array
		 */
		byte[] buf = GC.AllocateUninitializedArray<byte>(byteCount);
		MemoryMarshal.Cast<T, byte>(data).CopyTo(buf.AsSpan(0, byteCount));
		
		_ctx.Pend(() => {
			_gl.BindBuffer(_target, _handle);
			fixed (void* ptr = buf) {
				bool shouldOrphan = _desc.Frequency == BufferFrequency.Stream ||
					_desc.Frequency == BufferFrequency.Dynamic && byteCount > Capacity / 4;
				if (shouldOrphan) {
					// Orphaning.
					_gl.BufferData(_target, (UIntPtr) Capacity, null, _hint);
					_gl.BufferSubData(_target, byteOffset, (UIntPtr) byteCount, ptr);
				} else if (byteOffset == 0 && byteCount == Capacity) {
					// Wholely cover.
					_gl.BufferData(_target, (UIntPtr) byteCount, ptr, _hint);
				} else {
					// Sub data.
					_gl.BufferSubData(_target, byteOffset, (UIntPtr) byteCount, ptr);
				}
			}
			_gl.BindBuffer(_target, 0);
		});
	}
	
	public bool TryGetThreadContext(out ThreadContext ctx) {
		ctx = _ctx;
		return true;
	}
	
	public void Listen(ThreadContext ctx) {
		_ctx = (GLContext) ctx;
		_gl = _ctx._gl;
		
		_ctx.Pend(() => {
			_handle = _gl.GenBuffer();
			
			_target = GLEnumC.Cast(_desc.Type);
			_hint = GLEnumC.Cast(_desc.Usage, _desc.Frequency);
		});
	}
	
	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		
		_ctx.Pend(() => {
			_gl.DeleteBuffer(_handle);
		});
	}
}
