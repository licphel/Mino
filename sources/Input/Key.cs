#region
using System.Collections.Concurrent;
using Mino.Desktop;
using Mino.Graphics.Hardware;
#endregion

namespace Mino.Input;

/// <summary>
///     Provides key input observation.
/// </summary>
public class Key {
	private static readonly ConcurrentBag<Thread> _listenerThreads = new ConcurrentBag<Thread>();
	private static readonly ConcurrentDictionary<Thread, bool[]> _threadStamps =
		new ConcurrentDictionary<Thread, bool[]>();
	private static readonly ConcurrentDictionary<uint, Key> _activeListeners =
		new ConcurrentDictionary<uint, Key>();
	private static bool _isEventHooked;

	private Window _window;

	// Private ctor - we use key listeners for better performance.
	private Key(uint code) {
		_window = RenderSystem.GetWindow();
		Code = code;

		if (!_isEventHooked) {
			_window.KeyEvent += keyCallback;
			_isEventHooked = true;
		}
	}

	/// <summary>
	///     Key code of the listener.
	/// </summary>
	public uint Code { get; }

	/// <summary>
	///     The key is just pressed.
	/// </summary>
	public bool Press {
		get => isThisRollActivated() && Hold;
	}

	/// <summary>
	///     The key is being held.
	/// </summary>
	public bool Hold {
		get => _window.GetStatus(Code) != KeyStatus.Release;
	}

	/// <summary>
	///     The key is repeating.
	/// </summary>
	public bool Repeat {
		get => _window.GetStatus(Code) == KeyStatus.Repeat;
	}

	/// <summary>
	///     The key is just pressed or repeating.
	/// </summary>
	private bool React {
		get => Press || Repeat;
	}

	/// <summary>
	///     Returns if the key is pressed with given modifiers.
	/// </summary>
	/// <param name="mod">Modifiers, can be 'Any'.</param>
	/// <returns>True if the modifier combination is applied.</returns>
	public bool With(uint mod) {
		if (mod == ModAny) {
			return true;
		}
		return (_window.GetModifiers(Code) & mod) == mod;
	}

	private bool isThisRollActivated() {
		if (_threadStamps.TryGetValue(Thread.CurrentThread, out bool[]? map)) {
			return map[(int) Code];
		}
		return false;
	}

	/// <summary>
	///     Gets a key listener instance.
	/// </summary>
	/// <param name="code">Key code.</param>
	/// <returns>A cached key listener.</returns>
	public static Key Get(uint code) {
		if (_activeListeners.TryGetValue(code, out Key? value)) {
			return value;
		}
		return _activeListeners[code] = new Key(code);
	}

	/// <summary>
	///     Enable a thread to listener managed key events.
	/// </summary>
	/// <param name="thread">Listener thread.</param>
	public static void AddListeningThread(Thread thread) {
		_listenerThreads.Add(thread);
	}

	/// <summary>
	///     Ends a listening roll.
	/// </summary>
	public static void NextListeningRoll() {
		if (_threadStamps.TryGetValue(Thread.CurrentThread, out bool[]? map)) {
			Array.Fill(map, false);
		}
	}

	// Window key event callback:
	// Notify current thread keys.
	private static void keyCallback(uint keycode, uint modifier, KeyStatus status) {
		if (status == KeyStatus.Press) {
			foreach (Thread thread in _listenerThreads) {
				if (!_threadStamps.TryGetValue(thread, out bool[]? value)) {
					value = new bool[1024];
					_threadStamps[thread] = value;
				}
				value[(int) keycode] = true;
			}
		}
	}

