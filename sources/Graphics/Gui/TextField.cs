#region
using System.Runtime.InteropServices;
using System.Text;
using Mino.Desktop;
using Mino.Framework;
using Mino.Graphics.Hardware;
using Mino.Graphics.Sprite;
using Mino.Graphics.Text;
using Mino.Input;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///     An editable text field.
/// </summary>
public class TextField : Component {
	public const float DefaultWrap = 4.0F;
	public const float DefaultShine = 0.5F;
	
	public static readonly Key[] Keymap = [
		Key.Get(Key.A, Key.ModControl), // Select all key
		Key.Get(Key.C, Key.ModControl), // Copy key
		Key.Get(Key.V, Key.ModControl), // Paste key
		Key.Get(Key.X, Key.ModControl), // Clip key
		Key.Get(Key.Backspace), // Delete key
		Key.Get(Key.Enter), // Next line key
		Key.Get(Key.Left), // Move left key
		Key.Get(Key.Right), // Move right key
		Key.Get(Key.MouseLeft) // Click move key
	];

	/*
	 * [0] - Idle icon
	 * [1] - Hovering icon
	 * [2] - Clicked icon
	 */
	private Drawable?[] _asset_Drawables;
	/*
	 * [0] - Rendering font
	 */
	private Font _asset_Font;

	private StringBuilder _sb = new StringBuilder();
	private string _hint = string.Empty;
	private int _ptr;
	private ScrollBar _childBar;
	private bool _alls;
	private Action<char>? _hook;
	private TextBlob _blob;
	private TextBlob _blobHint;
	private float _wrap = DefaultWrap;
	private Countdown _cd1;
	private Countdown _cd2;
	private bool _focus;
	private float _lineH = Font.BasicLineHeight;

	public TextField(Drawable?[] drawables, Font font, ScrollBar bar) {
		if (drawables.Length != 3) {
			throw new Error("asset confirmation failed");
		}
		_asset_Drawables = drawables;
		_asset_Font = font;
		_blob = font.Bake(string.Empty);
		_blobHint = font.Bake(_hint);
		_cd1 = new Countdown();
		_cd2 = new Countdown();
		
		_childBar = bar;
		bar.SetParentScrollable(true);
		AddChild(bar);
		
		SetAttribute("HintColor", new Color(1.0F, 1.0F, 1.0F, 0.25F));
		SetAttribute("TextColor", new Color(1.0F, 1.0F, 1.0F));
		SetAttribute("SelectBackColor", new Color(0.2F, 0.65F, 1.0F, 0.5F));
		SetAttribute("SelectTextColor", new Color(1.0F, 1.0F, 1.0F));
	}
	
	/// <summary>
	///     Text in the field.
	/// </summary>
	public string Text {
		get => _sb.ToString();
		set {
			_sb.Clear();
			_sb.Append(value);
			_ptr = _sb.Length;
		}
	}

	/// <summary>
	///		Sets input hint of the field.
	/// </summary>
	/// <param name="hint">The text to display when the field is empty.</param>
	public void SetHint(string hint) {
		_hint = hint;
		rebakeHint();
	}
	
	/// <summary>
	///		Sets wrapping border of the bar.
	/// </summary>
	/// <param name="wrap">Wrap width.</param>
	public void SetWrap(float wrap) {
		_wrap = wrap;
	}

	/// <summary>
	///		Sets text size.
	/// </summary>
	/// <param name="lh">Font line height.</param>
	public void SetLineHeight(float lh) {
		_lineH = lh;
		rebake();
	}

	protected internal override void InitHooks() {
		base.InitHooks();

		Window window = RenderSystem.GetWindow();
		_hook = ch => {
			if (Canvas.Focused.Contains(this)) {
				insert(ch.ToString());
				rebake();
			}
		};
		window.CharInputEvent += _hook;
	}

	protected internal override void FreeHooks() {
		base.FreeHooks();

		Window window = RenderSystem.GetWindow();
		window.CharInputEvent -= _hook;
	}

	public override void Resolve(CanvasContext ctx) {
		base.Resolve(ctx);
		
		// Rebake on resolved - max width changes.
		rebake();
		rebakeHint();
	}

