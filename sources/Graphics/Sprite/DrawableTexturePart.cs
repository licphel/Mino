namespace Mino.Graphics.Sprite;

/// <summary>
///     Texture part packed in drawable interface.
/// </summary>
public class DrawableTexturePart : Drawable {
	private TexturePart _texPart;

	public DrawableTexturePart(in TexturePart texPart) {
		_texPart = texPart;
	}

	public override void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw,
		float vh) {
		brush.DrawTexture(_texPart, x, y, w, h, u, v, uw, vh);
	}
}
