#region
using System.Collections.Concurrent;
using Mino.Utility;
#endregion

namespace Mino.Modular;

/// <summary>
///     A realm of a mod.
/// </summary>
public sealed class Realm : IEquatable<Realm> {
	private static readonly ConcurrentDictionary<string, Realm> _realms = new ConcurrentDictionary<string, Realm>();
	/// <summary>
	///     The unknown realms for default fallback.
	/// </summary>
	public static readonly Realm Unknown = new Realm("unknown");

	public readonly string Name;

	public Realm(string name) {
		if (string.IsNullOrWhiteSpace(name)) {
			throw new Crash("Realm name cannot be empty");
		}
		if (!Validate(name)) {
			throw new Crash($"Realm name invalid: '{name}'");
		}

		Name = name;

		_realms.TryAdd(name, this);
	}

	/// <summary>
	///     Gets an identifier in current realm.
	/// </summary>
	/// <param name="str">Path of the identifier.</param>
	/// <returns>An validated identifier.</returns>
	public Identifier Get(string str) {
		return Identifier.Fallback(this, str);
	}

	/// <summary>
	///     Validates a char sequence.
	/// </summary>
	/// <param name="seq">The char sequence.</param>
	/// <returns>True if there's no invalid characters. Otherwise false.</returns>
	public static bool Validate(string seq) {
		foreach (char ch in seq) {
			if (":_/$.".Contains(ch)) {
				continue;
			}
			if (ch >= '0' && ch <= '9') {
				continue;
			}
			if (ch >= 'a' && ch <= 'z') {
				continue;
			}
			if (ch >= 'A' && ch <= 'Z') {
				continue;
			}
			return false;
		}
		return true;
	}

	/// <summary>
	///     Tries to find a realm by name.
	/// </summary>
	/// <param name="name">Realm name.</param>
	/// <returns>Nullable realm result.</returns>
	public static Realm TryFind(string name) {
		return _realms.GetValueOrDefault(name, Unknown);
	}

	public bool Equals(Realm? other) {
		if (other is null) {
			return false;
		}
		if (ReferenceEquals(this, other)) {
			return true;
		}
		return Name == other.Name;
	}

	public override bool Equals(object? obj) {
		return ReferenceEquals(this, obj) || obj is Realm other && Equals(other);
	}

	public override int GetHashCode() {
		return Name.GetHashCode();
	}

	public override string ToString() {
		return Name;
	}

	public static bool operator ==(Realm? left, Realm? right) {
		return Equals(left, right);
	}

	public static bool operator !=(Realm? left, Realm? right) {
		return !Equals(left, right);
	}
}
