namespace Mino.Framework.Resource;

/// <summary>
///     For thread context dep injection.
/// </summary>
public interface ThreadContextHolder {
	void Listen(ThreadContext ctx);

	bool TryGetThreadContext(out ThreadContext ctx);
}
