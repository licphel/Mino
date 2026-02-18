#region
using Mino.Graphics.Hardware;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///		Top user interface.
/// </summary>
public class Face : Component {
	private TooltipContext? _tooltipCtx;
	private bool _resolveNeeded = false;
	private Vector2 _preWinSize;
	
	/// <summary>
	///		Marks the children needs to resolve.
	/// </summary>
	public void RequestResolve() {
		_resolveNeeded = true;
	}

	public override void Update(CanvasContext ctx) {
		base.Update(ctx);

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
				if (comp.IsAccessible(ctx.Cursor)) {
					comp.AppendTooltip(_tooltipCtx);
				}
			}
			_tooltipCtx.End();
		}
	}

	public override void Draw(CanvasContext ctx) {
		base.Draw(ctx);
		
		// Handle pending resolve request.
		if (_resolveNeeded) {
			foreach (Component comp in Children) {
				comp.Resolve(ctx);
			}
			_resolveNeeded = false;
		}
		
		_tooltipCtx?.Draw(ctx);
	}

	/// <summary>
	///		Sets the interface tooltip context.
	/// </summary>
	/// <param name="ctx">(Nullable) tooltip context.</param>
	public void SetTooltipContext(TooltipContext? ctx) {
		_tooltipCtx = ctx;
	}
}
