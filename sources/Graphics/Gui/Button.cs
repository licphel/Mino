using Mino.Audio;
using Mino.Framework;
using Mino.Graphics.Sprite;
using Mino.Input;

namespace Mino.Graphics.Gui;

/// <summary>
///		Button component, support clicking mode and switching mode..
/// </summary>
public class Button : Component {
	public const float DefaultDelay = 0.05F;

	// Default keymap
	public static readonly Key[] Keymap = [
		Key.Get(Key.MouseLeft),
		Key.Get(Key.MouseRight)
	];
	
	public enum Mode {
		Clicking,
		Switching
	}

	/*
	 * On clicking mode:
	 * [0] - Idle icon
	 * [1] - Hovering icon
	 * [2] - Clicked icon
	 *
	 * On switching mode:
	 * [0] - Off icon
	 * [1] - On icon
	 */
	#region ASSET
	private Drawable?[] _asset_Drawables;
	private Line?[] _asset_Lines;
	#endregion
	
	private Mode _mode;
	private int _state;
	private float _secBuf;
	private float _delay = DefaultDelay;
	
	public Button(Drawable?[] drawable, Line?[] lines, Mode mode = Mode.Clicking) {
		if (drawable.Length != (mode == Mode.Clicking ? 3 : 2)) {
			throw new Error("asset confirmation failed");
		}
		if (lines.Length != 2) {
			throw new Error("asset confirmation failed");
		}
		
		_asset_Drawables = drawable;
		_asset_Lines = lines;
		_mode = mode;
	}

	/// <summary>
	///		Called when left clicked.
	/// </summary>
	public Action? OnAct1;
	/// <summary>
	///		Called when right clicked.
	/// </summary>
	public Action? OnAct2;

	/// <summary>
	///		Whether the button is switched on.
	/// </summary>
	/// <exception cref="Error">Thrown if the button cannot switch.</exception>
	public bool IsOn {
		get {
			if (_mode != Mode.Switching) {
				throw new Error("cannot switch");
			}
			return _state == 1;
		} 
	}

	/// <summary>
	///		Sets how long this button stay pressed after being clicked.
	/// </summary>
	/// <param name="time">Time delay.</param>
	public void SetClickDelay(in TimeSpan time) {
		_delay = (float) time.TotalSeconds;
	}

	public override void Update(TimeStep step) {
		base.Update(step);

		bool act1 = Keymap[0].Press;
		bool act2 = Keymap[1].Press;
		bool hover = Contains(Mcontext.Cursor);

		if (_mode == Mode.Clicking) {
			_secBuf -= (float) step.Delta;
			
			if (_secBuf <= 0) {
				if (act1 || act2) {
					_state = 2;
					_secBuf = _delay;
				}
				
				if (act1 && hover) {
					OnAct1?.Invoke();
					GuiSystem.PlaySound(_asset_Lines[0]);
				} else if(act2 && hover) {
					OnAct2?.Invoke();
					GuiSystem.PlaySound(_asset_Lines[1]);
				} else if (hover) {
					_state = 1;
				} else {
					_state = 0;
				}
			} else {
				// Keep pressed.
				_state = 2;
			}
		} else {
			if ((act1 || act2) && hover) {
				_state = 1 - _state;
			}
		}
	}

	public override void Draw(Brush brush) {
		base.Draw(brush);

		Drawable? drawable = _asset_Drawables[_state];
		if (drawable != null) {
			brush.Draw(drawable, BoundingBox);
		}
	}
}
