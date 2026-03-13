#region
using Mino.Utility.Logging;
#endregion

namespace Mino.Framework;

/// <summary>
///     Framework setups.
/// </summary>
public static class MinoFramework {
	/// <summary>
	///     Framework version.
	/// </summary>
	public static readonly Version Version = new Version(1, 0, 0);
	public static readonly int Iterations = 0;
	public static string[] Args = [];

	public static void Start(string[] args) {
		Console.WriteLine(
			$"""
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

		if (args.Contains("--debug")) {
			Log.Instance.EnableDebug();
		}
		if (args.Contains("--noexcept")) {
			Log.Instance.EnableNoexcept();
		}
	}
}
