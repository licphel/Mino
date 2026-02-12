namespace Mino.Framework.XPlatform;

/// <summary>
///		Identifies current os platform.
/// </summary>
public static class Platform {
	/*
	 * Only support these mainstream platforms.
	 */
	public const uint UNKNOWN = 0;
	public const uint WINDOWS = 1 << 0;
	public const uint LINUX = 1 << 1;
	public const uint MACOS = 1 << 2;
	public const uint ANDROID = 1 << 3;
	public const uint IOS = 1 << 4;
	public const uint DESKTOP = WINDOWS | LINUX | MACOS;
	public const uint MOBILE = ANDROID | IOS;
	public const uint ALL = DESKTOP | MOBILE;
	
	/// <summary>
	///		Gets current os platform.
	/// </summary>
	public static uint Current {
		get {
			if (OperatingSystem.IsWindows()) {
				return WINDOWS;
			}
			if (OperatingSystem.IsLinux()) {
				return LINUX;
			}
			if (OperatingSystem.IsMacOS()) {
				return MACOS;
			}
			if (OperatingSystem.IsAndroid()) {
				return ANDROID;
			}
			if (OperatingSystem.IsIOS()) {
				return IOS;
			}
			return UNKNOWN;
		}
	}
}
