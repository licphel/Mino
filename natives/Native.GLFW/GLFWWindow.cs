#region
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Mino.Framework.XPlatform;
using Mino.Graphics.Desktop;
using Mino.Graphics.Input;
using Mino.Mathematics;
using Mino.Nio;
using Silk.NET.GLFW;
using GLFW_Image = Silk.NET.GLFW.Image;
#endregion

namespace Mino.Native.GLFW;

public unsafe class GLFWWindow : Window, ServiceProvider {
	private Glfw _glfw = Glfw.GetApi();
	private WindowHandle* _handle;
	internal ConcurrentDictionary<int, int> _keyModMap = new ConcurrentDictionary<int, int>();
	internal ConcurrentDictionary<int, byte> _keyStatusMap = new ConcurrentDictionary<int, byte>();
	private Vector2 _cursor;
	private string _title = string.Empty;
	private bool _vsync;
	private bool _closed;
	private bool _cursorRelativeMode = false;
	private bool _debug;

	public override bool Debug { get => _debug; }

	public override Vector2 Size {
		get {
			_glfw.GetWindowSize(_handle, out int w, out int h);
			return new Vector2(w, h);
		}
		set => _glfw.SetWindowSize(_handle, (int) value.X, (int) value.Y);
	}

	public override Vector2 Position {
		get {
			_glfw.GetWindowPos(_handle, out int w, out int h);
			return new Vector2(w, h);
		}
		set => _glfw.SetWindowPos(_handle, (int) value.X, (int) value.Y);
	}

	public override bool Floating {
		get => _glfw.GetWindowAttrib(_handle, WindowAttributeGetter.Floating);
		set => _glfw.SetWindowAttrib(_handle, WindowAttributeSetter.Floating, value);
	}

	public override bool Decorated {
		get => _glfw.GetWindowAttrib(_handle, WindowAttributeGetter.Decorated);
		set => _glfw.SetWindowAttrib(_handle, WindowAttributeSetter.Decorated, value);
	}

	public override bool Visible {
		get => _glfw.GetWindowAttrib(_handle, WindowAttributeGetter.Visible);
		set {
			if (value) {
				_glfw.ShowWindow(_handle);
			} else {
				_glfw.HideWindow(_handle);
			}
		}
	}

	public override bool AutoIconify {
		get => _glfw.GetWindowAttrib(_handle, WindowAttributeGetter.AutoIconify);
		set => _glfw.SetWindowAttrib(_handle, WindowAttributeSetter.AutoIconify, value);
	}

	public override bool Maximized {
		get => _glfw.GetWindowAttrib(_handle, WindowAttributeGetter.Maximized);
		set {
			if (value) {
				_glfw.MaximizeWindow(_handle);
			} else {
				_glfw.RestoreWindow(_handle);
			}
		}
	}

	public override bool Resizable {
		get => _glfw.GetWindowAttrib(_handle, WindowAttributeGetter.Resizable);
		set => _glfw.SetWindowAttrib(_handle, WindowAttributeSetter.Resizable, value);
	}

	public override bool Vsync {
		get => _vsync;
		set {
			if (value) {
				_glfw.SwapInterval(1);
			} else {
				_glfw.SwapInterval(0);
			}
			_vsync = value;
		}
	}

	public override string Title {
		get => _title;
		set => _glfw.SetWindowTitle(_handle, _title = value);
	}

	public override Vector2 Cursor {
		get => _cursor;
		set => _glfw.SetCursorPos(_handle, value.X, value.Y);
	}

	public override bool CursorRelativeMode {
		get => _cursorRelativeMode;
		set {
			_cursorRelativeMode = value;
			_glfw.SetInputMode(
				_handle, CursorStateAttribute.Cursor,
				value ? CursorModeValue.CursorDisabled : CursorModeValue.CursorNormal);
		}
	}

	public override Vector2 CursorScroll { get; set; }

	public override bool Closed {
		get => _closed;
	}

