using System.Text;
using Mino.Graphics.Sprite;
using Mino.Graphics.Text;
using Mino.Mathematics;

namespace Mino.Graphics.Gui;

/// <summary>
///		A tooltip manager.
/// </summary>
public class TooltipContext {
	private Drawable? _background;
	private Font _font;
	private List<string> _tooltips = new List<string>();
	private string _collection = string.Empty;
	
	public TooltipContext(Drawable? background, Font font) {
		_background = background;
		_font = font;
	}

	public void Begin() {
		_tooltips.Clear();
	}

	public void End() {
		_tooltips.Reverse();
		
		// Collects to a single string.
		StringBuilder sb = new StringBuilder();
		foreach (string str in _tooltips) {
			sb.Append(str);
			sb.Append('\n');
		}
		// Remove last terminator.
		sb.Remove(sb.Length - 1, 1);
		_collection = sb.ToString();
	}

	public void Append(string text) {
		_tooltips.Add(text);
	}

	public void Draw(CanvasContext ctx) {
		if (_tooltips.Count <= 0) {
			return;
		}

		const float Edge = 8.0F;
		const float Lh = 12.0F;
		const float Offset = 20.0F;
		
		Vector2 size = ctx.Size;
		Vector2 cursor = ctx.Cursor;
		TextBlob bakedBlob = _font.Bake(_collection, size.X * 0.5F, Lh);
		
		float w = bakedBlob.Width;
		float h = bakedBlob.Height;

		// Limit to sight.
		float x = cursor.X + Offset;
		float y = cursor.Y + Offset;
		
		if (x + Edge + w >= size.X) {
			x = Math.Max(size.X - Edge - w, 0.0F);
		}
		if (y + Edge + h >= size.Y) {
			y = Math.Max(size.Y - Edge - h, 0.0F);
		}

		Brush brush = ctx.Brush;
		if (_background != null) {
			brush.Draw(_background, x - Edge, y - Edge, w + Edge * 2.0F, h + Edge * 2.0F);
		}
		brush.DrawText(bakedBlob, x, y);
	}
}
