namespace Mino.Framework.BSP;

/// <summary>
///     Identifies current os platform.
/// </summary>
public static class Platform {
	/*
	 * Only support these mainstream platforms.
	 */
	public const uint Unknown = 0;
	public const uint Windows = 1 << 0;
	public const uint Linux = 1 << 1;
	public const uint MacOS = 1 << 2;
	// public const uint Android = 1 << 3;
	// public const uint IOS = 1 << 4;
	
	public const uint Desktop = Windows | Linux | MacOS;
	// public const uint Mobile = Android | IOS;
	// public const uint All = Desktop | Mobile;

	/// <summary>
	///     Gets current os platform.
	/// </summary>
	public static uint Current {
		get {
			if (OperatingSystem.IsWindows()) {
				return Windows;
			}
			if (OperatingSystem.IsLinux()) {
				return Linux;
			}
			if (OperatingSystem.IsMacOS()) {
				return MacOS;
			}
			if (OperatingSystem.IsAndroid()) {
				// return Android;
			}
			if (OperatingSystem.IsIOS()) {
				// return IOS;
			}
			return Unknown;
		}
	}
}