	public override void Init(WindowHints hints) {
		if (!_glfw.Init()) {
			throw new Error("glfw init failed");
		}

		if (!tryCreateCtx(hints, 4) && !tryCreateCtx(hints, 3)) {
			throw new Error("gl 3.0+ not supported");
		}

		// Locate at center.
		VideoMode* vm = _glfw.GetVideoMode(_glfw.GetPrimaryMonitor());
		float x = (vm->Width - hints.Size.X) / 2;
		float y = (vm->Height - hints.Size.Y) / 2;
		_glfw.SetWindowPos(_handle, (int) x, (int) y);

		// Set cursor data.
		if (hints.CursorImage != null) {
			fixed (byte* dataPtr = hints.CursorImage.Bytes) {
				GLFW_Image cimg = new GLFW_Image();
				cimg.Width = hints.CursorImage.Width;
				cimg.Height = hints.CursorImage.Height;
				cimg.Pixels = dataPtr;
				Cursor* cursorPtr = _glfw.CreateCursor(&cimg, (int) hints.CursorHotspot.X, (int) hints.CursorHotspot.Y);
				_glfw.SetCursor(_handle, cursorPtr);
			}
		}

		// Set icon data.
		if (hints.Icon != null) {
			fixed (byte* dataPtr = hints.Icon.Bytes) {
				GLFW_Image cimg = new GLFW_Image();
				cimg.Width = hints.Icon.Width;
				cimg.Height = hints.Icon.Height;
				cimg.Pixels = dataPtr;
				_glfw.SetWindowIcon(_handle, 1, &cimg);
			}
		}

		if (hints.Maximized) {
			_glfw.MaximizeWindow(_handle);
		}

		hookGLFWCallbacks();

		_glfw.MakeContextCurrent(_handle);
		_glfw.SwapInterval(hints.Vsync ? 1 : 0);

		// Finally show the window to hide the setting process.
		if (hints.Visible) {
			_glfw.ShowWindow(_handle);
		}
	}

	private bool tryCreateCtx(in WindowHints hints, int major) {
		_glfw.DefaultWindowHints();
		_glfw.WindowHint(WindowHintBool.Decorated, hints.Decorated);
		_glfw.WindowHint(WindowHintBool.Floating, hints.Floating);
		_glfw.WindowHint(WindowHintBool.Resizable, hints.Resizable);
		_glfw.WindowHint(WindowHintBool.Maximized, hints.Maximized);
		_glfw.WindowHint(WindowHintBool.AutoIconify, hints.AutoIconify);
		_glfw.WindowHint(WindowHintBool.FocusOnShow, hints.FocusOnShow);
		_glfw.WindowHint(WindowHintInt.Samples, 0);
		_glfw.WindowHint(WindowHintBool.DoubleBuffer, true);
		_glfw.WindowHint(WindowHintBool.Visible, false);
		_glfw.WindowHint(WindowHintInt.ContextVersionMajor, major);
		_glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, false);
		_glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
		_glfw.WindowHint(WindowHintBool.OpenGLDebugContext, hints.DebugContext);
		_debug = hints.DebugContext;

