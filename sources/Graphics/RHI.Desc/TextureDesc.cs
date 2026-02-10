using Mino.Graphics.RHI.Enum;

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     Describes a texture.
/// </summary>
public record struct TextureDesc {
	public byte[]? Data;
	public int Depth;
	public TextureFormat Format;
	public int Height;
	public int MipLevels;
	public TextureType Type;
	public int Width;

	public TextureDesc() {
		Data = null;
		Depth = 0;
		Format = TextureFormat.RedGreenBlueAlpha8;
		Height = 0;
		Type = TextureType.Texture2D;
		Width = 0;
		MipLevels = 0;
	}

	/// <summary>
	///     Creates a descriptor by a 2D image.
	/// </summary>
	/// <param name="image">The source image.</param>
	/// <returns>A texture descriptor.</returns>
	public static TextureDesc CreateByImage2D(Image image) {
		return new TextureDesc {
			Data = image.Data,
			Width = image.Width,
			Height = image.Height
		};
	}
}
