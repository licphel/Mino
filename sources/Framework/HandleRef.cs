namespace Mino.Framework;

/// <summary>
///     A handle reference.
/// </summary>
public class HandleRef {
	public uint Handle;

	public HandleRef(uint handle) {
		Handle = handle;
	}

	public static implicit operator uint(HandleRef mh) {
		return mh.Handle;
	}

	// Swaps two handles.
	public static void Swap(HandleRef m1, HandleRef m2) {
		(m1.Handle, m2.Handle) = (m2.Handle, m1.Handle);
	}
}
