#region
using Mino.Algorithm;
using Mino.Framework;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Blits small textures to a bigger one to curb state changes.
/// </summary>
public class TextureAtlas : IDisposable {
	private const int InitialSize = 64;

	/*
	 *	We use maximum rectangle algorithm to manage insertions.
	 *	That may be not the best, but effective.
	 */
	private List<MaximumRect.RectI> _freeRects = new List<MaximumRect.RectI>();
	private Texture? _texture;
	private bool _init;
	private int _size;
	private bool _disposed;

	public void Init() {
		if (_init) {
			throw new Error("duplicated init");
		}
		_init = true;

		_size = InitialSize;
		_freeRects.Add(new MaximumRect.RectI(0, 0, _size, _size));

		// Upload a null texture.
		_texture = new Texture(
			new TextureDesc {
				InitialBytes = null,
				Format = TextureFormat.RedGreenBlueAlpha8,
				Width = _size,
				Height = _size,
				Type = TextureType.Texture2D
			});
	}

	private void expand() {
		int oldSize = _size;
		_size *= 2;

		// Generate new image and transfer data.
		Texture newTex = new Texture(
			new TextureDesc {
				Width = _size,
				Height = _size
			});
		Box2 cpyRegion = Box2.Create(0.0F, 0.0F, oldSize, oldSize);
		Blitter.Blit(_texture!, newTex, cpyRegion, cpyRegion);

		/*
		 * We swap these two handles to let
		 * the old texture get a new handle and new size,
		 * and the old handle can dispose with the new texture.
		 */
		{
			HandleRef.Swap(newTex._handle, _texture!._handle);
			_texture.Desc = newTex.Desc; // Update desc.
			newTex.Dispose();
		}

		// Add new free places.
		_freeRects.Add(new MaximumRect.RectI(oldSize, 0, oldSize, oldSize));
		_freeRects.Add(new MaximumRect.RectI(0, oldSize, oldSize, oldSize));
		_freeRects.Add(new MaximumRect.RectI(oldSize, oldSize, oldSize, oldSize));
	}

	/// <summary>
	///     Accepts an image and gets a part of the full texture.
	/// </summary>
	/// <param name="image">Image to insert.</param>
	/// <returns>A texture part, not ready for usage.</returns>
	/// <exception cref="Error">Thrown if not initialized or ended.</exception>
	public TexturePart Accept(Image image) {
		if (!_init || _disposed) {
			throw new Error("cannot accept");
		}
		// Expand till enough.
		MaximumRect.RectI dstRect;
		while (!MaximumRect.Find(_freeRects, image.Width, image.Height, out dstRect)) {
			expand();
		}

		// Copy image data.
		_texture!.Submit(
			new TextureSubmission {
				Bytes = image.Bytes,
				Region = (Box2) dstRect
			});

		return new TexturePart(_texture!, (Box2) dstRect);
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		// Texture itself has safeguarded dupe disposing.
		_texture?.Dispose();
		GC.SuppressFinalize(this);
	}
}
