#region
using System.Collections.Concurrent;
using System.Text;
using Mino.Nio;
#endregion

namespace Mino.Utility.Logging;

/// <summary>
///     Default asynchronous logger.
/// </summary>
public class LoggerSync : Logger {
	private readonly ConcurrentBag<StreamWriter> _writers = new ConcurrentBag<StreamWriter>();
	private bool _debugEnabled;
	private bool _noexcept;
	private bool _disposed;
	
	public void OutputTo(in Url url) {
		Stream? stream = url.OpenStream("w");
		if (stream != null) {
			// Use no bom UTF-8.
			_writers.Add(new StreamWriter(stream, new UTF8Encoding(false), 1024));
		}
	}

	public void EnableDebug() {
		_debugEnabled = true;
	}

	public void EnableNoexcept() {
		_noexcept = true;
	}

	public void Flush() {
		foreach (StreamWriter? writer in _writers) {
			writer?.Flush();
		}
	}

	public void Print(Severity severity, string? msg, Exception? ex, bool header) {
		if (severity == Severity.Debug && !_debugEnabled) {
			return;
		}

		string line = Logger.FormatLog(severity, msg, ex, header);
		
		if (severity == Severity.Debug) {
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(line);
			Console.ResetColor();
		}
		else if (severity == Severity.Info) {
			Console.WriteLine(line);
		} else {
			Console.Error.WriteLine(line);
		}
				
		foreach (StreamWriter writer in _writers) {
			writer.WriteLine(line);
			writer.Flush();
		}

		if (ex != null && _noexcept) {
			throw ex;
		}
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		GC.SuppressFinalize(this);
		
		foreach (StreamWriter? writer in _writers) {
			writer?.Dispose();
		}
	}
}
