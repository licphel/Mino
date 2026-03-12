namespace Mino.Framework;

/// <summary>
///		Key identifier.
/// </summary>
public readonly struct Identifier : IEquatable<Identifier> {
	public readonly string Scope;
	public readonly string Key;
	private readonly int hash;
	private readonly string full;

	/// <summary>
	///		Fallbacks to a default scope.
	/// </summary>
	/// <param name="scope">Default scope.</param>
	/// <param name="key">Key to convert.</param>
	/// <returns>An identifier.</returns>
	public static Identifier Fallback(string scope, string key) {
		if (key.Contains(':')) {
			return new Identifier(key);
		}
		return new Identifier(scope, key);
	}

	public Identifier(string scope, string key) {
		Scope = scope;
		Key = key;
		hash = HashCode.Combine(Scope, Key);
		if (string.IsNullOrEmpty(Scope)) {
			full = Key;
		}
		full = Scope + ":" + Key;
	}

	public Identifier(string path) {
		if (!path.Contains(':')) {
			Key = path;
			Scope = "";
		} else {
			string[] arr = path.Split(':');
			Scope = arr[0];
			Key = arr[1];
		}
		
		hash = HashCode.Combine(Scope, Key);
		if (string.IsNullOrEmpty(Scope)) {
			full = Key;
		}
		full = path;
	}

	public override bool Equals(object? obj) {
		if (obj is not Identifier identity) {
			return false;
		}
		return this == identity;
	}

	public override int GetHashCode() {
		return hash;
	}

	public static bool operator ==(Identifier i1, Identifier i2) {
		if (i1.hash != i2.hash) {
			return false;
		}
		return i1.Key.Equals(i2.Key) && i1.Scope.Equals(i2.Scope);
	}

	public static bool operator !=(Identifier i1, Identifier i2) {
		return !(i1 == i2);
	}

	public override string ToString() {
		return full;
	}

	public static implicit operator Identifier(string s) {
		return new Identifier(s);
	}

	public static implicit operator string(Identifier idt) {
		return idt.ToString();
	}
	
	public bool Equals(Identifier other) {
		return hash == other.hash && full == other.full;
	}
}
