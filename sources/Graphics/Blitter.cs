using Mino.Graphics.RHI.Enum;
using Mino.Mathematics;

namespace Mino.Graphics;

/// <summary>
///     Texture blitter, copies a region of a texture to another texture.
/// </summary>
public static unsafe class Blitter {
	/// <summary>
	///     Copies a texture to another texture.
	/// </summary>
	/// <param name="from">Source texture part.</param>
	/// <param name="to">Target texture part.</param>
	/// <param name="filter">Filter of drawing.</param>
	/// <exception cref="Error">Thrown if there's no data to draw or draw to.</exception>
	public static void Blit(in TexturePart from, in TexturePart to, TextureFilter filter = TextureFilter.Nearest) {
		from.Src.blit(to.Src, to.Region, from.Region, filter);
	}

	/// <summary>
	///     Copies a block of data from an image to another image.
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="dst"></param>
	/// <param name="src"></param>
	/// <exception cref="Error">Thrown if src size != dst size.</exception>
	public static void BlockCopy(Image from, Image to, in Box2 dst, in Box2 src) {
		if (src.Size != dst.Size) {
			throw new Error("not supported: image scaling");
		}
		if (from.Data == null || to.Data == null) {
			throw new Error("no data in image");
		}
		const float CMP = 0.1F;

		if (src.MinX < -CMP || src.MaxX - CMP > from.Width ||
		src.MinY < -CMP || src.MaxY - CMP > from.Height ||
		dst.MinX < -CMP || dst.MaxX - CMP > to.Width ||
		dst.MinY < -CMP || dst.MaxY - CMP > to.Height) {
			throw new Error("coordinates out of bounds");
		}

		byte[] fd = from.Data;
		byte[] td = to.Data;
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

		to.Data = dstData;
	}
}
