#region
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using Mino.Nio;
#endregion

namespace Mino.Utility.Logging;

/// <summary>
///     Default asynchronous logger.
/// </summary>
public class LoggerAsync : Logger {
	private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();
	private readonly CancellationTokenSource _cts = new CancellationTokenSource();
	private readonly Task _processor;
	private readonly ConcurrentBag<StreamWriter> _writers = new ConcurrentBag<StreamWriter>();
	private bool _debugEnabled;
	private bool _noexcept;
	private bool _disposed;
	private readonly TaskCompletionSource _completed = new TaskCompletionSource();
	
	public LoggerAsync() {
		_processor = Task.Run(processAsync);
	}

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

	public void Print(Severity level, string? msg, Exception? ex, bool header) {
		if (level == Severity.Debug && !_debugEnabled) {
			return;
		}

		string ln = Logger.FormatLog(level, msg, ex, header);
		if (!_channel.Writer.TryWrite(ln)) {
			// Instead use console.
			Console.WriteLine(ln);
		}

		if (ex != null && _noexcept) {
			throw ex;
		}
	}

	private async Task processAsync() {
		while (!_cts.IsCancellationRequested) {
			try {
				string line = await _channel.Reader.ReadAsync(_cts.Token);
				
				foreach (StreamWriter writer in _writers) {
					await writer.WriteLineAsync(line);
					await writer.FlushAsync();
				}
			} catch (OperationCanceledException) {
				break;
			} catch {
				await Task.Delay(100);
			}
		}
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		GC.SuppressFinalize(this);

		_channel.Writer.Complete();
		
		try {
			// Wait at most 1.5s.
			_processor.Wait((int) (1000 * 1.5));
		} catch (AggregateException) {
			// Ignored
		}
		
		foreach (StreamWriter? writer in _writers) {
			writer?.Dispose();
		}
		_cts.Dispose();
	}
}
