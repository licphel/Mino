namespace Mino.Mathematics.TwoDim;

/// <summary>
///		Double position.
/// </summary>
public readonly struct Pos : IEquatable<Pos> {
	public readonly double X = 0;
	public readonly double Y = 0;

	public Pos() {
	}
	
	public Pos(double x, double y) {
		X = x;
		Y = y;
	}

	public Pos(in Vector2 vec) {
		X = vec.X;
		Y = vec.Y;
	}
	
	public Pos(in PosI pos) {
		X = pos.X;
		Y = pos.Y;
	}

	/// <summary>
	///     Computes the squared distance between two poses.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The distance between the poses.</returns>
	public static double DistanceSquared(in Pos a, in Pos b) {
		double dx = a.X - b.X;
		double dy = a.Y - b.Y;
		return dx * dx + dy * dy;
	}
	
	/// <summary>
	///     Computes the squared distance between two poses.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The squared distance between the poses.</returns>
	public static double Distance(in Pos a, in Pos b) {
		return Math.Sqrt(DistanceSquared(a, b));
	}
	
	public bool Equals(Pos other) {
		return Comparison.DoEqual(X, other.X) && Comparison.DoEqual(Y, other.Y);
	}
	
	public override bool Equals(object? obj) {
		return obj is Pos other && Equals(other);
	}
	
	public override int GetHashCode() {
		return HashCode.Combine(X, Y);
	}
	
	public override string ToString() {
		return $"({X:3F}, {Y:3F})";
	}

	public static implicit operator Pos(in PosI pos) {
		return new Pos(pos);
	}
	
	public static implicit operator Vector2(in Pos pos) {
		return new Vector2((float) pos.X, (float) pos.Y);
	}
	
	public static Pos operator +(in Pos a, in Pos b) {
		return new Pos(a.X + b.X, a.Y + b.Y);
	}
	
	public static Pos operator -(in Pos a, in Pos b) {
		return new Pos(a.X - b.X, a.Y - b.Y);
	}
	
	public static bool operator ==(Pos left, Pos right) {
		return left.Equals(right);
	}
	
	public static bool operator !=(Pos left, Pos right) {
		return !left.Equals(right);
	}
}
