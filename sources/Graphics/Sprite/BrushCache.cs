#region
using Mino.Nio;
#endregion

namespace Mino.Graphics.Sprite;

/// <summary>
///     Brush draw-to cache.
/// </summary>
public interface BrushCache {
	public ByteBuffer VertexBuf { get; }
	public ByteBuffer IndexBuf { get; }

	/// <summary>
	///     Creates a normal closed cache.
	/// </summary>
	/// <returns>A brush cache.</returns>
	public static BrushCache CreateNormal() {
		return new Self();
	}

	/// <summary>
	///     Self cache, no additional output.
	/// </summary>
	internal class Self : BrushCache {
		public ByteBuffer VertexBuf { get; } = new ByteBuffer().With(Endianness.Native);
		public ByteBuffer IndexBuf { get; } = new ByteBuffer().With(Endianness.Native);
	}
}
