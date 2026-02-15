namespace Mino.Framework.XPlatform;

/// <summary>
///     Validates service availability.
/// </summary>
public interface ServiceProvider {
	/// <summary>
	///     Checks if a backend works on current platform.
	/// </summary>
	/// <returns></returns>
	public bool CheckWork() {
		return true;
	}
}
