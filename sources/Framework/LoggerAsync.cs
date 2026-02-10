using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using Mino.Nio;

namespace Mino.Framework;

/// <summary>
///     Default asynchronous logger.
/// </summary>
public class LoggerAsync : Logger {
	private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();
	private readonly CancellationTokenSource _cts = new CancellationTokenSource();
	private readonly Task _processor;
	private readonly ConcurrentBag<StreamWriter> _writers = new ConcurrentBag<StreamWriter>();
	private bool _debugEnabled;
	private bool _disposed;
	private bool _rethrow;

	public LoggerAsync() {
		_processor = Task.Run(processAsync);
	}

	public void SetWriteTo(in Url url) {
		Stream? stream = url.OpenStream();
		if (stream != null) {
			// Use no bom UTF-8.
			_writers.Add(new StreamWriter(stream, new UTF8Encoding(false), 1024));
		}
	}

	public void EnableDebug() {
		_debugEnabled = true;
	}

	public void EnableNoexcept() {
		_rethrow = true;
	}

	public void Flush() {
		foreach (StreamWriter? writer in _writers) {
			writer?.Flush();
		}
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_channel.Writer.Complete();
		_processor.Wait(100);
		foreach (StreamWriter? writer in _writers) {
			writer?.Dispose();
		}
		_cts.Dispose();
		GC.SuppressFinalize(this);
	}

	public void Log(Logger.Level level, string msg, Exception? ex) {
		if (level == Logger.Level.Debug && !_debugEnabled) {
			return;
		}
		_channel.Writer.TryWrite(Logger.FormatLog(level, msg, ex));
		if (_rethrow && ex != null) {
			throw ex;
		}
	}

	private async Task processAsync() {
		while (!_cts.IsCancellationRequested) {
			try {
				string line = await _channel.Reader.ReadAsync(_cts.Token);
				foreach (StreamWriter writer in _writers) {
					try {
						await writer.WriteAsync(line);
						await writer.FlushAsync();
					} catch {
						_writers.TryTake(out _);
					}
				}
			} catch (OperationCanceledException) {
				break;
			} catch {
				await Task.Delay(100);
			}
		}
	}
}
