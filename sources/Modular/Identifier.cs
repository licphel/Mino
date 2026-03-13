using Mino.Nio;
using Mino.Utility;

namespace Mino.Modular;

/// <summary>
///		Key identifier.
/// </summary>
public readonly struct Identifier : IEquatable<Identifier> {
	/// <summary>
	///		Identifier scope root.
	/// </summary>
	public class ScopeRoot {
		public readonly string Scope;
		
		public ScopeRoot(string scope) {
			Scope = scope;
		}

		public Identifier Of(string key) {
			return new Identifier(Scope, key);
		}

		public static implicit operator string(ScopeRoot root) {
			return root.Scope;
		}
	}
	
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
			Scope = string.Empty;
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

	/// <summary>
	///		Converts to a url.
	/// </summary>
	/// <returns>A url of scope - mod id, key - resource finder.</returns>
	/// <exception cref="Crash">Thrown if there's no matching mod.</exception>
	public Url ToUrl() {
		Mod? mod = Mod.Mods!.GetValueOrDefault(Scope, null);
		if (mod == null) {
			throw new Crash($"Scope '{Scope}' is not a mod id");
		}
		
		return mod.Directory / Key;
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

	// string -> Id.
	public static implicit operator Identifier(string s) {
		return new Identifier(s);
	}

	// Id -> string.
	public static implicit operator string(Identifier idt) {
		return idt.ToString();
	}

	// Id -> Url.
	public static implicit operator Url(in Identifier id) {
		return id.ToUrl();
	}
	
	public bool Equals(Identifier other) {
		return hash == other.hash && full == other.full;
	}
}
