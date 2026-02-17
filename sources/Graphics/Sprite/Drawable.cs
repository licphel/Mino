#region
#endregion

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

	internal class DrawableTexture : Drawable {
		private Texture _texture;

		internal DrawableTexture(Texture texture) {
			_texture = texture;
		}

		public override void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw, float vh) {
			brush.DrawTexture(_texture, x, y, w, h, u, v, uw, vh);
		}
	}

	internal class DrawableTexturePart : Drawable {
		private TexturePart _texPart;

		internal DrawableTexturePart(in TexturePart texPart) {
			_texPart = texPart;
		}

		public override void Draw(Brush brush, float x, float y, float w, float h, float u, float v, float uw, float vh) {
			brush.DrawTexture(_texPart, x, y, w, h, u, v, uw, vh);
		}
	}
}
