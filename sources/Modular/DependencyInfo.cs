namespace Mino.Modular;

/// <summary>
///		A mod dep info.
/// </summary>
public readonly struct DependencyInfo {
	public readonly string ModId;
	public readonly Version? MinVersion;
	public readonly Version? MaxVersion;
	
	public DependencyInfo(string modId, Version? minVersion = null, Version? maxVersion = null) {
		ModId = modId;
		MinVersion = minVersion;
		MaxVersion = maxVersion;
	}

	public override string ToString() {
		string i0 = $"'{ModId}'";
		if (MinVersion != null) {
			i0 += $" >={MinVersion}";
		}
		if (MaxVersion != null) {
			i0 += $" <={MaxVersion}";
		}
		return i0;
	}
}
