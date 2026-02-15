namespace Mino.Desktop;

/// <summary>
///     Opaque window context locator.
/// </summary>
public class WindowOpaqueContext {
	private Func<string, IntPtr> _gProcAddress;

	public WindowOpaqueContext(Func<string, IntPtr> gProcAddress) {
		_gProcAddress = gProcAddress;
	}

	public IntPtr GetProcAddress(string proc) {
		return _gProcAddress.Invoke(proc);
	}
}
