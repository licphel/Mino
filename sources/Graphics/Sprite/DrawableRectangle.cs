using Mino.Mathematics;

namespace Mino.Graphics.Sprite;

/// <summary>
///     Drawable colored rectangle.
/// </summary>
public class DrawableRectangle : Drawable {
	private Color _color;
	private bool _framed;

	public DrawableRectangle(Color color, bool framed = false) {
		_color = color;
		_framed = framed;
	}

	public override void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw,
		float vh) {
		Color _oldColor = brush.Color;
		brush.Color = _color;
		if (_framed) {
			brush.DrawRectangleFrame(x, y, w, h);
		} else {
			brush.DrawRectangle(x, y, w, h);
		}
		brush.Color = _oldColor;
	}
}
