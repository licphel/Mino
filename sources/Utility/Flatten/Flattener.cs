namespace Mino.Utility.Flatten;

/// <summary>
///		A KV-structure flattener for efficient storage.
/// </summary>
public readonly struct Flattener : IEquatable<Flattener> {
	public readonly ulong Compressed;
	
	public Flattener(ulong compressed) {
		Compressed = compressed;
	}
	
	public bool Equals(Flattener other) {
		return Compressed == other.Compressed;
	}
	
	public override bool Equals(object? obj) {
		return obj is Flattener other && Equals(other);
	}
	
	public override int GetHashCode() {
		return Compressed.GetHashCode();
	}
	
	public static bool operator ==(Flattener left, Flattener right) {
		return left.Equals(right);
	}
	
	public static bool operator !=(Flattener left, Flattener right) {
		return !left.Equals(right);
	}
}
