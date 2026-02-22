namespace Mino.Framework.Resource;

/// <summary>
///     A thread-held context.
/// </summary>
public interface ThreadContext : IDisposable {
	/// <summary>
	///     Initializes the context.
	/// </summary>
	void Init();

	/// <summary>
	///     Raises a request to poll events.
	/// </summary>
	void PollEvents();

	/// <summary>
	///     Runs all commands.
	/// </summary>
	public void Present();

	/// <summary>
	///     Raises a request to execute a command.
	/// </summary>
	/// <param name="cmd">Command to pend.</param>
	void Pend(in NoAllocCommand cmd);

	/// <summary>
	///		Pends an action on the context thread.
	///		Warning: this may lead to low performance.
	/// </summary>
	/// <param name="action">Action to pend.</param>
	void Pend(Action action);

	/// <summary>
	///     The factory to create thread-held resources.
	/// </summary>
	ResourceFactory<ThreadContextHolder> Factory { get; }

	/// <summary>
	///     The context thread.
	/// </summary>
	public Thread? CtxThread { get; }
}
