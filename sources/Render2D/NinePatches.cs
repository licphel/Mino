#region
using Mino.Graphics;
using Mino.Mathematics;
#endregion

namespace Mino.Render2D;

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

		float rw = w % _tw;
		if (rw == 0) {
			rw = _tw;
		}
		float rh = h % _th;
		if (rh == 0) {
			rh = _th;
		}

		float x2 = x + (Fit ? w - _tw * _scale : GetAlignedWidth(w));
		float y2 = y + (Fit ? h - _th * _scale : GetAlignedWidth(h));
		
		for (int i = 1; i < nw - 1; i++) {
			for (int j = 1; j < nh - 1; j++) {
				float w1 = i == nw - 2 ? rw : _tw;
				float h1 = j == nh - 2 ? rh : _th;
				brush.DrawTexture(_central, x + i * _tw * _scale, y + j * _th * _scale, w1 * _scale, h1 * _scale, _central.Width - w1, 0, w1, h1);
			}
		}
		
		for (int i = 1; i < nh - 1; i++) {
			float h1 = i == nh - 2 ? rh : _th;
			brush.DrawTexture(_left, x, y + i * _th * _scale, _tw * _scale, h1 * _scale, 0, 0, _tw, h1);
		}
		for (int i = 1; i < nw - 1; i++) {
			float w1 = i == nw - 2 ? rw : _tw;
			brush.DrawTexture(_top, x + i * _tw * _scale, y, w1 * _scale, _th * _scale, _top.Width - w1, 0, w1, _th);
		}
		for (int i = 1; i < nh - 1; i++) {
			float h1 = i == nh - 2 ? rh : _th;
			brush.DrawTexture(_right, x2, y + i * _th * _scale, _tw * _scale, h1 * _scale, 0, 0, _tw, h1);
		}
		for (int i = 1; i < nw - 1; i++) {
			float w1 = i == nw - 2 ? rw : _tw;
			brush.DrawTexture(_bottom, x + i * _tw * _scale, y2, w1 * _scale, _th * _scale, _bottom.Region.Width - w1, 0, w1, _th);
		}
		
		brush.DrawTexture(_bottomLeft, x, y2, _tw * _scale, _th * _scale);
		brush.DrawTexture(_topLeft, x, y, _tw * _scale, _th * _scale);
		brush.DrawTexture(_bottomRight, x2, y2, _tw * _scale, _th * _scale);
		brush.DrawTexture(_topRight, x2, y, _tw * _scale, _th * _scale);
	}

	public float GetAlignedWidth(float mw) {
		return _tw * _scale * (cntX(mw) - 1);
	}

	public float GetAlignedHeight(float mh) {
		return _th * _scale * (cntY(mh) - 1);
	}

	private int cntX(float mw) {
		return (int) MathF.Ceiling(mw / _tw / _scale);
	}

	private int cntY(float mh) {
		return (int) MathF.Ceiling(mh / _th / _scale);
	}
}
