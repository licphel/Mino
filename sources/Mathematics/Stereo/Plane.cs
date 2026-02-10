namespace Mino.Mathematics.Stereo;

/// <summary>
///     A plane in 3D space defined by normal and distance from origin.
/// </summary>
public readonly struct Plane : IEquatable<Plane> {
	/// <summary>
	///     Plane normal (unit vector).
	/// </summary>
	public readonly Vector3 Normal;

	/// <summary>
	///     Distance from origin along normal (signed).
	/// </summary>
	public readonly float Distance;

	/// <summary>
	///     Initializes a new plane from normal and distance.
	/// </summary>
	/// <param name="normal">Plane normal (will be normalized).</param>
	/// <param name="distance">Distance from origin.</param>
	public Plane(in Vector3 normal, float distance) {
		Normal = normal.Normalize();
		Distance = distance;
	}

	/// <summary>
	///     Initializes a new plane from normal and point.
	/// </summary>
	/// <param name="normal">Plane normal (will be normalized).</param>
	/// <param name="point">Point on the plane.</param>
	public Plane(in Vector3 normal, in Vector3 point) {
		Normal = normal.Normalize();
		Distance = -Normal.Dot(point);
	}

	/// <summary>
	///     Gets the signed distance from point to plane.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>Positive if point is in front of plane, negative if behind.</returns>
	public float GetDistanceToPoint(in Vector3 point) {
		return Normal.Dot(point) + Distance;
	}

	/// <summary>
	///     Projects a point onto the plane.
	/// </summary>
	/// <param name="point">The point to project.</param>
	/// <returns>The projected point on the plane.</returns>
	public Vector3 ProjectPoint(in Vector3 point) {
		float dist = GetDistanceToPoint(point);
		return point - Normal * dist;
	}

	/// <summary>
	///     Normalizes the plane (ensures normal is unit length).
	/// </summary>
	/// <returns>Normalized plane.</returns>
	public Plane Normalize() {
		float length = Normal.Length;
		if (Comparison.DoEqual(0.0F, length)) {
			return this;
		}
		return new Plane(Normal / length, Distance / length);
	}

	/// <summary>
	///     Transforms the plane by a matrix.
	/// </summary>
	/// <param name="matrix">Transformation matrix.</param>
	/// <returns>Transformed plane.</returns>
	public Plane Transform(in Matrix4x4 matrix) {
		Matrix4x4 invTranspose = matrix.Invert().Transpose();
		Vector4 planeEq = new Vector4(Normal, Distance);
		Vector4 transformed = invTranspose * planeEq;
		return new Plane(new Vector3(transformed.X, transformed.Y, transformed.Z), transformed.W);
	}

	/// <summary>
	///     Creates a plane from three points.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <param name="c">Third point.</param>
	/// <returns>A plane containing all three points.</returns>
	public static Plane CreateFromPoints(in Vector3 a, in Vector3 b, in Vector3 c) {
		Vector3 normal = (b - a).Cross(c - a).Normalize();
		return new Plane(normal, a);
	}

	/// <summary>
	///     Checks if this plane is similar to another plane within tolerance.
	/// </summary>
	/// <param name="other">Other plane to compare.</param>
	/// <param name="positionTolerance">Position tolerance.</param>
	/// <param name="normalTolerance">Normal tolerance (dot product).</param>
	/// <returns>True if planes are similar.</returns>
	public bool IsSimilarTo(in Plane other, float positionTolerance = 1E-4F,
		float normalTolerance = 1E-4F) {
		if (Math.Abs(Normal.Dot(other.Normal)) < 1.0F - normalTolerance) {
			return false;
		}
		return Math.Abs(Distance - other.Distance) < positionTolerance;
	}

	public bool Equals(Plane other) {
		return Normal.Equals(other.Normal) && Comparison.DoEqual(Distance, other.Distance);
	}

	public override bool Equals(object? obj) {
		return obj is Plane other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Normal, Distance);
	}

	public override string ToString() {
		return $"[{Normal}: {Distance:F3}]";
	}

	public static bool operator ==(in Plane left, in Plane right) {
		return left.Equals(right);
	}

	public static bool operator !=(in Plane left, in Plane right) {
		return !left.Equals(right);
	}
}
