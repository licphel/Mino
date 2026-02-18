#region
#endregion

using Mino.Mathematics;

namespace Mino.Graphics.Sprite;

/// <summary>
///     Drawable object.
/// </summary>
public abstract class Drawable {
	public abstract void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw, float vh);

	// Implicit Texture -> Drawable.
	public static implicit operator Drawable(Texture texture) {
		return new DrawableTexture(texture);
	}

	// Implicit TexturePart -> Drawable.
	public static implicit operator Drawable(TexturePart texPart) {
		return new DrawableTexturePart(texPart);
	}

	/// <summary>
	///		Texture packed in drawable interface.
	/// </summary>
	public class DrawableTexture : Drawable {
		private Texture _texture;

		internal DrawableTexture(Texture texture) {
			_texture = texture;
		}

		public override void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw, float vh) {
			brush.DrawTexture(_texture, x, y, w, h, u, v, uw, vh);
		}
	}

	/// <summary>
	///		Texture part packed in drawable interface.
	/// </summary>
	public class DrawableTexturePart : Drawable {
		private TexturePart _texPart;

		internal DrawableTexturePart(in TexturePart texPart) {
			_texPart = texPart;
		}

		public override void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw, float vh) {
			brush.DrawTexture(_texPart, x, y, w, h, u, v, uw, vh);
		}
	}

	/// <summary>
	///		Drawable colored rectangle.
	/// </summary>
	public class ColoredRectangle : Drawable {
		private Color _color;
		private bool _framed;
		
		public ColoredRectangle(Color color, bool framed = false) {
			_color = color;
			_framed = framed;
		}
		
		public override void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw, float vh) {
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
}
