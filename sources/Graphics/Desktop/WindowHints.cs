using Mino.Mathematics;

namespace Mino.Graphics.Desktop;

/// <summary>
///     A series of settings used to create a window.
/// </summary>
public record struct WindowHints {
	public bool AutoIconify = false;
	public Vector2 CursorHotspot = Vector2.Zero;
	public Image? CursorImage = null;
	public bool DebugContext = false;
	public bool Decorated = true;
	public bool Floating = false;
	public bool FocusOnShow = false;
	public Image? Icon = null;
	public bool Maximized = false;
	public bool Resizable = true;
	public Vector2 Size = new Vector2(800, 450);
	public string Title = string.Empty;
	public bool Visible = true;
	public bool Vsync = true;

	public WindowHints() {
	}
}
