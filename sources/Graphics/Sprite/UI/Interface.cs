#region
using Mino.Framework;
using Mino.Graphics.Hardware;
using Mino.Input;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Sprite.UI;

/// <summary>
///		Top user interface.
/// </summary>
public class Interface : Component {
	public static readonly List<Interface> Presents = new List<Interface>();
	
	private TooltipContext? _tooltipCtx;
	private bool _resolveNeeded = false;
	private Vector2 _preWinSize;
	
	public Interface() {
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

		_tooltipCtx?.Begin();
		base.Update(step);
		_tooltipCtx?.End();

		// Observe window resize
		// and mark resolve.
		Vector2 ws = RenderSystem.GetWindow().Size;
		if (_preWinSize != ws) {
			_preWinSize = ws;
			Resolve();
		}
	}

	public override void Draw(Brush brush) {
		if (!IsVisible) {
			return;
		}
		
		MappingContext mc = brush.CreateContext();

		// Handle pending resolve request.
		if (_resolveNeeded) {
			foreach (Component comp in Children) {
				comp.OnRemap?.Invoke(comp, mc);
			}
			_resolveNeeded = false;
		}

		base.Draw(brush);
		_tooltipCtx?.Draw(brush);
		
		updateFocus(this, mc.Cursor);
	}

	/// <summary>
	///		Sets the interface tooltip context.
	/// </summary>
	/// <param name="ctx">(Nullable) tooltip context.</param>
	public void SetTooltipContext1(TooltipContext? ctx) {
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
