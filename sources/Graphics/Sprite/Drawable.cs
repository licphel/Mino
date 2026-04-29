namespace Mino.Graphics.Sprite;

/// <summary>
///     Drawable object.
/// </summary>
public abstract class Drawable {
	public abstract void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw, float vh);
}