	public override void Update(CanvasContext ctx) {
		_focus = Canvas.Focused.Contains(this);
		_cd1.Update(ctx.Step);
		_cd2.Update(ctx.Step);
		
		if (_focus) {
			Window window = RenderSystem.GetWindow();
			// Clipboard operation.
			// CTRL+A
			if (Keymap[0].React) {
				_alls = !_alls;
			}
			// CTRL+C
			if (Keymap[1].React) {
				window.ClipboardText = Text;
			}
			// CTRL+V
			if (Keymap[2].React) {
				insert(window.ClipboardText);
				rebake();
			}
			// CTRL+X
			if (Keymap[3].React) {
				window.ClipboardText = Text;
				_sb.Clear();
				_ptr = 0;
				_alls = false;
				rebake();
			}
			// BACKSPACE
			if (Keymap[4].React) {
				if (_sb.Length > 0 && _ptr != 0) {
					_ptr = Math.Max(0, _ptr - 1);
					_sb.Remove(_ptr, 1);
				}

				if (_alls) {
					_ptr = 0;
					_sb.Clear();
					_alls = false;
				}
				
				rebake();
			}
			// ENTER
			if (Keymap[5].React) {
				insert("\n");
				rebake();
			}

			// Pointer motion
			if (Keymap[6].React) {
				_ptr = Math.Max(0, _ptr - 1);
			}
			if (Keymap[7].React) {
				_ptr = Math.Min(_sb.Length, _ptr + 1);
			}
			if (Keymap[8].Press) {
				Vector2 ds = BoundingBox.Min + new Vector2(_wrap, _wrap);
				if (_blob.GetGlyphInstance(ctx.Cursor - ds, out GlyphInstance gi)) {
					_ptr = gi.Index + 1;
				}
			}
		}
		
		base.Update(ctx);
	}

	public override void AppendTooltip(TooltipContext ctx) {
		base.AppendTooltip(ctx);
		ctx.Append("hello");
	}

	public override void Draw(CanvasContext ctx) {
		Brush brush = ctx.Brush;
		int dat = 0;
		if (_focus) {
			dat = 2;
		} else if (IsAccessible(ctx.Cursor)) {
			dat = 1;
		}
		Drawable? drawable = _asset_Drawables[dat];
		if (drawable != null) {
			brush.Draw(drawable, BoundingBox);
		}

		if (_sb.Length == 0 && !string.IsNullOrEmpty(_hint)) {
			Color color1 = (Color) GetAttribute("HintColor")!;
			Color color2 = (Color) GetAttribute("TextColor")!;
			drawGlyphs(ctx, _blobHint, color1, color2);
		} else {
			if (_alls) {
				Color color1 = (Color) GetAttribute("SelectTextColor")!;
				Color color2 = (Color) GetAttribute("SelectBackColor")!;
				drawGlyphs(ctx, _blob, color1, color2);
			} else {
				Color color = (Color) GetAttribute("TextColor")!;
				drawGlyphs(ctx, _blob, color, color);
			}
		}
		
		base.Draw(ctx);
	}

	private void drawGlyphs(CanvasContext ctx, TextBlob blob, Color color, Color bgColor) {
		_childBar.SetSize(blob.Height + _wrap * 2);
		float x = BoundingBox.MinX + _wrap;
		float y = BoundingBox.MinY + _wrap - _childBar.GetPos(ctx);
		
		Brush brush = ctx.Brush;
		Color _oldColor = brush.Color;
		brush.SetScissor(BoundingBox);

		// Draw bg rects.
		if (_alls && _sb.Length > 0) {
			brush.Color = bgColor;
			for (int i = 0; i < blob.GlyphRunList.Count; i++) {
				ref GlyphInstance gi = ref CollectionsMarshal.AsSpan(blob.GlyphRunList)[i];
				brush.DrawRectangle(gi.AdjacentBounds.Translate(x, y));
			}
		}
		
		brush.Color = color;
		brush.DrawText(blob, x, y);

		if (_focus) {
			if (!_cd2.Ready) {
				if (_ptr != 0 && _ptr - 1 < blob.Length) {
					GlyphInstance gi = blob.GlyphRunList[_ptr - 1];
					//do not use emptyDisplay
					x += gi.Bounds.MinX + gi.Glyph.Advance - 1.0F;
					y += gi.Line * _blob.Info.LineGap;
				}
				brush.Color = bgColor;
				brush.DrawRectangle(x, y - _blob.Info.Descender, 1.0F, _lineH);
				
				_cd1.Push(TimeSpan.FromSeconds(DefaultShine));
			}
			if (_cd1.Ready) {
				_cd2.Push(TimeSpan.FromSeconds(DefaultShine));
			}
		}

		brush.Color = _oldColor;
		brush.DisableScissor();
	}
	
	private void insert(string txt) {
		if (_alls) {
			_alls = false;
			_sb.Clear();
			_ptr = 0;
		}
		
		_sb.Insert(_ptr, txt);
		_ptr += txt.Length;
	}

	private void rebake() {
		// Update blob.
		_blob = _asset_Font.Bake(Text, BoundingBox.Width - _wrap * 2, _lineH);
	}
	
	private void rebakeHint() {
		// Update blob.
		_blobHint = _asset_Font.Bake(_hint, BoundingBox.Width - _wrap * 2, _lineH);
	}
}
