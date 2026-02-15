#region
using Mino.Input;
using Mino.Mathematics;
using Mino.Nio;
#endregion

namespace Mino.Desktop;

/// <summary>
///     Represents a native window.
/// </summary>
public abstract class Window : IDisposable {
	/// <summary>
	///     Called when one char is inputted.
	/// </summary>
	public Action<char>? CharInputEvent;
	/// <summary>
	///     Called when cursor drops some files into the window.
	/// </summary>
	public Action<Url[]>? CursorDropEvent;
	/// <summary>
	///     Called when cursor leaves or enters the window.
	/// </summary>
	public Action<bool>? CursorEnterLeftEvent;
	/// <summary>
	///     Call on cursor motion.
	/// </summary>
	public Action<Vector2>? CursorMoveEvent;
	/// <summary>
	///     Called on cursor scrolling.
	/// </summary>
	public Action<Vector2>? CursorScrollEvent;

	/// <summary>
	///     Called when a key is pressed or released.
	/// </summary>
	public Action<uint, uint, KeyStatus>? KeyEvent;
	/// <summary>
	///     Called when window gets or loses focus.
	/// </summary>
	public Action<bool>? WindowFocusEvent;
	/// <summary>
	///     Called when window is iconified or de-iconified.
	/// </summary>
	public Action<bool>? WindowIconifyEvent;
	/// <summary>
	///     Called when window is maximized or de-maximized.
	/// </summary>
	public Action<bool>? WindowMaximizeEvent;
	/// <summary>
	///     Called when window position changes.
	/// </summary>
	public Action<Vector2>? WindowMoveEvent;
	/// <summary>
	///     Called on window resizing.
	/// </summary>
	public Action<Vector2>? WindowResizeEvent;

	/// <summary>
	///     Whether the window has a debug context.
	/// </summary>
	public abstract bool Debug { get; }

	/// <summary>
	///     Size of the window.
	/// </summary>
	public abstract Vector2 Size { get; set; }

	/// <summary>
	///     Position of the window.
	/// </summary>
	public abstract Vector2 Position { get; set; }

	/// <summary>
	///     Whether the window keeps float at top.
	/// </summary>
	public abstract bool Floating { get; set; }

	/// <summary>
	///     Whether the window has a frame.
	/// </summary>
	public abstract bool Decorated { get; set; }

	/// <summary>
	///     Visibility of the window.
	/// </summary>
	public abstract bool Visible { get; set; }

	/// <summary>
	///     Whether the window iconifies when losing focus.
	/// </summary>
	public abstract bool AutoIconify { get; set; }

	/// <summary>
	///     Whether the window is maximized.
	/// </summary>
	public abstract bool Maximized { get; set; }

	/// <summary>
	///     Resizability of the window.
	/// </summary>
	public abstract bool Resizable { get; set; }

	/// <summary>
	///     Whether the window uses v-sync.
	/// </summary>
	public abstract bool Vsync { get; set; }

	/// <summary>
	///     Title of the window.
	/// </summary>
	public abstract string Title { get; set; }

	/// <summary>
	///     Cursor position (Y-Down).
	/// </summary>
	public abstract Vector2 Cursor { get; set; }

	/// <summary>
	///     Whether to use relative cursor.
	/// </summary>
	public abstract bool CursorRelativeMode { get; set; }

	/// <summary>
	///     Cursor scroll delta, +Y is upward and +X is rightward.
	/// </summary>
	public abstract Vector2 CursorScroll { get; set; }

	public abstract bool Closed { get; }

	public abstract void Dispose();
	/// <summary>
	///     Initializes the window with the given hints.
	/// </summary>
	/// <param name="hints">the preferred settings</param>
	public abstract void Init(WindowHints hints);

	/// <summary>
	///     Gets an opaque context of this window for graphics apis.
	/// </summary>
	/// <returns>A native opaque context.</returns>
	public abstract WindowOpaqueContext GetOpaqueContext();

	/// <summary>
	///     Process window events.
	/// </summary>
	public abstract void ProcessWindowEvents();

	/// <summary>
	///     Present the buffered contents.
	/// </summary>
	public abstract void Present();

	/// <summary>
	///     Gets a key's status.
	/// </summary>
	/// <param name="code">Code of the key.</param>
	/// <returns>Status of the key.</returns>
	public abstract KeyStatus GetStatus(uint code);

	/// <summary>
	///     Gets a key's modifiers.
	/// </summary>
	/// <param name="code">Code of the key.</param>
	/// <returns>Combined modifiers of the key.</returns>
	public abstract uint GetModifiers(uint code);
}
