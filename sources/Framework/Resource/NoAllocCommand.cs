namespace Mino.Framework.Resource;

using unsafe CmdPtr = delegate* managed<object, ThreadContext, byte[]?, void>;

/// <summary>
///		Allocation-free context command.
/// </summary>
public unsafe struct NoAllocCommand {
	private CmdPtr _ptr;
	private object _pin;
	private byte[]? _data;

	public readonly void Execute(ThreadContext ctx) {
		_ptr(_pin, ctx, _data);
	}

	///  <summary>
	/// 		Refers to a object and creates a no alloc command.
	///  </summary>
	///  <param name="obj">Object to pin.</param>
	///  <param name="ptr">Static function pointer.</param>
	///  <param name="data">Optional additional data.</param>
	///  <returns></returns>
	public static NoAllocCommand Create(object? obj, CmdPtr ptr, byte[]? data = null) {
		return new NoAllocCommand {
			_ptr = ptr,
			_pin = obj!, // Ignore nullability.
			_data = data
		};
	}
}
