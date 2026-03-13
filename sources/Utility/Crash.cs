#region
using Mino.Utility.Logging;
#endregion

namespace Mino.Utility;

/// <summary>
///     A crash thrown from the framework.
/// </summary>
public class Crash : Exception {
	public Crash(string message) : base(message) {
		Log.Fatal(message);
	}

	public Crash(string message, Exception? innerException) : base(message, innerException) {
		Log.Fatal(message, innerException ?? this);
	}
}
