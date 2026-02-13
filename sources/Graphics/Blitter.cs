using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Enum;
using Mino.Mathematics;

namespace Mino.Graphics;

/// <summary>
///     Texture blitter, copies a region of a texture to another texture.
/// </summary>
public static unsafe class Blitter {
	/// <summary>
	///     Copies a texture region to another texture region.
	/// </summary>
	/// <param name="from">Source texture.</param>
	/// <param name="to">Target texture.</param>
	/// <param name="dst">Dst region.</param>
	/// <param name="src">Src region.</param>
	/// <param name="filter">Filter of drawing.</param>
	public static void Blit(Texture from, Texture to, in Box2 dst, in Box2 src, TextureFilter filter = TextureFilter.Nearest) {
		Blit(new TexturePart(from, src), new TexturePart(to, dst), filter);
	}
	
	/// <summary>
	///     Copies a texture part to another texture part.
	/// </summary>
	/// <param name="from">Source texture part.</param>
	/// <param name="to">Target texture part.</param>
	/// <param name="filter">Filter of drawing.</param>
	/// <exception cref="Error">Thrown if there's no data to draw or draw to.</exception>
	public static void Blit(in TexturePart from, in TexturePart to, TextureFilter filter = TextureFilter.Nearest) {
		RenderBackend backend = RenderSystem.GetBackend();
		Box2 src = from.Region;
		Box2 dst = to.Region;

		// Call backend blitter.
		backend.TextureBlit(
			from.Src, (int) src.MinX, (int) src.MinY, (int) src.Width, (int) src.Height,
			to.Src, (int) dst.MinX,
			(int) dst.MinY, (int) dst.Width, (int) dst.Height, filter);
	}

	/// <summary>
	///     Copies a RT region to another RT region.
	/// </summary>
	/// <param name="from">Source rt.</param>
	/// <param name="to">Target rt.</param>
	/// <param name="dst">Dst region.</param>
	/// <param name="src">Src region.</param>
	/// <param name="filter">Filter of drawing.</param>
	public static void Blit(RenderTarget from, RenderTarget to, in Box2 dst, in Box2 src,
		TextureFilter filter = TextureFilter.Nearest) {
		RenderBackend backend = RenderSystem.GetBackend();

		// Call backend blitter.
		backend.RenderTargetBlit(
			from, (int) src.MinX, (int) src.MinY, (int) src.Width, (int) src.Height,
			to, (int) dst.MinX,
			(int) dst.MinY, (int) dst.Width, (int) dst.Height, filter);
	}

	/// <summary>
	///     Copies a block of data from an image to another image.
	/// </summary>
	/// <param name="from">Source image.</param>
	/// <param name="to">Target image.</param>
	/// <param name="dst">Dst region.</param>
	/// <param name="src">Src region.</param>
	/// <exception cref="Error">Thrown if src size != dst size.</exception>
	public static void BlockCopy(Image from, Image to, in Box2 dst, in Box2 src) {
		if (src.Size != dst.Size) {
			throw new Error("not supported: image scaling");
		}
		if (from.Bytes == null || to.Bytes == null || from.Bytes.Length < from.PixelStride
		|| to.Bytes.Length < to.PixelStride) {
			throw new Error("no data in image");
		}
		const float CMP = 0.01F;

		if (src.MinX < -CMP || src.MaxX - CMP > from.Width ||
		src.MinY < -CMP || src.MaxY - CMP > from.Height ||
		dst.MinX < -CMP || dst.MaxX - CMP > to.Width ||
		dst.MinY < -CMP || dst.MaxY - CMP > to.Height) {
			throw new Error("coordinates out of bounds");
		}

		byte[] fd = from.Bytes;
		byte[] td = to.Bytes;
		byte[] dstData = td;

		if (from == to) {
			dstData = new byte[td.Length];
			Array.Copy(td, dstData, td.Length);
		}

		const int s = 4; // We only support RGBA8.
		int w = (int) src.Width;
		int h = (int) src.Height;
		int x0 = (int) src.MinX;
		int y0 = (int) src.MinY;
		int x1 = (int) dst.MinX;
		int y1 = (int) dst.MinY;

		fixed (byte* sPtr = fd, dPtr = dstData) {
			// Transform to target regions.
			byte* nsPtr = sPtr + s * (y0 * from.Width + x0);
			byte* ndPtr = dPtr + s * (y1 * to.Width + x1);

			for (int offY = 0; offY < h; offY++) {
				for (int offX = 0; offX < w; offX++) {
					byte* sp = nsPtr + s * (offY * from.Width + offX);
					byte* dp = ndPtr + s * (offY * to.Width + offX);

					// Copy RGBA 4 comps.
					dp[0] = sp[0];
					dp[1] = sp[1];
					dp[2] = sp[2];
					dp[3] = sp[3];
				}
			}
		}

		to.Bytes = dstData;
	}
}
