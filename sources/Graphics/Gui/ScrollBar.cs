#region
using Mino.Desktop;
using Mino.Graphics.Hardware;
using Mino.Graphics.Sprite;
using Mino.Input;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///		Scroll bar component.
///
///		Bounding Box
///		--------------- X
///		|    w r a p	|
///		| w |-------|	|
///		| r |		|	|
///		| a |		|	|
///		| p |		|	|
///		Y
/// </summary>
public class ScrollBar : Component {
	public const float DefaultSpeed = 1000.0F;
	public const float DefaultFriction = 5.0F;

	public static readonly Key[] Keymap = [
		Key.Get(Key.MouseLeft) // Drag key
	];
	
	/*
	 * Assets
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
	private float _totalSize;
	private float _f = DefaultFriction;
	
	public ScrollBar(Drawable?[] drawables) {
		if (drawables.Length != 2) {
			throw new Error("asset confirmation failed");
		}
		_asset_Drawables = drawables;
	}

	/// <summary>
	///		Sets wrapping border of the bar.
	/// </summary>
	/// <param name="wrap">Wrap width.</param>
	public void SetWrap(float wrap) {
		_wrap = wrap;
		_startPos = -wrap;
	}

	/// <summary>
	///		Sets scrolling speed of the bar.
	/// </summary>
	/// <param name="speed">Scrolling speed.</param>
	public void SetSpeed(float speed) {
		_speed = speed;
	}
	
	/// <summary>
	///		Sets scrolling friction of the bar.
	/// </summary>
	/// <param name="f">Scrolling friction.</param>
	public void SetFriction(float f) {
		_f = f;
	}

	/// <summary>
	///		Sets the vertical size of the elements.
	/// </summary>
	/// <param name="size">Element size.</param>
	public void SetSize(float size) {
		_totalSize = size;
	}

	/// <summary>
	///		Gets the lerped position.
	/// </summary>
	/// <param name="ctx">Current canvas context.</param>
	/// <returns>A lerped scroll position.</returns>
	public float GetPos(CanvasContext ctx) {
		return ctx.Partial * (_startPos - _prevPos) + _prevPos;
	}

	/// <summary>
	///		Sets the bar to the top.
	/// </summary>
	public void SetTopped() {
		_startPos = -_wrap;
		clamp();
		_prevPos = _startPos;
	}

	/// <summary>
	///		Sets the bar to the ground.
	/// </summary>
	public void SetGrounded() {
		_startPos = _totalSize - BoundingBox.Height + _wrap * 2;
		clamp();
		_prevPos = _startPos;
	}

	private void clamp() {
		// A minimum offset.
		if (_startPos <= -_wrap) {
			_startPos = -_wrap;
			_acceleration = 0;
		}

		if (_startPos - _totalSize - _wrap >= -BoundingBox.Height) {
			_startPos = _totalSize - BoundingBox.Height + _wrap;
			_acceleration = 0;
		}

		if (_totalSize + _wrap * 2 < BoundingBox.Height) {
			_startPos = -_wrap;
			_acceleration = 0;
		}
	}

	public override void Update(CanvasContext ctx) {
		Window window = RenderSystem.GetWindow();
		Vector2 scroll = window.CursorScroll;
		Vector2 cursor = ctx.Cursor;
		
		float dt = (float) ctx.Step.Delta;
		
		_prevPos = _startPos;

		if (Contains(cursor) && scroll != Vector2.Zero) {
			_acceleration -= _speed * scroll.Y;
			window.CursorScroll = Vector2.Zero;
		}

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
			_startPos = _sp0 + (cursor.Y - _lcy) / BoundingBox.Height * _totalSize;
			clamp();
		}
	}

	public override void Draw(CanvasContext ctx) {
		float th = BoundingBox.Height - _wrap * 2.0F;
		float tw = BoundingBox.Width - _wrap * 2.0F;

		float per = th / _totalSize;
		if (per > 1.0F) {
			per = 1.0F;
		}

		float scrollPer = Math.Abs(GetPos(ctx)) / _totalSize;
		float h = th * per;
		float oh = scrollPer * th;

		_bw = tw;
		_bh = h;
		_bx = BoundingBox.MinX + _wrap;
		_by = BoundingBox.MinY + _wrap + oh;
		
		Brush brush = ctx.Brush;
		if (per < 1.0F) {
			Drawable? drawable = _asset_Drawables[1];
			if (drawable != null) {
				brush.Draw(drawable, BoundingBox);
			}
			
			drawable = _asset_Drawables[0];
			if (drawable != null) {
				brush.Draw(drawable, _bx, _by, _bw, _bh, 0.0F, 0.0F, _bw, _bh);
			}
		}
	}
}
