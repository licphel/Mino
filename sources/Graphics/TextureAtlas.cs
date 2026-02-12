using Mino.Algorithm;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Mino.Mathematics;

namespace Mino.Graphics;

/// <summary>
///     Blits small textures to a bigger one to curb state changes.
/// </summary>
public class TextureAtlas {
	private const int INITIAL_SIZE = 64;
	
	private bool _ended;

	/*
	 *	We use maximum rectangle algorithm to manage insertions.
	 *	That may be not the best, but effective.
	 */
	private List<MaximumRect.RectI> _freeRects = new List<MaximumRect.RectI>();
	private Texture? _texture;

	public Image? BufferImage;

	public void Init() {
		if (BufferImage != null || _ended) {
			throw new Error("already initialized");
		}

		BufferImage = Image.CreateEmpty(INITIAL_SIZE, INITIAL_SIZE);
		_freeRects.Add(new MaximumRect.RectI(0, 0, INITIAL_SIZE, INITIAL_SIZE));

		// Upload null texture.
		_texture = new Texture(
			new TextureDesc {
				Data = null,
				Format = TextureFormat.RedGreenBlueAlpha8,
				Width = INITIAL_SIZE,
				Height = INITIAL_SIZE,
				Type = TextureType.Texture2D
			});
	}

	private void expand() {
		Image tmp = BufferImage!;
		int size = tmp.Width;

		// Generate new image and transfer data.
		BufferImage = Image.CreateEmpty(size * 2, size * 2);
		Box2 cpyRegion = Box2.Create(0.0F, 0.0F, size, size);
		Blitter.BlockCopy(tmp, BufferImage, cpyRegion, cpyRegion);

		// Add new free places.
		_freeRects.Add(new MaximumRect.RectI(size, 0, size, size));
		_freeRects.Add(new MaximumRect.RectI(0, size, size, size));
		_freeRects.Add(new MaximumRect.RectI(size, size, size, size));
	}

	/// <summary>
	///     Accepts an image and gets a part of the full texture.
	/// </summary>
	/// <param name="image">Image to insert.</param>
	/// <returns>A texture part, not ready for usage.</returns>
	/// <exception cref="Error">Thrown if not initialized or ended.</exception>
	public TexturePart Accept(Image image) {
		if (BufferImage == null || _ended) {
			throw new Error("cannot accept");
		}
		// Expand till enough.
		MaximumRect.RectI dstRect;
		while (!MaximumRect.Find(_freeRects, image.Width, image.Height, out dstRect)) {
			expand();
		}

		// Copy image data.
		Blitter.BlockCopy(image, BufferImage, dstRect, Box2.Create(0.0F, 0.0F, image.Width, image.Height));
		return new TexturePart(_texture!, (Box2) dstRect);
	}

	/// <summary>
	///     Ends the atlas and submits accepted images.
	/// </summary>
	public void EndAccept() {
		if (BufferImage == null) {
			throw new Error("not initialized");
		}
		if (_ended) {
			return;
		}
		_ended = true;

		// Finally, upload data.
		_texture!.Submit(TextureDesc.CreateByImage2D(BufferImage));
	}
}
