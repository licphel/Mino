#region
using Mino.Mathematics;
using Mino.Nio;
using StbImageSharp;
using ColorComponents = StbImageSharp.ColorComponents;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Represents a 2D image.
/// </summary>
public interface Image : IDisposable {
	/// <summary>
	///     Image data, whose format may vary.
	/// </summary>
	public byte[]? Bytes { get; set; }

	/// <summary>
	///     Width of the image.
	/// </summary>
	public int Width { get; }

	/// <summary>
	///     Height of the image.
	/// </summary>
	public int Height { get; }

	/// <summary>
	///     Pixel stride bytes.
	/// </summary>
	public int PixelStride {
		get => 4;
	}

	/// <summary>
	///     Image pixel indexer.
	/// </summary>
	public ImagePixelProxy this[int x] {
		get => new ImagePixelProxy(x, this);
	}

	/// <summary>
	///     Creates a literal image.
	/// </summary>
	/// <param name="width">Image width.</param>
	/// <param name="height">Image height.</param>
	/// <param name="bytes">Image data.</param>
	/// <returns>A blank image.</returns>
	/// <exception cref="InvalidOperationException">Thrown if size is negative.</exception>
	public static Image Create(int width, int height, byte[]? bytes = null) {
		if (width < 0 || height < 0) {
			throw new InvalidOperationException("Negative size");
		}
		return new LiteralImage {
			Bytes = bytes ?? new byte[4 * width * height],
			Width = width,
			Height = height
		};
	}

	/// <summary>
	///     Parses an RGBA image data from a byte buffer.
	/// </summary>
	/// <param name="buffer">an untouched byte buffer</param>
	/// <returns>An image implementation.</returns>
	public static Image Parse(ByteBuffer buffer) {
		StbImage.stbi_set_flip_vertically_on_load(0);

		ImageResult? imageResult = ImageResult.FromMemory(buffer.BufferArray, ColorComponents.RedGreenBlueAlpha);
		LiteralImage img = new LiteralImage {
			Bytes = imageResult.Data,
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
		byte[]? data = img.Bytes;
		int width = img.Width;
		int height = img.Height;

		if (data == null || width <= 0 || height <= 0) {
			return;
		}

		int pix = img.PixelStride;
		int stride = width * pix;
		byte[] flippedData = new byte[data.Length];

		for (int y = 0; y < height; y++) {
			int sourceY = height - 1 - y;
			for (int x = 0; x < width; x++) {
				int sourceIndex = sourceY * stride + x * pix;
				int destIndex = y * stride + x * pix;

				for (int b = 0; b < pix; b++) {
					flippedData[destIndex + b] = data[sourceIndex + b];
				}
			}
		}

		img.Bytes = flippedData;
	}

	// Image pixel proxy used in pixel indexer.
	public class ImagePixelProxy {
		public int X;
		public byte[] Data;
		public int W;
		public int P;

		public ImagePixelProxy(int x, Image image) {
			X = x;
			Data = image.Bytes ?? throw new InvalidOperationException("No image data");
			W = image.Width;
			P = image.PixelStride;
		}

		public Color this[int y] {
			get {
				int i = P * (W * y + X);
				return Color.Create(Data[i++], Data[i++], Data[i++], Data[i]);
			}
			set {
				int i = P * (W * y + X);
				Data[i++] = (byte) Math.Clamp(value.Red * 255, 0, 255);
				Data[i++] = (byte) Math.Clamp(value.Green * 255, 0, 255);
				Data[i++] = (byte) Math.Clamp(value.Blue * 255, 0, 255);
				Data[i] = (byte) Math.Clamp(value.Alpha * 255, 0, 255);
			}
		}
	}

	/// <summary>
	///     STB image result.
	/// </summary>
	private sealed class LiteralImage : Image {
		public byte[]? Bytes { get; set; }
		public int Height { get; internal init; }
		public int Width { get; internal init; }

		public void Dispose() {
			// Nothing to do.
		}
	}
}
