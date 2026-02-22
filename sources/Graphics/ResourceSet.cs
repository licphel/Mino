using Mino.Framework.Resource;

namespace Mino.Graphics;

/// <summary>
///     Storages and uploads a set of resources.
/// </summary>
public interface ResourceSet : ThreadContextHolder, IDisposable {
	/// <summary>
	///     The resource set layout.
	/// </summary>
	ResourceSetLayout Layout { get; }

	/// <summary>
	///     Binds a sampled texture to a slot.
	/// </summary>
	/// <param name="slot">Bound slot.</param>
	/// <param name="texture">Bound texture.</param>
	/// <param name="sampler">Bound sampler.</param>
	void BindTexture(int slot, Texture texture, Sampler sampler);

	/// <summary>
	///     Binds a uniform buffer to a slot.
	/// </summary>
	/// <param name="slot">Bound slot.</param>
	/// <param name="buffer">Bound buffer.</param>
	/// <param name="size">Buffer data size in bytes.</param>
	/// <param name="offset">Buffer data offset in bytes.</param>
	void BindUniform(int slot, BufferObject buffer, int size, int offset = 0);
}
