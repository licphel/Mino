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
	///		The element list.
	/// </summary>
	public List<Component> Elements { get; } = new List<Component>();

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

	/// <summary>
	///		Adds an element.
	/// </summary>
	/// <param name="comp">The element to add.</param>
	public void AddElement(Component comp) {
		Elements.Add(comp);
		AddChild(comp);
	}
	
	/// <summary>
	///		Removes an element.
	/// </summary>
	/// <param name="comp">The element to remove.</param>
	public void RemoveElement(Component comp) {
		Elements.Remove(comp);
		RemoveChild(comp);
	}
	
	/// <summary>
	///		Clears all elements.
	/// </summary>
	public void ClearElements() {
		Children.RemoveAll(Elements.Contains);
		Elements.Clear();
	}

	public override void Update(CanvasContext ctx) {
		float dy = -_childBar.GetPos(ctx);
		
		foreach (Component e in Elements) {
			trsC(e, dy); 
			/*
			 * Temporarily translate children pos
			 *
			 * We do not simply use a transform matrix since children may handle input or other
			 * logics here...
			 */
			e.Update(ctx);
			trsC(e, -dy);
		}
		
		base.Update(ctx);
	}

	protected override void UpdateChild(CanvasContext ctx, Component child) {
		if (!Elements.Contains(child)) {
			base.UpdateChild(ctx, child);
		}
	}

	public override void Draw(CanvasContext ctx) {
		Brush brush = ctx.Brush;
		
		Drawable? drawable = _asset_Drawables[0];
		if (drawable != null) {
			brush.Draw(drawable, BoundingBox);
		}
		
		int hc = (int) MathF.Ceiling((float) Elements.Count / _horiCap);
		_childBar.SetSize(hc * _eh + Math.Max(hc - 1, 0) * _wrap + _wrap * 2);
		float dy = -_childBar.GetPos(ctx);
		
		brush.SetScissor(BoundingBox.Inflate(-_wrap, -_wrap));
		
		foreach (Component e in Elements) {
			trsC(e, dy); 
			/*
			 * Temporarily translate children pos
			 *
			 * We do not simply use a transform matrix since children may handle input or other
			 * logics here...
			 */
			e.Draw(ctx);
			trsC(e, -dy);
		}
		
		brush.DisableScissor();
		
		base.Draw(ctx);
	}

	protected override void DrawChild(CanvasContext ctx, Component child) {
		if (!Elements.Contains(child)) {
			base.DrawChild(ctx, child);
		}
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
