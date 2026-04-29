namespace Mino.Graphics.Sprite;

/// <summary>
///     Texture packed in drawable interface.
/// </summary>
public class DrawableTexture : Drawable {
	private Texture _texture;

	public DrawableTexture(Texture texture) {
		_texture = texture;
	}

	public override void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw,
		float vh) {
		brush.DrawTexture(_texture, x, y, w, h, u, v, uw, vh);
	}
}
