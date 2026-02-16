#region
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Planar;

/// <summary>
///     An implementation of the classic 9-patches.
/// </summary>
public class NinePatches : Drawable {
	private readonly TexturePart _top;
	private readonly TexturePart _central;
	private readonly TexturePart _left;
	private readonly TexturePart _topLeft;
	private readonly TexturePart _bottomLeft;
	private readonly TexturePart _right;
	private readonly TexturePart _topRight;
	private readonly TexturePart _bottomRight;
	private readonly TexturePart _bottom;
	private readonly float _scale;
	private readonly float _th;
	private readonly float _tw;

	public NinePatches(TexturePart tex, float scale = 1.0F) {
		_scale = scale;

		const float p13 = 1.0F / 3;
		const float p23 = 2.0F / 3;

		_tw = p13 * tex.Width;
		_th = p13 * tex.Height;

		_bottomLeft = Slice(tex, 0, p23, p13, p13);
		_bottom = Slice(tex, p13, p23, p13, p13);
		_bottomRight = Slice(tex, p23, p23, p13, p13);
		_left = Slice(tex, 0, p13, p13, p13);
		_central = Slice(tex, p13, p13, p13, p13);
		_right = Slice(tex, p23, p13, p13, p13);
		_topLeft = Slice(tex, 0, 0, p13, p13);
		_top = Slice(tex, p13, 0, p13, p13);
		_topRight = Slice(tex, p23, 0, p13, p13);

		return;

		// Slice by percentage.
		static TexturePart Slice(in TexturePart texPart, float u, float v, float w, float h) {
			float w0 = texPart.Width;
			float h0 = texPart.Height;
			return new TexturePart(texPart, Box2.Create(w0 * u, h0 * v, w0 * w, h0 * h));
		}
	}

	/// <summary>
	///     Whether different parts can overlap on each other to fit the drawing size accurately.
	///     (Keep it false when transparent)
	/// </summary>
	public bool Fit { get; set; }

	public override void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw, float vh) {
		int nw = cntX(w);
		int nh = cntY(h);

		float atw = _tw * _scale;
		float ath = _th * _scale;

		if (Fit) {
			float lcw = w - atw * (nw - 2);
			float lch = h - ath * (nh - 2);

			float rx = x + w - atw;
			float by = y + h - ath;

			// Central.
			for (int j = 1; j < nh - 1; j++) {
				for (int i = 1; i < nw - 1; i++) {
					float drawX = x + i * atw;
					float drawY = y + j * ath;

					// Fit to box.
					float drawW = i == nw - 2 ? lcw : atw;
					float drawH = j == nh - 2 ? lch : ath;

					brush.DrawTexture(_central, drawX, drawY, drawW, drawH);
				}
			}

			// Edges.
			for (int j = 1; j < nh - 1; j++) {
				float drawY = y + j * ath;
				float drawH = j == nh - 2 ? lch : ath;
				brush.DrawTexture(_left, x, drawY, atw, drawH);
				brush.DrawTexture(_right, rx, drawY, atw, drawH);
			}

			for (int i = 1; i < nw - 1; i++) {
				float drawX = x + i * atw;
				float drawW = i == nw - 2 ? lcw : atw;
				brush.DrawTexture(_top, drawX, y, drawW, ath);
				brush.DrawTexture(_bottom, drawX, by, drawW, ath);
			}

			// Corners.
			brush.DrawTexture(_topLeft, x, y, atw, ath);
			brush.DrawTexture(_topRight, rx, y, atw, ath);
			brush.DrawTexture(_bottomLeft, x, by, atw, ath);
			brush.DrawTexture(_bottomRight, rx, by, atw, ath);
		} else {
			float rx = x + atw * (nw - 1);
			float by = y + ath * (nh - 1);

			// Do not limit
			/*
			rx = Math.Min(rx, x + w - atw);
			by = Math.Min(by, y + h - ath);
			*/
			
			// Central.
			for (int j = 1; j < nh - 1; j++) {
				for (int i = 1; i < nw - 1; i++) {
					float drawX = x + i * atw;
					float drawY = y + j * ath;
					brush.DrawTexture(_central, drawX, drawY, atw, ath);
				}
			}

			// Edges.
			for (int j = 1; j < nh - 1; j++) {
				float drawY = y + j * ath;
				brush.DrawTexture(_left, x, drawY, atw, ath);
				brush.DrawTexture(_right, rx, drawY, atw, ath);
			}

			for (int i = 1; i < nw - 1; i++) {
				float drawX = x + i * atw;
				brush.DrawTexture(_top, drawX, y, atw, ath);
				brush.DrawTexture(_bottom, drawX, by, atw, ath);
			}

			// Corners.
			brush.DrawTexture(_topLeft, x, y, atw, ath);
			brush.DrawTexture(_topRight, rx, y, atw, ath);
			brush.DrawTexture(_bottomLeft, x, by, atw, ath);
			brush.DrawTexture(_bottomRight, rx, by, atw, ath);
		}
	}

	private int cntX(float mw) {
		return Math.Max(2, (int) MathF.Ceiling(mw / _tw / _scale));
	}

	private int cntY(float mh) {
		return Math.Max(2, (int) MathF.Ceiling(mh / _th / _scale));
	}

	private float GetTileWidth() {
		return _tw * _scale;
	}

	private float GetTileHeight() {
		return _th * _scale;
	}
}
