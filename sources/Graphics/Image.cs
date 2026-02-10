using Mino.Nio;
using StbImageSharp;
using ColorComponents = StbImageSharp.ColorComponents;

namespace Mino.Graphics;

/// <summary>
///     Represents a 2D image.
/// </summary>
public interface Image : IDisposable {
	/// <summary>
	///     Image data, whose format may vary.
	/// </summary>
	public byte[]? Data { get; set; }

	/// <summary>
	///     Width of the image.
	/// </summary>
	public int Width { get; }

	/// <summary>
	///     Height of the image.
	/// </summary>
	public int Height { get; }

	/// <summary>
	///     Parses an RGBA image data from a byte buffer.
	/// </summary>
	/// <param name="buffer">an untouched byte buffer</param>
	/// <returns>An image implementation.</returns>
	public static Image Parse(ByteBuffer buffer) {
		StbImage.stbi_set_flip_vertically_on_load(0);

		ImageResult? imageResult = ImageResult.FromMemory(buffer.BufferArray, ColorComponents.RedGreenBlueAlpha);
		LiteralImage img = new LiteralImage {
			Data = imageResult.Data,
			Width = imageResult.Width,
			Height = imageResult.Height
		};
		return img;
	}

	/// <summary>
	///     Flips the given data.
	/// </summary>
	/// <param name="img">A 2D RGBA image.</param>
	public static void FlipImage2D(Image img) {
		byte[]? data = img.Data;
		int width = img.Width;
		int height = img.Height;

		if (data == null || width <= 0 || height <= 0) {
			return;
		}

		int stride = width * 4;
		byte[] flippedData = new byte[data.Length];

		for (int y = 0; y < height; y++) {
			int sourceY = height - 1 - y;
			for (int x = 0; x < width; x++) {
				int sourceIndex = sourceY * stride + x * 4;
				int destIndex = y * stride + x * 4;

				for (int b = 0; b < 4; b++) {
					flippedData[destIndex + b] = data[sourceIndex + b];
				}
			}
		}

		img.Data = flippedData;
	}

	/// <summary>
	///     STB image result.
	/// </summary>
	private class LiteralImage : Image {
		public byte[]? Data { get; set; }
		public int Height { get; internal init; }
		public int Width { get; internal init; }

		public void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}
