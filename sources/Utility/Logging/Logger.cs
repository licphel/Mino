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
	/// <param name="level">The log level.</param>
	/// <param name="msg">The message to log.</param>
	/// <param name="ex">Optional exception to log.</param>
	void Print(Severity level, string? msg, Exception? ex);

	/// <summary>
	///     Gets a standard log line.
	/// </summary>
	/// <param name="level">The log level.</param>
	/// <param name="msg">The message to log.</param>
	/// <param name="ex">Optional exception.</param>
	/// <returns></returns>
	public static string FormatLog(Severity level, string? msg, Exception? ex) {
		string timestamp = DateTime.UtcNow.ToString("u", CultureInfo.CurrentCulture);
		string levelStr = level.ToString();
		if (ex == null) {
			return $"{timestamp} {levelStr}. {msg}\n";
		}
		if (msg != null) {
			return $"{timestamp} {levelStr}. {msg}: {ex}\n";
		}
		return $"{timestamp} {levelStr}. {ex}\n";
	}
}