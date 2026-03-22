namespace Mino.Utility.Flatten;

/// <summary>
///		A state key name.
/// </summary>
public sealed class Name {
	/// <summary>
	///		The key of the state.
	/// </summary>
	public readonly string Key;

	/// <summary>
	///		Initial value of the state.
	/// </summary>
	public readonly object InitValue;

	/// <summary>
	///		Legal values of the state.
	/// </summary>
	public readonly object[] Values;

	/// <summary>
	///		Palette of the state.
	/// </summary>
	public readonly Palette<object> Palette;
	
	public Name(string key, object initValue, params object[] vals) {
		Key = key;
		InitValue = initValue;
		Values = vals;
		Palette = new Palette<object>();

		// Palettes the objects for future use.
		foreach (object o in vals) {
			Palette.Add(o);
		}
	}
}