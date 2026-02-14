using Mino.Graphics.RHI.Enum;

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     Describes a texture.
/// </summary>
public record struct TextureDesc {
	public int Width;
	public int Height;
	public int Depth;
	public byte[]? InitialBytes;
	public TextureFormat Format;
	public TextureType Type;
	public int MipLevels;

	public TextureDesc() {
		InitialBytes = null;
		Width = 0;
		Height = 0;
		Depth = 0;
		Format = TextureFormat.RedGreenBlueAlpha8;
		Type = TextureType.Texture2D;
		MipLevels = 0;
	}

	/// <summary>
	///     Creates a desc by a 2D image.
	/// </summary>
	/// <param name="image">The source image.</param>
	/// <returns>A texture desc.</returns>
	public static TextureDesc CreateByImage(Image image) {
		return new TextureDesc {
			Width = image.Width,
			Height = image.Height,
			Depth = 0,
			InitialBytes = image.Bytes,
			Format = TextureFormat.RedGreenBlueAlpha8,
			Type = TextureType.Texture2D,
			MipLevels = 0
		};
	}
}