		_handle = _glfw.CreateWindow((int) hints.Size.X, (int) hints.Size.Y, hints.Title, null, null);
		if (_handle == null) {
			return false;
		}
		return true;
	}

	public override WindowOpaqueContext GetOpaqueContext() {
		return new WindowOpaqueContext(proc => _glfw.GetProcAddress(proc));
	}

	public override void ProcessWindowEvents() {
		_glfw.PostEmptyEvent();
		_glfw.WaitEventsTimeout(0.1);
	}

	public override void Present() {
		_glfw.SwapBuffers(_handle);
	}

	public override KeyStatus GetStatus(KeyCode code) {
		return (KeyStatus) _keyStatusMap.GetValueOrDefault((int) code, (byte) KeyStatus.Release);
	}

	public override KeyModifier GetModifiers(KeyCode code) {
		return (KeyModifier) _keyModMap.GetValueOrDefault((int) code, (int) KeyModifier.None);
	}

	public override void Dispose() {
		_glfw.SetWindowShouldClose(_handle, true);
		_glfw.Terminate();
		GC.SuppressFinalize(this);
	}

	private static T keepAlive<T>(T v) where T : class {
		// Allocate a gc handle to keep it alive.
		_ = GCHandle.Alloc(v, GCHandleType.Normal);
		return v;
	}

	private void hookGLFWCallbacks() {
		_glfw.SetWindowCloseCallback(
			_handle, keepAlive<GlfwCallbacks.WindowCloseCallback>(_ => {
				_glfw.SetWindowShouldClose(_handle, true);
				_closed = true;
			}));
		_glfw.SetCharCallback(
			_handle, keepAlive<GlfwCallbacks.CharCallback>((_, codepoint) => {
				CharInputEvent?.Invoke((char) codepoint);
			}));
		_glfw.SetKeyCallback(
			_handle, keepAlive<GlfwCallbacks.KeyCallback>((_, key, _, action, mods) => {
				KeyEvent?.Invoke((KeyCode) key, (KeyModifier) mods, (KeyStatus) action);
				_keyStatusMap[(int) key] = (byte) action;
				_keyModMap[(int) key] = (int) mods;
			}));
		_glfw.SetMouseButtonCallback(
			_handle, keepAlive<GlfwCallbacks.MouseButtonCallback>((_, key, action, mods) => {
				key += (int) KeyCode.MouseLeft; // Add an offset.
				KeyEvent?.Invoke((KeyCode) key, (KeyModifier) mods, (KeyStatus) action);
				_keyStatusMap[(int) key] = (byte) action;
			}));
		_glfw.SetCursorPosCallback(
			_handle, keepAlive<GlfwCallbacks.CursorPosCallback>((_, x, y) => {
				_cursor = new Vector2((float) x, (float) y);
			}));
		_glfw.SetScrollCallback(
			_handle, keepAlive<GlfwCallbacks.ScrollCallback>((_, x, y) => {
				CursorScroll = new Vector2((float) x, (float) y);
			}));
		_glfw.SetWindowSizeCallback(
			_handle, keepAlive<GlfwCallbacks.WindowSizeCallback>((_, width, height) => {
				WindowResizeEvent?.Invoke(new Vector2(width, height));
			}));
		_glfw.SetWindowFocusCallback(
			_handle, keepAlive<GlfwCallbacks.WindowFocusCallback>((_, focused) => {
				WindowFocusEvent?.Invoke(focused);
			}));
		_glfw.SetWindowPosCallback(
			_handle, keepAlive<GlfwCallbacks.WindowPosCallback>((_, x, y) => {
				WindowMoveEvent?.Invoke(new Vector2(x, y));
			}));
		_glfw.SetWindowMaximizeCallback(
			_handle, keepAlive<GlfwCallbacks.WindowMaximizeCallback>((_, maximized) => {
				WindowMaximizeEvent?.Invoke(maximized);
			}));
		_glfw.SetWindowIconifyCallback(
			_handle, keepAlive<GlfwCallbacks.WindowIconifyCallback>((_, iconified) => {
				WindowMaximizeEvent?.Invoke(iconified);
			}));
		_glfw.SetCursorEnterCallback(
			_handle, keepAlive<GlfwCallbacks.CursorEnterCallback>((_, entered) => {
				CursorEnterLeftEvent?.Invoke(entered);
			}));
		_glfw.SetDropCallback(
			_handle, keepAlive<GlfwCallbacks.DropCallback>((_, count, paths) => {
				var urls = new List<Url>();
				for (int i = 0; i < count; i++) {
					IntPtr pathPtr = ((IntPtr*) paths)[i];
					string? filePath = Marshal.PtrToStringUTF8(pathPtr);
					if (!string.IsNullOrEmpty(filePath)) {
						urls.Add(new Url(filePath));
					}
				}
				CursorDropEvent?.Invoke(urls.ToArray());
			}));
	}
}
