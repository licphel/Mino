#region
using System.Collections.Concurrent;
#endregion

namespace Mino.Framework.Resource;

/// <summary>
///     Thread context with batch processing support.
/// </summary>
public abstract unsafe class AbstractThreadContext : ThreadContext {
	private readonly BlockingCollection<NoAllocCommand> _commandQueue = new BlockingCollection<NoAllocCommand>(64);
	private readonly CancellationTokenSource _cts = new CancellationTokenSource();
	private volatile bool _disposed;
	private volatile bool _initialized;
	private readonly Lock _lock = new Lock();

	public ResourceFactory<ThreadContextHolder> Factory { get; } = new ResourceFactory<ThreadContextHolder>();
	public Thread CtxThread { get; }

	protected AbstractThreadContext() {
		CtxThread = new Thread(loop) {
			Name = GetType().Name,
			IsBackground = true,
			Priority = ThreadPriority.AboveNormal
		};
	}

	public void Init() {
		lock (_lock) {
			if (_initialized) {
				return;
			}
			CtxThread.Start();
			_initialized = true;
			OnInit();
		}
	}

	public virtual void PollEvents() { }

	public void Pend(in NoAllocCommand cmd) {
		_commandQueue.Add(cmd);
	}

	public void Pend(Action action) {
		Pend(NoAllocCommand.Create(action, &wrapper));
		return;

		static void wrapper(object? obj, ThreadContext ctx) {
			Action? action = obj as Action;
			action?.Invoke();
		}
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_cts.Cancel();
		CtxThread.Join(1000);
		_cts.Dispose();
		OnDispose();
		GC.SuppressFinalize(this);
	}

	private void loop() {
		try {
			OnContextStart();
			
			while (!_cts.Token.IsCancellationRequested) {
				// Bug fixed: No SpinWait, no Sleep.
				// (for fast screen recording)
				// use a blocking queue is much better.
				_commandQueue.Take().Execute(this);
			}
		} finally {
			OnContextStop();
		}
	}

	protected virtual void OnInit() { }
	protected virtual void OnContextStart() { }
	protected virtual void OnContextStop() { }
	protected virtual void OnDispose() { }
}
