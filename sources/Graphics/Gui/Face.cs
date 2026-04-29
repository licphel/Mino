#region
using Mino.Framework;
using Mino.Graphics.Sprite;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///     Top user interface.
/// </summary>
public class Face : Component {
	private TooltipContext? _tooltipCtx;
	private bool _resolveNeeded = false;
	private Vector2 _preWinSize;

	/// <summary>
	///     Marks the children needs to resolve.
	/// </summary>
	public void RequestResolve() {
		_resolveNeeded = true;
	}

	public override void Update(in TimeStep step) {
		// Observe window resize
		// and mark resolve.
		Vector2 ws = RenderSystem.GetWindow().Size;
		if (_preWinSize != ws) {
			_preWinSize = ws;
			RequestResolve();
		}

		// Update tooltip.
		if (_tooltipCtx != null) {
			_tooltipCtx.Begin();
			foreach (Component comp in Children) {
				if (comp.IsAccessible()) {
					comp.AppendTooltip(_tooltipCtx);
				}
			}
			_tooltipCtx.End();
		}

		base.Update(step);
	}

	public override void Draw(Brush brush, float partial) {
		// Handle pending resolve request.
		if (_resolveNeeded) {
			Resolve();
			foreach (Component comp in Children) {
				comp.Resolve();
			}
			_resolveNeeded = false;
		}
		
		base.Draw(brush, partial);

		_tooltipCtx?.Draw(brush, partial);
	}

	/// <summary>
	///     Sets the interface tooltip context.
	/// </summary>
	/// <param name="ctx">(Nullable) tooltip context.</param>
	public void SetTooltipContext(TooltipContext? ctx) {
		_tooltipCtx = ctx;
	}

	public override bool IsAccessible() {
		if (Canvas.Presents.Count == 0) {
			return false;
		}
		// We just check if it is the top face.
		return this == Canvas.Presents[^1];
	}
}
