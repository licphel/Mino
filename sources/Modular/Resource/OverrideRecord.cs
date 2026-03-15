namespace Mino.Modular.Resource;

/// <summary>
///		Event: on asset overriding phase.
/// </summary>
public class OverrideRecord {
	private Dictionary<string, List<string>> _rec = new Dictionary<string, List<string>>();

	/// <summary>
	///		Records an override.
	/// </summary>
	/// <param name="overrider">Overrider.</param>
	/// <param name="overriden">Overriden mod.</param>
	public void Record(string overrider, string overriden) {
		if (_rec.TryGetValue(overriden, out List<string>? list)) {
			list.Add(overrider);
		} else {
			_rec[overriden] = [overrider];
		}
	}

	/// <summary>
	///		Gets all overriders that once modified a mod.
	/// </summary>
	/// <param name="overriden">Overriden mod.</param>
	/// <returns>An array containing all overriders in turn.</returns>
	public string[] GetOverriders(string overriden) {
		return _rec.GetValueOrDefault(overriden, []).ToArray();
	}
}
