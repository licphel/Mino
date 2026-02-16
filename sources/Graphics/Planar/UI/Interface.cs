using Mino.Framework;

namespace Mino.Graphics.Planar.UI;

/// <summary>
///		User interface.
/// </summary>
public class Interface {
	public static readonly List<Interface> Presents = new List<Interface>();
	
	private bool _init;
	private Interface? _parent = null;
	private TooltipContext? _tooltipContext = null;
	
	public virtual void Reflush() {
		if (!_init) {
			OnInit?.Invoke();
			_init = true;
		}
		OnRelocate?.Invoke();
	}

	public void Display(bool clear = false) {
		if (clear) {
			Presents.Clear();
		}
		if (Presents.Count > 0) {
			_parent = Presents[^1];
		}
		
		Presents.Add(this);
		Reflush();
	}

	public void Close() {
		OnClosed?.Invoke();
		Presents.Remove(this);
	}

	public Action? OnInit;
	public Action? OnRelocate;
	public Action? OnClosed;
	public Action<TimeStep>? OnUpdate;
	public Action<Brush>? OnDraw;

	public virtual void Update(TimeStep step) {
		OnUpdate?.Invoke(step);

		if (_tooltipContext != null) {
			_tooltipContext.Begin();
			
			//... TODO: collect tooltips.
			
			_tooltipContext.End();
		}
	}

	public virtual void Draw(Brush brush) {
		OnDraw?.Invoke(brush);

		_tooltipContext?.Draw(brush);
	}

	public void SetTooltipContext(TooltipContext? ctx) {
		_tooltipContext = ctx;
	}
}
