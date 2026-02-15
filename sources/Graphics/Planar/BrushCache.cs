#region
using Mino.Nio;
#endregion

namespace Mino.Graphics.Planar;

/// <summary>
///     Brush draw-to cache.
/// </summary>
public interface BrushCache {
	public ByteBuffer VertexBuf { get; }
	public ByteBuffer IndexBuf { get; }

	/// <summary>
	///     Self cache, no additional output.
	/// </summary>
	public class Self : BrushCache {
		public ByteBuffer VertexBuf { get; } = new ByteBuffer();
		public ByteBuffer IndexBuf { get; } = new ByteBuffer();

		public Self() {
			// Set to native endianness as gfx api expected.
			VertexBuf.Endianness = Endianness.Native;
			IndexBuf.Endianness = Endianness.Native;
		}
	}
}
