#region
using System.Runtime.InteropServices;
using Mino.Graphics.Hardware.Desc;
using Mino.Graphics.Hardware.Enum;
using Mino.Mathematics;
using HandleRef = Mino.Framework.HandleRef;
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
	private List<RectI> _freeRects = new List<RectI>();
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
		_freeRects.Add(new RectI(0, 0, _size, _size));

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
		_freeRects.Add(new RectI(oldSize, 0, oldSize, oldSize));
		_freeRects.Add(new RectI(0, oldSize, oldSize, oldSize));
		_freeRects.Add(new RectI(oldSize, oldSize, oldSize, oldSize));
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
		RectI dstRect;
		while (!find(image.Width, image.Height, out dstRect)) {
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
	
	private bool find(int width, int height, out RectI result, int padding = 1) {
		int best = -1;
		int bestScore = int.MaxValue;

		for (int i = 0; i < _freeRects.Count; i++) {
			ref RectI fr = ref CollectionsMarshal.AsSpan(_freeRects)[i];
			if (fr.Width < width + padding || fr.Height < height + padding) {
				continue;
			}

			int score = Math.Min(
				fr.Width - (width + padding),
				fr.Height - (height + padding)
			);

			if (score < bestScore) {
				bestScore = score;
				best = i;
			}
		}

		if (best == -1) {
			result = new RectI();
			return false;
		}

		RectI used = _freeRects[best];
		int dx = used.X;
		int dy = used.Y;

		int remainW = used.Width - (width + padding);
		int remainH = used.Height - (height + padding);

		RectI right1 = new RectI(used.X + width + padding, used.Y, remainW, height + padding);
		RectI top1 = new RectI(used.X, used.Y + height + padding, used.Width, remainH);
		RectI top2 = new RectI(used.X, used.Y + height + padding, width + padding, remainH);
		RectI right2 = new RectI(used.X + width + padding, used.Y, remainW, used.Height);

		_freeRects.RemoveAt(best);

		if (remainW > 0 && remainH > 0) {
			int waste1 = Math.Abs(right1.Width * right1.Height - top1.Width * top1.Height);
			int waste2 = Math.Abs(right2.Width * right2.Height - top2.Width * top2.Height);

			if (waste1 <= waste2) {
				append(right1);
				append(top1);
			} else {
				append(right2);
				append(top2);
			}
		} else if (remainW > 0) {
			append(new RectI(used.X + width + padding, used.Y, remainW, height + padding));
		} else if (remainH > 0) {
			append(new RectI(used.X, used.Y + height + padding, width + padding, remainH));
		}

		merge();

		result = new RectI(dx, dy, width, height);
		return true;
	}

	private void append(in RectI rect) {
		if (rect.Width > 0 && rect.Height > 0) {
			_freeRects.Add(rect);
		}
	}

	private void merge() {
		bool merged;
		do {
			merged = false;
			Span<RectI> span = CollectionsMarshal.AsSpan(_freeRects);

			for (int i = 0; i < _freeRects.Count; i++) {
				if (span[i].Width == 0 || span[i].Height == 0) {
					continue;
				}

				for (int j = i + 1; j < _freeRects.Count; j++) {
					if (span[j].Width == 0 || span[j].Height == 0) {
						continue;
					}

					if (span[i].X == span[j].X && span[i].Width == span[j].Width) {
						if (span[i].Y + span[i].Height == span[j].Y) {
							span[i].Height += span[j].Height;
							_freeRects.RemoveAt(j);
							merged = true;
							break;
						}
						if (span[j].Y + span[j].Height == span[i].Y) {
							span[i].Y = span[j].Y;
							span[i].Height += span[j].Height;
							_freeRects.RemoveAt(j);
							merged = true;
							break;
						}
					}

					if (span[i].Y == span[j].Y && span[i].Height == span[j].Height) {
						if (span[i].X + span[i].Width == span[j].X) {
							span[i].Width += span[j].Width;
							_freeRects.RemoveAt(j);
							merged = true;
							break;
						}
						if (span[j].X + span[j].Width == span[i].X) {
							span[i].X = span[j].X;
							span[i].Width += span[j].Width;
							_freeRects.RemoveAt(j);
							merged = true;
							break;
						}
					}
				}
				if (merged) {
					break;
				}
			}
		} while (merged);
	}

	private struct RectI {
		public int X;
		public int Y;
		public int Width;
		public int Height;

		public RectI(int x, int y, int width, int height) {
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public static implicit operator Box2(in RectI rect) {
			return Box2.Create(rect.X, rect.Y, rect.Width, rect.Height);
		}
	}
}
