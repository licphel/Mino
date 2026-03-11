namespace Mino.Framework.Resource;

using unsafe CmdPtr = delegate* managed<object, ThreadContext, void>;

/// <summary>
///		Allocation-free context command.
/// </summary>
public unsafe struct NoAllocCommand {
	private CmdPtr _ptr;
	private object _pin;

	public readonly void Execute(ThreadContext ctx) {
		_ptr(_pin, ctx);
	}

	///  <summary>
	/// 		Refers to a object and creates a no alloc command.
	///  </summary>
	///  <param name="obj">Object to pin.</param>
	///  <param name="ptr">Static function pointer.</param>
	///  <returns></returns>
	public static NoAllocCommand Create(object? obj, CmdPtr ptr) {
		return new NoAllocCommand {
			_ptr = ptr,
			_pin = obj! // Ignore nullability.
		};
	}
}
