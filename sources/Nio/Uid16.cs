#region
using Mino.Mathematics.Random;
#endregion

namespace Mino.Nio;

/// <summary>
///     16-bytes unique identifier prioritizing efficiency.
/// </summary>
public readonly struct Uid16 : IEquatable<Uid16>, IComparable<Uid16> {
	public static readonly Uid16 Empty = new Uid16(0L, 0L);

	/// <summary>
	///     The lower 8 bytes.
	/// </summary>
	public readonly long PartA;
	/// <summary>
	///     The higher 8 bytes.
	/// </summary>
	public readonly long PartB;
	private readonly int _hashCache;

	public Uid16(long partA, long partB) {
		PartA = partA;
		PartB = partB;
		_hashCache = HashCode.Combine(partA, partB);
	}

	/// <summary>
	///     Generates a random unique id by a random generator.
	/// </summary>
	/// <param name="randomGenerator">Optional generator. Keep null to use default generator.</param>
	/// <returns>A random unique id.</returns>
	public static Uid16 Create(RandomGenerator? randomGenerator = null) {
		RandomGenerator rng = randomGenerator ?? RandomGenerator.Default;
		int high1 = rng.NextInt();
		int low1 = rng.NextInt();
		int high2 = rng.NextInt();
		int low2 = rng.NextInt();
		long part1 = (long) high1 << 32 | (uint) low1;
		long part2 = (long) high2 << 32 | (uint) low2;
		return new Uid16(part1, part2);
	}

	/// <summary>
	///     Get a byte array form of the unique id.
	/// </summary>
	/// <returns>A byte array with a length of 16.</returns>
	public byte[] ToByteArray() {
		byte[] bytes = new byte[16];
		Buffer.BlockCopy(BitConverter.GetBytes(PartA), 0, bytes, 0, 8);
		Buffer.BlockCopy(BitConverter.GetBytes(PartB), 0, bytes, 8, 8);
		return bytes;
	}

	public override string ToString() {
		byte[] bytes = ToByteArray();
		return Convert.ToHexStringLower(bytes);
	}

	public bool Equals(Uid16 other) {
		return PartA == other.PartA && PartB == other.PartB;
	}

	public override bool Equals(object? obj) {
		return obj is Uid16 other && Equals(other);
	}

	public override int GetHashCode() {
		return _hashCache;
	}

	public int CompareTo(Uid16 other) {
		if (PartA != other.PartA) {
			return PartA.CompareTo(other.PartA);
		}
		return PartB.CompareTo(other.PartB);
	}

	public static bool operator ==(Uid16 left, Uid16 right) {
		return left.Equals(right);
	}

	public static bool operator !=(Uid16 left, Uid16 right) {
		return !left.Equals(right);
	}
}
