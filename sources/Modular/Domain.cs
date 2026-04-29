#region
using System.Collections.Concurrent;
using System.Reflection;
#endregion

namespace Mino.Modular;

/// <summary>
///     A domain of a mod.
/// </summary>
public sealed class Domain : IEquatable<Domain> {
	private static readonly ConcurrentDictionary<string, Domain> _domains = new ConcurrentDictionary<string, Domain>();
	/// <summary>
	///     The unknown domains for default fallback.
	/// </summary>
	public static readonly Domain Unknown = new Domain("unknown");

	public readonly string Name;

	internal Domain(string name) {
		if (string.IsNullOrWhiteSpace(name)) {
			throw new RMLException("Domain name cannot be empty");
		}
		if (!Validate(name)) {
			throw new RMLException($"Domain name invalid: '{name}'");
		}

		Name = name;

		_domains.TryAdd(name, this);
	}

	/// <summary>
	///     Gets an identifier in current domain.
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
			if (":_/$.-".Contains(ch)) {
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
	///     Tries to find a domain by name.
	/// </summary>
	/// <param name="name">Domain name.</param>
	/// <returns>Nullable domain result.</returns>
	public static Domain TryFind(string name) {
		return _domains.GetValueOrDefault(name, Unknown);
	}

	/// <summary>
	///		Gets the domain of the current
	/// </summary>
	/// <param name="throwIfNotFound">Whether to throw an exception when not found.</param>
	/// <returns></returns>
	public static Domain GetCurrent(bool throwIfNotFound = false) {
		if (Mod.ModsByAsm.TryGetValue(Assembly.GetCallingAssembly(), out Mod? mod)) {
			return mod.Domain;
		}
		if (throwIfNotFound) {
			throw new RMLException("Domain is not initialized");
		}
		return Unknown;
	}

	public bool Equals(Domain? other) {
		if (other is null) {
			return false;
		}
		if (ReferenceEquals(this, other)) {
			return true;
		}
		return Name == other.Name;
	}

	public override bool Equals(object? obj) {
		return ReferenceEquals(this, obj) || obj is Domain other && Equals(other);
	}

	public override int GetHashCode() {
		return Name.GetHashCode();
	}

	public override string ToString() {
		return Name;
	}

	public static bool operator ==(Domain? left, Domain? right) {
		return Equals(left, right);
	}

	public static bool operator !=(Domain? left, Domain? right) {
		return !Equals(left, right);
	}
}
