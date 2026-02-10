using System.Globalization;
using Mino.Nio;

namespace Mino.Framework;

/// <summary>
///     Multi-targeted logger.
/// </summary>
public interface Logger : IDisposable {
	/// <summary>
	///     Log levels.
	/// </summary>
	public enum Level {
		Debug,
		Info,
		Warn,
		Fatal
	}

	private static Logger? _default;
	private static readonly Lock _lock = new Lock();

	/// <summary>
	///     A default global logger, whose implementation depends.
	/// </summary>
	static Logger Global {
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
	///     Adds a write target to the logger.
	/// </summary>
	/// <param name="url">The target url.</param>
	void SetWriteTo(in Url url);

	/// <summary>
	///     Enables to log debug level messages.
	/// </summary>
	void EnableDebug();

	/// <summary>
	///     Enables noexcept mode: any exception will be re-thrown.
	/// </summary>
	void EnableNoexcept();

	/// <summary>
	///     Flushes the log streams.
	/// </summary>
	void Flush();

	/// <summary>
	///     Prints a log, may be not synchronous.
	/// </summary>
	/// <param name="level">The log level.</param>
	/// <param name="msg">The message to log.</param>
	/// <param name="ex">Optional exception to log.</param>
	void Log(Level level, string msg, Exception? ex);

	public void Info(string msg) {
		Log(Level.Info, msg, null);
	}

	public void Info(Exception ex) {
		Log(Level.Info, ex.Message, ex);
	}

	public void Warn(string msg) {
		Log(Level.Warn, msg, null);
	}

	public void Warn(Exception ex) {
		Log(Level.Warn, ex.Message, ex);
	}

	public void Fatal(string msg) {
		Log(Level.Fatal, msg, null);
	}

	public void Fatal(Exception ex) {
		Log(Level.Fatal, ex.Message, ex);
	}

	public void Debug(string msg) {
		Log(Level.Debug, msg, null);
	}

	public void Debug(Exception ex) {
		Log(Level.Debug, ex.Message, ex);
	}

	/// <summary>
	///     Gets a standard log line.
	/// </summary>
	/// <param name="level">The log level.</param>
	/// <param name="msg">The message to log.</param>
	/// <param name="ex">Optional exception.</param>
	/// <returns></returns>
	public static string FormatLog(Level level, string msg, Exception? ex) {
		string timestamp = DateTime.UtcNow.ToString("u", CultureInfo.CurrentCulture);
		string levelStr = level.ToString();
		if (ex == null) {
			return $"{timestamp} {levelStr}. {msg}\n";
		}
		return $"{timestamp} {levelStr}. {ex.GetType().Name}: {msg}\n";
	}
}
