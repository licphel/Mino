#region
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Desktop;

/// <summary>
///     A series of settings used to create a window.
/// </summary>
public record struct WindowHints {
	public Vector2 Size = new Vector2(800, 450);
	public Vector2 CursorHotspot = Vector2.Zero;
	public Image? CursorImage = null;
	public Image? Icon = null;
	public bool DebugContext = false;
	public bool AutoIconify = false;
	public bool Decorated = true;
	public bool Floating = false;
	public bool FocusOnShow = false;
	public bool Maximized = false;
	public bool Resizable = true;
	public string Title = string.Empty;
	public bool Visible = true;
	public bool Vsync = true;

	public WindowHints() {
	}
}
