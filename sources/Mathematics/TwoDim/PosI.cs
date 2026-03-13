namespace Mino.Mathematics.TwoDim;

/// <summary>
///		Integer position.
/// </summary>
public readonly struct PosI : IEquatable<PosI> {
	public readonly int X = 0;
	public readonly int Y = 0;

	public PosI() {
	}
	
	public PosI(int x, int y) {
		X = x;
		Y = y;
	}

	public PosI(in Vector2 vec) {
		X = (int) MathF.Floor(vec.X);
		Y = (int) MathF.Floor(vec.Y);
	}
	
	public PosI(in Pos pos) {
		X = (int) Math.Floor(pos.X);
		Y = (int) Math.Floor(pos.Y);
	}
	
	/// <summary>
	///     Computes the squared distance between two poses.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The distance between the poses.</returns>
	public static double DistanceSquared(in PosI a, in PosI b) {
		double dx = a.X - b.X;
		double dy = a.Y - b.Y;
		return dx * dx + dy * dy;
	}
	
	/// <summary>
	///     Computes the distance between two poses.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The distance between the poses.</returns>
	public static double Distance(in PosI a, in PosI b) {
		return Math.Sqrt(DistanceSquared(a, b));
	}

	public bool Equals(PosI other) {
		return X == other.X && Y == other.Y;
	}
	
	public override bool Equals(object? obj) {
		return obj is PosI other && Equals(other);
	}
	
	public override int GetHashCode() {
		return HashCode.Combine(X, Y);
	}

	public override string ToString() {
		return $"({X}, {Y})";
	}

	public static implicit operator PosI(in Pos pos) {
		return new PosI(pos);
	}
	
	public static implicit operator Vector2(in PosI pos) {
		return new Vector2(pos.X, pos.Y);
	}
	
	public static PosI operator +(in PosI a, in PosI b) {
		return new PosI(a.X + b.X, a.Y + b.Y);
	}
	
	public static PosI operator -(in PosI a, in PosI b) {
		return new PosI(a.X - b.X, a.Y - b.Y);
	}
	
	public static bool operator ==(PosI left, PosI right) {
		return left.Equals(right);
	}
	
	public static bool operator !=(PosI left, PosI right) {
		return !left.Equals(right);
	}
}