	#region KEYCODES
	public const uint Space = 32;
	public const uint Apostrophe = 39;
	public const uint Comma = 44;
	public const uint Minus = 45;
	public const uint Period = 46;
	public const uint Slash = 47;
	public const uint D0 = 48;
	public const uint D1 = 49;
	public const uint D2 = 50;
	public const uint D3 = 51;
	public const uint D4 = 52;
	public const uint D5 = 53;
	public const uint D6 = 54;
	public const uint D7 = 55;
	public const uint D8 = 56;
	public const uint D9 = 57;
	public const uint Semicolon = 59;
	public const uint Equal = 61;
	public const uint A = 65;
	public const uint B = 66;
	public const uint C = 67;
	public const uint D = 68;
	public const uint E = 69;
	public const uint F = 70;
	public const uint G = 71;
	public const uint H = 72;
	public const uint I = 73;
	public const uint J = 74;
	public const uint K = 75;
	public const uint L = 76;
	public const uint M = 77;
	public const uint N = 78;
	public const uint O = 79;
	public const uint P = 80;
	public const uint Q = 81;
	public const uint R = 82;
	public const uint S = 83;
	public const uint T = 84;
	public const uint U = 85;
	public const uint V = 86;
	public const uint W = 87;
	public const uint X = 88;
	public const uint Y = 89;
	public const uint Z = 90;
	public const uint LeftBracket = 91;
	public const uint Backslash = 92;
	public const uint RightBracket = 93;
	public const uint GraveAccent = 96;
	public const uint World1 = 161;
	public const uint World2 = 162;
	public const uint Escape = 256;
	public const uint Enter = 257;
	public const uint Tab = 258;
	public const uint Backspace = 259;
	public const uint Insert = 260;
	public const uint Delete = 261;
	public const uint Right = 262;
	public const uint Left = 263;
	public const uint Down = 264;
	public const uint Up = 265;
	public const uint PageUp = 266;
	public const uint PageDown = 267;
	public const uint Home = 268;
	public const uint End = 269;
	public const uint CapsLock = 280;
	public const uint ScrollLock = 281;
	public const uint NumLock = 282;
	public const uint PrintScreen = 283;
	public const uint Pause = 284;
	public const uint F1 = 290;
	public const uint F2 = 291;
	public const uint F3 = 292;
	public const uint F4 = 293;
	public const uint F5 = 294;
	public const uint F6 = 295;
	public const uint F7 = 296;
	public const uint F8 = 297;
	public const uint F9 = 298;
	public const uint F10 = 299;
	public const uint F11 = 300;
	public const uint F12 = 301;
	public const uint F13 = 302;
	public const uint F14 = 303;
	public const uint F15 = 304;
	public const uint F16 = 305;
	public const uint F17 = 306;
	public const uint F18 = 307;
	public const uint F19 = 308;
	public const uint F20 = 309;
	public const uint F21 = 310;
	public const uint F22 = 311;
	public const uint F23 = 312;
	public const uint F24 = 313;
	public const uint F25 = 314;
	public const uint Kp0 = 320;
	public const uint Kp1 = 321;
	public const uint Kp2 = 322;
	public const uint Kp3 = 323;
	public const uint Kp4 = 324;
	public const uint Kp5 = 325;
	public const uint Kp6 = 326;
	public const uint Kp7 = 327;
	public const uint Kp8 = 328;
	public const uint Kp9 = 329;
	public const uint KpDecimal = 330;
	public const uint KpDivide = 331;
	public const uint KpMultiply = 332;
	public const uint KpSubtract = 333;
	public const uint KpAdd = 334;
	public const uint KpEnter = 335;
	public const uint KpEqual = 336;
	public const uint LeftShift = 340;
	public const uint LeftControl = 341;
	public const uint LeftAlt = 342;
	public const uint LeftSuper = 343;
	public const uint RightShift = 344;
	public const uint RightControl = 345;
	public const uint RightAlt = 346;
	public const uint RightSuper = 347;
	public const uint Menu = 348;
	public const uint MouseLeft = 512; // Magic number 512 to unify keyboard and mouse.
	public const uint MouseRight = 1 + MouseLeft;
	public const uint MouseMiddle = 2 + MouseLeft;
	public const uint Mouse4 = 3 + MouseLeft;
	public const uint Mouse5 = 4 + MouseLeft;
	public const uint Mouse6 = 5 + MouseLeft;
	public const uint Mouse7 = 6 + MouseLeft;
	public const uint Mouse8 = 7 + MouseLeft;
	#endregion

	#region KEYMODIFIERS
	public const uint ModNone = 0x0000;
	public const uint ModShift = 0x0001;
	public const uint ModControl = 0x0002;
	public const uint ModAlt = 0x0004;
	public const uint ModSuper = 0x0008;
	public const uint ModCapsLock = 0x0010;
	public const uint ModNumsLock = 0x0020;
	public const uint ModAny = 0xFFFF; // Any modifier is acceptable.
	#endregion
}
