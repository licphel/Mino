#region
using System.Globalization;
using Mino.Nio;
#endregion

namespace Mino.Utility.Logging;

/// <summary>
///     Multi-targeted logger.
/// </summary>
public interface Logger : IDisposable {
	/// <summary>
	///     Adds a write target to the logger.
	/// </summary>
	/// <param name="url">The target url.</param>
	void OutputTo(in Url url);

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
	/// <param name="severity">The log level.</param>
	/// <param name="msg">The message to log.</param>
	/// <param name="ex">Optional exception to log.</param>
	/// <param name="header">Whether to use logging header.</param>
	void Print(Severity severity, string? msg, Exception? ex, bool header);

	/// <summary>
	///     Gets a standard log line.
	/// </summary>
	/// <param name="level">The log level.</param>
	/// <param name="msg">The message to log.</param>
	/// <param name="ex">Optional exception.</param>
	/// <param name="header">Whether to use logging header.</param>
	/// <returns>A formatted line.</returns>
	public static string FormatLog(Severity level, string? msg, Exception? ex, bool header) {
		string timestamp = DateTime.UtcNow.ToString("u", CultureInfo.CurrentCulture);
		string levelStr = level.ToString();
		string threadName = Thread.CurrentThread.Name ?? "-";
		string head = header ? $"[{timestamp}] [{threadName}/{levelStr}]" : string.Empty;
		if (ex == null) {
			return $"{head} {msg}";
		}
		if (msg != null) {
			return $"{head} {msg}: {ex}";
		}
		return $"{head} {ex}";
	}
}