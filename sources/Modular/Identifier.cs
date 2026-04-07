using Mino.Utility;

namespace Mino.Modular;

/// <summary>
///		Key identifier.
/// </summary>
public readonly struct Identifier : IEquatable<Identifier> {
	public readonly Domain Domain;
	public readonly string Path;
	private readonly int hash;
	private readonly string full;

	/// <summary>
	///		Fallbacks to a default domain.
	/// </summary>
	/// <param name="domain">Default domain.</param>
	/// <param name="path">Path to convert.</param>
	/// <returns>An identifier.</returns>
	public static Identifier Fallback(Domain domain, string path) {
		if (path.Contains(':')) {
			return Of(path);
		}
		return new Identifier(domain, path);
	}

	private Identifier(Domain domain, string path) {
		Domain = domain;
		Path = path;
		hash = HashCode.Combine(Domain, Path);
		full = Domain + ":" + Path;

		if (!Domain.Validate(full)) {
			throw new Crash($"Identifier invalid: {full}");
		}
	}

	public static Identifier Of(string full) {
		Domain? domain;
		string path;
		
		if (!full.Contains(':')) {
			path = full;
			domain = Domain.Unknown;
		} else {
			string[] arr = full.Split(':');
			if (arr.Length != 2) {
				throw new Crash($"Cannot parse {full}");
			}
			
			domain = Domain.TryFind(arr[0]);
			path = arr[1];
		}

		return new Identifier(domain, path);
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
		return i1.Path.Equals(i2.Path) && i1.Domain.Equals(i2.Domain);
	}

	public static bool operator !=(Identifier i1, Identifier i2) {
		return !(i1 == i2);
	}

	public override string ToString() {
		return full;
	}

	// string -> Id.
	public static implicit operator Identifier(string str) {
		return Of(str);
	}

	// Id -> string.
	public static implicit operator string(Identifier id) {
		return id.ToString();
	}
	
	public bool Equals(Identifier other) {
		return hash == other.hash && full == other.full;
	}
}
