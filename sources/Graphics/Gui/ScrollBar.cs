#region
using Mino.Desktop;
using Mino.Graphics.Sprite;
using Mino.Input;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///     Scroll bar component.
///     Bounding Box
///     --------------- X
///     |    w r a p	|
///     | w |-------|	|
///     | r |		|	|
///     | a |		|	|
///     | p |		|	|
///     Y
/// </summary>
public class ScrollBar : Component {
	public const float DefaultSpeed = 250.0F;
	public const float DefaultFriction = 5.0F;

	public static readonly Key[] Keymap = [
		Key.Get(Key.MouseLeft) // Drag key
	];

	/*
	 * [0] - bar icon
	 * [1] - background icon
	 */
	public Drawable?[] _asset_Drawables;

	private float _acceleration;
	private float _bx;
	private float _by;
	private float _bw;
	private float _bh;
	private bool _dragging;
	private float _lcy;
	private float _sp0;
	private float _wrap;
	private float _prevPos;
	private float _speed = DefaultSpeed;
	private float _startPos;
	private float _vertical;
	private float _f = DefaultFriction;
	private Action<Vector2>? _hook;
	private bool _parentScrollable;

	public ScrollBar(Drawable?[] drawables) {
		if (drawables.Length != 2) {
			throw new Error("asset confirmation failed");
		}
		_asset_Drawables = drawables;
	}

	/// <summary>
	///     Sets wrapping border of the bar.
	/// </summary>
	/// <param name="wrap">Wrap width.</param>
	public void SetWrap(float wrap) {
		_wrap = wrap;
		_startPos = -wrap;
	}

	/// <summary>
	///     Sets scrolling speed of the bar.
	/// </summary>
	/// <param name="speed">Scrolling speed.</param>
	public void SetSpeed(float speed) {
		_speed = speed;
	}

	/// <summary>
	///     Sets scrolling friction of the bar.
	/// </summary>
	/// <param name="f">Scrolling friction.</param>
	public void SetFriction(float f) {
		_f = f;
	}

	/// <summary>
	///     Sets the vertical size of the elements.
	/// </summary>
	/// <param name="size">Element size.</param>
	public void SetSize(float size) {
		_vertical = size;
	}

	/// <summary>
	///     Gets the lerped position.
	/// </summary>
	/// <param name="ctx">Current canvas context.</param>
	/// <returns>A lerped scroll position.</returns>
	public float GetPos(CanvasContext ctx) {
		return ctx.Partial * (_startPos - _prevPos) + _prevPos;
	}

	/// <summary>
	///     Sets the bar to the top.
	/// </summary>
	public void SetTopped() {
		_startPos = -_wrap;
		clamp();
		_prevPos = _startPos;
	}

	/// <summary>
	///     Sets the bar to the ground.
	/// </summary>
	public void SetGrounded() {
		_startPos = _vertical - BoundingBox.Height + _wrap * 2;
		clamp();
		_prevPos = _startPos;
	}

	/// <summary>
	///     Sets if the scroll bar will also detect scroll in its parent's bounding box.
	/// </summary>
	/// <param name="value">True is enabled.</param>
	public void SetParentScrollable(bool value) {
		_parentScrollable = value;
	}

	protected internal override void InitHooks() {
		base.InitHooks();

		Window window = RenderSystem.GetWindow();
		_hook = scroll => {
			bool canScroll = Hovering;
			if (!canScroll && _parentScrollable && Parent != null) {
				canScroll = Parent.Hovering;
			}
			if (canScroll && scroll != Vector2.Zero) {
				_acceleration -= _speed * scroll.Y;
			}
		};
		window.CursorScrollEvent += _hook;
	}

	protected internal override void FreeHooks() {
		base.FreeHooks();

		Window window = RenderSystem.GetWindow();
		window.CursorScrollEvent -= _hook;
	}

	public override void Update(CanvasContext ctx) {
		Vector2 cursor = ctx.Cursor;

		float dt = (float) ctx.Step.Delta;

		_prevPos = _startPos;
		_startPos += _acceleration * dt;
		if (_acceleration > 0) {
			_acceleration = Math.Clamp(_acceleration - dt * _speed * _f, 0, int.MaxValue);
		} else if (_acceleration < 0) {
			_acceleration = Math.Clamp(_acceleration + dt * _speed * _f, int.MinValue, 0);
		}
		clamp();

		if (Keymap[0].Hold) {
			if (!_dragging) {
				float mx = cursor.X;
				float my = cursor.Y;

				if (mx >= _bx - 1.0F && mx <= _bx + _bw + 1.0F && my >= _by - 1.0F && my <= _by + _bh + 1.0F) {
					_dragging = true;
					_lcy = cursor.Y;
					_sp0 = _startPos;
				}
			}
		} else {
			_dragging = false;
		}

		if (_dragging) {
			_startPos = _sp0 + (cursor.Y - _lcy) / BoundingBox.Height * _vertical;
			clamp();
		}

		base.Update(ctx);
	}

	public override void Draw(CanvasContext ctx) {
		float th = BoundingBox.Height - _wrap * 2.0F;
		float tw = BoundingBox.Width - _wrap * 2.0F;

		// Avoid NaN.
		_vertical = MathF.Max(0.01F, _vertical);

		float per = BoundingBox.Height / _vertical;
		if (per > 1.0F) {
			per = 1.0F;
		}

		float scrollPer = Math.Abs(GetPos(ctx)) / _vertical;
		float h = th * per;
		float oh = scrollPer * th;

		_bw = tw;
		_bh = h;
		_bx = BoundingBox.MinX + _wrap;
		_by = BoundingBox.MinY + _wrap + oh;

		Brush brush = ctx.Brush;
		Drawable? drawable = _asset_Drawables[1];
		if (drawable != null) {
			brush.Draw(drawable, BoundingBox);
		}

		drawable = _asset_Drawables[0];
		if (drawable != null) {
			brush.Draw(drawable, _bx, _by, _bw, _bh);
		}

		base.Draw(ctx);
	}

	private void clamp() {
		if (_startPos <= 0) {
			_startPos = 0;
			_acceleration = 0;
		}

		if (_startPos - _vertical >= -BoundingBox.Height) {
			_startPos = _vertical - BoundingBox.Height;
			_acceleration = 0;
		}

		if (_vertical < BoundingBox.Height) {
			_startPos = 0;
			_acceleration = 0;
		}
	}
}
