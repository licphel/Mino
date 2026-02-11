using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;

namespace Mino.Graphics;

/// <summary>
///     Storages and uploads a set of resources.
/// </summary>
public class ResourceSet : IDisposable {
	private RenderBackend _backend;
	private bool _disposed;
	private uint _handle;

	public ResourceSet(in ResourceSetLayout layout) {
		// Set userdata.
		Layout = layout;

		_backend = RenderSystem.GetBackend();
		_handle = _backend.ResourceSetGen();
		// Compile the rs.
		_backend.ResourceSetLayout(_handle, layout);
	}

	/// <summary>
	///     The resource set layout.
	/// </summary>
	public ResourceSetLayout Layout { get; }

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.ResourceSetDelete(_handle);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Binds a sampled texture to a slot.
	/// </summary>
	/// <param name="slot">Bound slot.</param>
	/// <param name="texture">Bound texture.</param>
	/// <param name="sampler">Bound sampler.</param>
	public void BindTexture(int slot, Texture texture, Sampler sampler) {
		assert(ResourceType.Texture, slot);
		_backend.ResourceSetBindTexture(_handle, slot, texture, sampler);
	}

	/// <summary>
	///     Binds a uniform buffer to a slot.
	/// </summary>
	/// <param name="slot">Bound slot.</param>
	/// <param name="buffer">Bound buffer.</param>
	/// <param name="size">Buffer data size in bytes.</param>
	/// <param name="offset">Buffer data offset in bytes.</param>
	/// <exception cref="Error">Thrown if size or offset is invalid.</exception>
	public void BindUniform(int slot, Buffer buffer, int size, int offset = 0) {
		assert(ResourceType.UniformBuffer, slot);
		if (size <= 0 || offset < 0) {
			throw new Error("no uniform data");
		}

		_backend.ResourceSetBindBuffer(
			_handle, slot, ResourceType.UniformBuffer, buffer, offset, size);
	}

	private void assert(ResourceType type, int slot) {
		if (slot < 0 || slot >= Layout.Slots.Length) {
			throw new Error("invalid slot");
		}
		// Dynamic validation.
		ResourceType expectedType = Layout.Slots[slot].Type;
		if (expectedType != type) {
			throw new Error($"validation failed: {expectedType} expected");
		}
	}

	// Finalizer in case.
	~ResourceSet() {
		Dispose();
	}

	// Implicit cast to native handle.
	public static implicit operator uint(ResourceSet obj) {
		return obj._handle;
	}
}
