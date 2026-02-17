#region
using Mino.Framework;
using Mino.Graphics.Hardware;
using Mino.Graphics.Sprite;
using Mino.Input;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///		Top user interface.
/// </summary>
public class Face : Component {
	public static readonly List<Face> Presents = new List<Face>();
	
	private TooltipContext? _tooltipCtx;
	private bool _resolveNeeded = false;
	private Vector2 _preWinSize;
	
	public Face() {
		IsInteractive = true;
	}

	/// <summary>
	///		Marks the children needs to resolve.
	/// </summary>
	public void Resolve() {
		_resolveNeeded = true;
	}

	/// <summary>
	///		Displays the interface.
	/// </summary>
	public void Display() {
		if (!Presents.Contains(this)) {
			Presents.Add(this);
		}
		Resolve();
	}

	/// <summary>
	///		Closes the interface.
	/// </summary>
	public void Close() {
		Presents.Remove(this);
		Parent?.RemoveChild(this);
	}

	public override void Update(TimeStep step) {
		if (!IsVisible) {
			return;
		}

		base.Update(step);

		// Observe window resize
		// and mark resolve.
		Vector2 ws = RenderSystem.GetWindow().Size;
		if (_preWinSize != ws) {
			_preWinSize = ws;
			Resolve();
		}
		
		Vector2 cursor = Mcontext.Cursor;
		
		// Update tooltip.
		if (_tooltipCtx != null) {
			_tooltipCtx.Begin();
			foreach (Component comp in Children) {
				if (comp.IsAccessible(cursor)) {
					comp.AppendTooltip(_tooltipCtx);
				}
			}
			_tooltipCtx.End();
		}
		
		updateFocus(this, cursor);
	}

	public override void Draw(Brush brush) {
		if (!IsVisible) {
			return;
		}
		
		base.Draw(brush);
		
		// Handle pending resolve request.
		if (_resolveNeeded) {
			foreach (Component comp in Children) {
				comp.OnResolve?.Invoke(comp, Mcontext);
			}
			_resolveNeeded = false;
		}
		
		_tooltipCtx?.Draw(brush);
	}

	/// <summary>
	///		Sets the interface tooltip context.
	/// </summary>
	/// <param name="ctx">(Nullable) tooltip context.</param>
	public void SetTooltipContext(TooltipContext? ctx) {
		_tooltipCtx = ctx;
	}
	
	private static void updateFocus(Component root, in Vector2 cursor, bool radical = true) {
		if (!Key.Get(Key.MouseLeft).Press) {
			return;
		}

		if (radical) {
			Focused = null;
		}

		foreach (Component comp in root.Children) {
			if (comp.IsAccessible(cursor)) {
				Focused = comp;
				if (comp.Children.Count > 0) {
					updateFocus(comp, cursor, false);
				}
			}
		}
	}
}
