#region
using Mino.Nio;
using Mino.Utility.Logging;
#endregion

namespace Mino.Framework;

/// <summary>
///     Framework setups.
/// </summary>
public static class FrameworkSetup {
	/// <summary>
	///     Framework version.
	/// </summary>
	public static readonly Version Version = new Version(1, 0, 0);
	public static readonly int Iterations = 0;
	public static string[] Args = [];
	public static bool InDev = false;
	public static Url __Basepath = string.Empty;

	/*
	 * Available args:
	 * --debug: Enable logger debug output to console
	 * --noexcept: Throw when logger gets an error
	 * --indev: In-development mode will redirect url root
	 */
	
	public static void Start(string[] args) {
		Console.WriteLine(
			"""
			-------------------------------
			| M I N O   F R A M E W O R K |
			-------------------------------
			| Starting up...              |
			-------------------------------
			""");

		/*
		 * For better logging, we give main thread a name.
		 * Maybe users can change it? up to them
		 */
		Thread.CurrentThread.Name = "Main";
		Args = args;

		if (args.Contains("--indev")) {
			InDev = true;
			
			string exePath = AppDomain.CurrentDomain.BaseDirectory;
			string currentDir = exePath;
    
			while (!string.IsNullOrEmpty(currentDir)) {
				if (Directory.GetFiles(currentDir, "*.csproj").Length != 0 ||
				Directory.GetFiles(currentDir, "*.sln").Length != 0) {
					__Basepath = currentDir;
					Log.Info($"In-dev mode is on. Basepath switched to '{__Basepath}'");
					break;
				}
				currentDir = Path.GetDirectoryName(currentDir)!;
			}
			
			if (string.IsNullOrEmpty(__Basepath)) {
				__Basepath = exePath;
				Log.Info($"In-dev mode is on, however, we cannot find a proper project base. Basepath fallbacks to '{__Basepath}'");
			}
			
		} else {
			__Basepath = AppDomain.CurrentDomain.BaseDirectory;
		}
		
		if (args.Contains("--debug")) {
			Log.Instance.EnableDebug();
		}
		if (args.Contains("--noexcept")) {
			Log.Instance.EnableNoexcept();
		}
	}
}
