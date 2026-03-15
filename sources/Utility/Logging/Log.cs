namespace Mino.Utility.Logging;

/// <summary>
///		Static logger.
/// </summary>
public static class Log {
	private static Logger? _default;
	private static readonly Lock _lock = new Lock();

	/// <summary>
	///     A default logger instance, whose implementation depends.
	/// </summary>
	public static Logger Instance {
		get {
			if (_default == null) {
				lock (_lock) {
					_default ??= new LoggerSync();
				}
			}
			return _default;
		}
	}
	
	/// <summary>
	///     Prints a log, may be not synchronous.
	/// </summary>
	/// <param name="level">The log level.</param>
	/// <param name="msg">The message to log.</param>
	/// <param name="ex">Optional exception to log.</param>
	public static void Print(Severity level, string? msg, Exception? ex, bool header) {
		Instance.Print(level, msg, ex, header);
	}
	
	public static void Debug(string msg, Exception? ex = null, bool header = true) {
		Print(Severity.Debug, msg, ex, header);
	}

	public static void Info(string msg, Exception? ex = null, bool header = true) {
		Print(Severity.Info, msg, ex, header);
	}
	
	public static void Warn(string msg, Exception? ex = null, bool header = true) {
		Print(Severity.Warn, msg, ex, header);
	}

	public static void Fatal(string msg, Exception? ex = null, bool header = true) {
		Print(Severity.Fatal, msg, ex, header);
	}
	
	public static void Debug(Exception ex, bool header = true) {
		Print(Severity.Debug, null, ex, header);
	}

	public static void Info(Exception ex, bool header = true) {
		Print(Severity.Info, null, ex, header);
	}
	
	public static void Warn(Exception ex, bool header = true) {
		Print(Severity.Warn, null, ex, header);
	}

	public static void Fatal(Exception ex, bool header = true) {
		Print(Severity.Fatal, null, ex, header);
	}
}
