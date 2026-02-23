namespace Mino.Desktop;

/// <summary>
///     Opaque proc address resolver.
/// </summary>
public class ProcAddress {
	private Func<string, IntPtr> _gProcAddress;

	public ProcAddress(Func<string, IntPtr> gProcAddress) {
		_gProcAddress = gProcAddress;
	}

	public IntPtr GetProcAddress(string proc) {
		return _gProcAddress.Invoke(proc);
	}
}
