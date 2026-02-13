using Mino.Mathematics;

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///		A texture submission data.
/// </summary>
public struct TextureSubmission {
	public byte[]? Bytes;
	public Box3 Region;

	public TextureSubmission() {
		Bytes = null;
		Region = default;
	}
	
	/// <summary>
	///     Creates a submission by a 2D image.
	/// </summary>
	/// <param name="image">The source image.</param>
	/// <returns>A texture submission data.</returns>
	public static TextureSubmission CreateByImage(Image image) {
		return new TextureSubmission {
			Bytes = image.Bytes,
			Region = Box3.Create(0.0F, 0.0F, 0.0F, image.Width, image.Height, 0.0F)
		};
	}
}
