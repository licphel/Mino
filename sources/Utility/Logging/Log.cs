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
					_default ??= new LoggerAsync();
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
	public static void Print(Severity level, string? msg, Exception? ex) {
		Instance.Print(level, msg, ex);
	}
	
	public static void Debug(string msg, Exception? ex = null) {
		Print(Severity.Debug, msg, ex);
	}

	public static void Info(string msg, Exception? ex = null) {
		Print(Severity.Info, msg, ex);
	}
	
	public static void Warn(string msg, Exception? ex = null) {
		Print(Severity.Warn, msg, ex);
	}

	public static void Fatal(string msg, Exception? ex = null) {
		Print(Severity.Fatal, msg, ex);
	}
	
	public static void Debug(Exception ex) {
		Print(Severity.Debug, null, ex);
	}

	public static void Info(Exception ex) {
		Print(Severity.Info, null, ex);
	}
	
	public static void Warn(Exception ex) {
		Print(Severity.Warn, null, ex);
	}

	public static void Fatal(Exception ex) {
		Print(Severity.Fatal, null, ex);
	}
}
