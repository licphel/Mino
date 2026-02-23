using Mino.Graphics.Sprite;
using Mino.Mathematics;

namespace Mino.Graphics.Gui;

/// <summary>
///		A bracket contains some components that users can interact with.
/// </summary>
public class Bracket : Component {
	public const float DefaultWrap = 4.0F;
	public const float DefaultPadding = 2.0F;

	private ScrollBar _childBar;
	public Drawable?[] _asset_Drawables;
	private int _horiCap = 1;
	private float _eh;
	private float _ew;
	private float _wrap = DefaultWrap;
	private float _padding = DefaultPadding;

	public Bracket(Drawable?[] drawables, ScrollBar bar) {
		if (drawables.Length != 1) {
			throw new Error("asset confirmation failed");
		}
		_asset_Drawables = drawables;
		
		_childBar = bar;
		bar.SetParentScrollable(true);
		AddChild(bar);
	}

	/// <summary>
	///		Count of the contained elements.
	/// </summary>
	public int Count {
		get => Children.Count - 1;
	}

	/// <summary>
	///		Sets the maximum elements to be lined horizontally.
	/// </summary>
	/// <param name="value">Capacity to set.</param>
	public void SetHorizontalCapacity(int value) {
		_horiCap = value;
	}
	
	/// <summary>
	///     Sets wrapping border of the bracket.
	/// </summary>
	/// <param name="wrap">Wrap width.</param>
	public void SetWrap(float wrap) {
		_wrap = wrap;
	}
	
	/// <summary>
	///     Sets padding between elements of the bracket.
	/// </summary>
	/// <param name="padding">Padding size.</param>
	public void SetPadding(float padding) {
		_padding = padding;
	}

	/// <summary>
	///		Fits the horizontal capacity and wrap automatically.
	/// </summary>
	/// <param name="thisWidth">Bracket width.</param>
	/// <param name="elementSize">Element per size.</param>
	/// <param name="horizontalCap">Horizontal element capacity.</param>
	public void SetLayout(float thisWidth, in Vector2 elementSize, int horizontalCap) {
		// thisWidth = (elementWidth + wrap) * horizontalCap + wrap * 2.
		float wp = (thisWidth - elementSize.X * horizontalCap) / (horizontalCap + 2);
		_ew = elementSize.X;
		_eh = elementSize.Y;
		SetWrap(wp);
		SetHorizontalCapacity(horizontalCap);
	}

	protected override void UpdateChild(CanvasContext ctx, Component child) {
		if (child == _childBar) {
			base.UpdateChild(ctx, child);
			return;
		}
		
		float dy = -_childBar.GetPos(ctx);
		trsC(child, dy); 
		/*
		 * Temporarily translate children pos
		 *
		 * We do not simply use a transform matrix since children may handle input or other
		 * logics here...
		 */
		child.Update(ctx);
		trsC(child, -dy);
	}

	public override void Draw(CanvasContext ctx) {
		int hc = (int) MathF.Ceiling((float) Count / _horiCap);
		_childBar.SetSize(hc * _eh + Math.Max(hc - 1, 0) * _wrap + _wrap * 2);
		
		Brush brush = ctx.Brush;
		brush.SetScissor(Box2.GetUnion(BoundingBox, _childBar.BoundingBox));
		
		base.Draw(ctx);
		
		brush.DisableScissor();
	}

	protected override void DrawChild(CanvasContext ctx, Component child) {
		if (child == _childBar) {
			base.DrawChild(ctx, child);
			return;
		}
		
		float dy = -_childBar.GetPos(ctx);
		trsC(child, dy); 
		/*
		 * Temporarily translate children pos
		 *
		 * We do not simply use a transform matrix since children may handle input or other
		 * logics here...
		 */
		child.Draw(ctx);
		trsC(child, -dy);
	}

	private static void trsC(Component child, float dy) {
		child.BoundingBox = child.BoundingBox.Translate(0.0F, dy);
	}

	protected override void HandleChildBox(Component child) {
		int idx = Children.IndexOf(child);
		
		if (idx == 0) {
			// Child scroll bar.
			base.HandleChildBox(child);
			
		} else {
			float by = BoundingBox.MinY + _wrap;
			float bx = BoundingBox.MinX + _wrap;
			int added = 0;
			
			while (--idx > 0) {
				bx += _ew + _wrap;
				if (++added >= _horiCap) {
					bx = BoundingBox.MinX + _wrap;
					by += _eh + _wrap;
					added = 0;
				}
			}

			child.BoundingBox = child.BoundingBox.Translate(new Vector2(bx, by));
		}
	}
}
