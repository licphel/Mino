#region
using Mino.Audio;
using Mino.Framework;
using Mino.Graphics.Sprite;
using Mino.Input;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///     Button component, support clicking mode and switching mode.
/// </summary>
public class Button : Component {
	public const float DefaultDelay = 0.05F;
	
	public static readonly Key[] Keymap = [
		Key.Get(Key.MouseLeft), // Action 1 key
		Key.Get(Key.MouseRight) // Action 2 key
	];

	/// <summary>
	///     Button modes.
	/// </summary>
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
	private Drawable?[] _asset_Drawables;
	private Line?[] _asset_Lines;

	private Mode _mode;
	private int _state;
	private Countdown _pressCD;
	private TimeSpan _delay = TimeSpan.FromSeconds(DefaultDelay);

	public Button(Drawable?[] drawables, Line?[] lines, Mode mode = Mode.Clicking) {
		if (drawables.Length != (mode == Mode.Clicking ? 3 : 2)) {
			throw new Error("asset confirmation failed");
		}
		if (lines.Length != 2) {
			throw new Error("asset confirmation failed");
		}

		_asset_Drawables = drawables;
		_asset_Lines = lines;
		_mode = mode;
		_pressCD = new Countdown();
	}

	/// <summary>
	///     Called when left clicked.
	/// </summary>
	public Action? OnAct1;
	/// <summary>
	///     Called when right clicked.
	/// </summary>
	public Action? OnAct2;

	/// <summary>
	///     Whether the button is switched on.
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
	///     Sets how long this button stay pressed after being clicked.
	/// </summary>
	/// <param name="time">Time delay.</param>
	public void SetClickDelay(in TimeSpan time) {
		_delay = time;
	}

	public override void Update(CanvasContext ctx) {
		_pressCD.Update(ctx.Step);

		bool act1 = Keymap[0].Press;
		bool act2 = Keymap[1].Press;
		bool hover = Contains(ctx.Cursor);

		if (_mode == Mode.Clicking) {
			if (_pressCD.Ready) {
				if ((act1 || act2) && hover) {
					_state = 2;
					_pressCD.Push(_delay);
				}

				if (act1 && hover) {
					OnAct1?.Invoke();
					Canvas.PlaySound(_asset_Lines[0]);
				} else if (act2 && hover) {
					OnAct2?.Invoke();
					Canvas.PlaySound(_asset_Lines[1]);
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
		
		base.Update(ctx);
	}

	public override void Draw(CanvasContext ctx) {
		Brush brush = ctx.Brush;
		Drawable? drawable = _asset_Drawables[_state];
		if (drawable != null) {
			brush.Draw(drawable, BoundingBox);
		}
		
		base.Draw(ctx);
	}
}
