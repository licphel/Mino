using Mino.Mathematics.Spatial;

namespace Mino.Mathematics;

/// <summary>
///     A sphere in 3D space defined by center and radius.
/// </summary>
public readonly struct Sphere : IEquatable<Sphere> {
	public static readonly Sphere Empty = new Sphere(
		new Vector3(float.NaN, float.NaN, float.NaN), 0.0F);

	public readonly Vector3 Center;
	public readonly float Radius;

	/// <summary>
	///     Initializes a new sphere.
	/// </summary>
	/// <param name="center">Sphere center.</param>
	/// <param name="radius">Sphere radius (must be >= 0).</param>
	/// <exception cref="Error">Thrown when radius is negative.</exception>
	public Sphere(in Vector3 center, float radius) {
		if (radius < 0) {
			throw new Error("Sphere radius cannot be negative.", nameof(radius));
		}
		Center = center;
		Radius = radius;
	}

	/// <summary>
	///     Gets the diameter of the sphere.
	/// </summary>
	public float Diameter {
		get => Radius * 2.0F;
	}

	/// <summary>
	///     Gets the surface area of the sphere.
	/// </summary>
	public float SurfaceArea {
		get => 4.0F * MathF.PI * Radius * Radius;
	}

	/// <summary>
	///     Gets the volume of the sphere.
	/// </summary>
	public float Volume {
		get => 4.0F / 3.0F * MathF.PI * Radius * Radius * Radius;
	}

	/// <summary>
	///     Gets the bounding box that encloses the sphere.
	/// </summary>
	public Box3 BoundingBox {
		get => Box3.CreateCentral(Center, new Vector3(Diameter, Diameter, Diameter));
	}

	/// <summary>
	///     Checks if the sphere contains a point.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>True if point is inside or on sphere surface.</returns>
	public bool Contains(in Vector3 point) {
		return Vector3.DistanceSquared(Center, point) <= Radius * Radius;
	}

	/// <summary>
	///     Checks if this sphere contains another sphere.
	/// </summary>
	/// <param name="other">The other sphere to test.</param>
	/// <returns>True if other sphere is completely inside this sphere.</returns>
	public bool Contains(in Sphere other) {
		float distance = Vector3.Distance(Center, other.Center);
		return distance + other.Radius <= Radius;
	}

	/// <summary>
	///     Checks if this sphere intersects with another sphere.
	/// </summary>
	/// <param name="other">The other sphere to test.</param>
	/// <returns>True if spheres intersect or touch.</returns>
	public bool Intersects(in Sphere other) {
		float distance2 = Vector3.DistanceSquared(Center, other.Center);
		float radiusSum = Radius + other.Radius;
		return distance2 <= radiusSum * radiusSum;
	}

	/// <summary>
	///     Checks if this sphere intersects with a box.
	/// </summary>
	/// <param name="box">The box to test.</param>
	/// <returns>True if sphere and box intersect.</returns>
	public bool Intersects(in Box3 box) {
		Vector3 closest = Vector3.Clamp(Center, box.Min, box.Max);
		return Vector3.DistanceSquared(Center, closest) <= Radius * Radius;
	}

	/// <summary>
	///     Checks if this sphere intersects with a ray.
	/// </summary>
	/// <param name="ray">The ray to test.</param>
	/// <param name="t">Output intersection distance (closest if two intersections).</param>
	/// <returns>True if ray intersects the sphere.</returns>
	public bool Intersects(in Ray ray, out float t) {
		Vector3 oc = ray.Origin - Center;
		float a = ray.Direction.LengthSquared;
		float b = 2.0F * oc.Dot(ray.Direction);
		float c = oc.LengthSquared - Radius * Radius;

		float discriminant = b * b - 4.0F * a * c;

		if (discriminant < 0) {
			t = float.NaN;
			return false;
		}

		float sqrtDisc = MathF.Sqrt(discriminant);
		float t1 = (-b - sqrtDisc) / (2.0F * a);
		float t2 = (-b + sqrtDisc) / (2.0F * a);

		if (t1 >= 0 && t2 >= 0) {
			t = Math.Min(t1, t2);
			return true;
		}
		if (t1 >= 0) {
			t = t1;
			return true;
		}
		if (t2 >= 0) {
			t = t2;
			return true;
		}

		t = float.NaN;
		return false;
	}

	/// <summary>
	///     Checks if this sphere intersects with a plane.
	/// </summary>
	/// <param name="plane">The plane to test.</param>
	/// <returns>
	///     -1: Sphere is completely on negative side of plane
	///     0: Sphere intersects the plane
	///     1: Sphere is completely on positive side of plane
	/// </returns>
	public int Intersects(in Plane plane) {
		float distance = plane.GetDistanceToPoint(Center);

		if (distance > Radius) {
			return 1;
		}
		if (distance < -Radius) {
			return -1;
		}
		return 0;
	}

	/// <summary>
	///     Gets the distance from a point to sphere surface.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>
	///     Positive if point is outside sphere (distance to surface),
	///     Negative if point is inside sphere (depth inside sphere),
	///     Zero if point is on sphere surface.
	/// </returns>
	public float DistanceTo(in Vector3 point) {
		return Vector3.Distance(Center, point) - Radius;
	}

	/// <summary>
	///     Gets the squared distance from a point to sphere surface.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>The squared distance to sphere surface.</returns>
	public float DistanceSquaredTo(in Vector3 point) {
		float distance = Vector3.Distance(Center, point);
		return (distance - Radius) * (distance - Radius);
	}

	/// <summary>
	///     Gets the closest point on sphere surface to a given point.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>The closest point on sphere surface.</returns>
	public Vector3 GetClosestPoint(in Vector3 point) {
		Vector3 direction = point - Center;

		if (Comparison.DoEqual(0.0F, direction.LengthSquared)) {
			return Center + new Vector3(Radius, 0.0F, 0.0F);
		}

		return Center + direction.Normalize() * Radius;
	}

	/// <summary>
	///     Creates a sphere that encloses a set of points.
	/// </summary>
	/// <param name="points">Collection of points to enclose.</param>
	/// <returns>The smallest sphere containing all points.</returns>
	/// <exception cref="Error">Thrown when points collection is empty.</exception>
	public static Sphere CreateFromPoints(IEnumerable<Vector3> points) {
		IEnumerable<Vector3> flatPs = points as Vector3[] ?? points.ToArray();

		Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

		foreach (Vector3 point in flatPs) {
			min = Vector3.Min(min, point);
			max = Vector3.Max(max, point);
		}
		Vector3 center = (min + max) * 0.5F;

		float maxDistance2 = 0.0F;
		foreach (Vector3 point in flatPs) {
			float distance2 = Vector3.DistanceSquared(center, point);
			if (distance2 > maxDistance2) {
				maxDistance2 = distance2;
			}
		}

		return new Sphere(center, MathF.Sqrt(maxDistance2));
	}

	/// <summary>
	///     Creates a sphere from a bounding box.
	/// </summary>
	/// <param name="box">The bounding box.</param>
	/// <returns>A sphere that encloses the box.</returns>
	public static Sphere CreateFromBox(in Box3 box) {
		Vector3 center = (box.Min + box.Max) * 0.5F;
		float radius = Vector3.Distance(center, box.Max);
		return new Sphere(center, radius);
	}

	/// <summary>
	///     Creates a sphere that is the union of two spheres.
	/// </summary>
	/// <param name="a">First sphere.</param>
	/// <param name="b">Second sphere.</param>
	/// <returns>The smallest sphere containing both spheres.</returns>
	public static Sphere GetUnion(in Sphere a, in Sphere b) {
		Vector3 direction = b.Center - a.Center;
		float distance = direction.Length;

		if (distance + b.Radius <= a.Radius) {
			return a;
		}
		if (distance + a.Radius <= b.Radius) {
			return b;
		}

		float radius = (distance + a.Radius + b.Radius) * 0.5F;
		Vector3 center = a.Center + direction.Normalize() * (radius - a.Radius);

		return new Sphere(center, radius);
	}

	/// <summary>
	///     Creates a sphere that is the intersection of two spheres.
	/// </summary>
	/// <param name="a">First sphere.</param>
	/// <param name="b">Second sphere.</param>
	/// <returns>
	///     The intersection sphere if spheres overlap,
	///     or Empty if they don't intersect.
	/// </returns>
	public static Sphere GetIntersection(in Sphere a, in Sphere b) {
		if (!a.Intersects(b)) {
			return Empty;
		}

		Vector3 direction = b.Center - a.Center;
		float distance = direction.Length;

		if (Comparison.DoEqual(0.0F, distance)) {
			return new Sphere(a.Center, Math.Min(a.Radius, b.Radius));
		}

		float radius = (a.Radius - b.Radius + distance) * 0.5F;
		Vector3 center = a.Center + direction.Normalize() * (a.Radius - radius);

		return new Sphere(center, radius);
	}

	/// <summary>
	///     Creates a sphere that tightly fits around a capsule.
	/// </summary>
	/// <param name="start">Start point of capsule line segment.</param>
	/// <param name="end">End point of capsule line segment.</param>
	/// <param name="radius">Capsule radius.</param>
	/// <returns>A sphere enclosing the capsule.</returns>
	public static Sphere CreateFromCapsule(in Vector3 start, in Vector3 end, float radius) {
		Vector3 center = (start + end) * 0.5F;
		float halfLength = Vector3.Distance(start, end) * 0.5F;
		float sphereRadius = halfLength + radius;

		return new Sphere(center, sphereRadius);
	}

	/// <summary>
	///     Transforms the sphere by a matrix.
	/// </summary>
	/// <param name="matrix">Transformation matrix.</param>
	/// <returns>Transformed sphere (becomes an ellipsoid, approximated as sphere).</returns>
	/// <remarks>
	///     For uniform scaling, returns exact sphere.
	///     For non-uniform scaling, returns bounding sphere of transformed ellipsoid.
	/// </remarks>
	public Sphere Transform(in Matrix4x4 matrix) {
		Vector3 newCenter = matrix.Transform(Center);
		Matrix3x3 linear = matrix.ToMatrix3x3();
		float maxScale = Math.Max(
			Math.Abs(linear.M00), Math.Max(Math.Abs(linear.M11), Math.Abs(linear.M22)));
		float newRadius = Radius * maxScale;

		return new Sphere(newCenter, newRadius);
	}

	/// <summary>
	///     Expands the sphere by a given amount.
	/// </summary>
	/// <param name="amount">Amount to expand radius (can be negative to shrink).</param>
	/// <returns>Expanded sphere.</returns>
	public Sphere Expand(float amount) {
		float newRadius = Math.Max(0, Radius + amount);
		return new Sphere(Center, newRadius);
	}

	/// <summary>
	///     Translates the sphere by a vector.
	/// </summary>
	/// <param name="translation">Translation vector.</param>
	/// <returns>Translated sphere.</returns>
	public Sphere Translate(in Vector3 translation) {
		return new Sphere(Center + translation, Radius);
	}

	/// <summary>
	///     Linearly interpolates between two spheres.
	/// </summary>
	/// <param name="a">Start sphere.</param>
	/// <param name="b">End sphere.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>Interpolated sphere.</returns>
	public static Sphere Lerp(in Sphere a, in Sphere b, float t) {
		t = Math.Clamp(t, 0.0F, 1.0F);
		Vector3 center = Vector3.Lerp(a.Center, b.Center, t);
		float radius = a.Radius + (b.Radius - a.Radius) * t;
		return new Sphere(center, radius);
	}

	/// <summary>
	///     Creates a sphere from four points (circumsphere of tetrahedron).
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <param name="c">Third point.</param>
	/// <param name="d">Fourth point.</param>
	/// <returns>Sphere passing through all four points.</returns>
	public static Sphere CreateFromFourPoints(in Vector3 a, in Vector3 b, in Vector3 c,
		in Vector3 d) {
		Vector3 ba = b - a;
		Vector3 ca = c - a;
		Vector3 da = d - a;

		float ba2 = ba.LengthSquared;
		float ca2 = ca.LengthSquared;
		float da2 = da.LengthSquared;

		float det = 2.0F * (
			ba.X * (ca.Y * da.Z - ca.Z * da.Y) -
			ba.Y * (ca.X * da.Z - ca.Z * da.X) +
			ba.Z * (ca.X * da.Y - ca.Y * da.X)
		);

		if (Comparison.DoEqual(0.0F, det)) {
			return Empty;
		}

		Vector3 crossBc = ca.Cross(da);
		Vector3 crossCa = da.Cross(ba);
		Vector3 crossAb = ba.Cross(ca);

		Vector3 center = a + (
			crossBc * ba2 +
			crossCa * ca2 +
			crossAb * da2
		) / det;

		float radius = Vector3.Distance(center, a);
		return new Sphere(center, radius);
	}

	public bool Equals(Sphere other) {
		return Center.Equals(other.Center) && Comparison.DoEqual(Radius, other.Radius);
	}

	public override bool Equals(object? obj) {
		return obj is Sphere other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Center, Radius);
	}

	public override string ToString() {
		return $"[{Center}: {Radius:F3}]";
	}

	public static bool operator ==(in Sphere left, in Sphere right) {
		return left.Equals(right);
	}

	public static bool operator !=(in Sphere left, in Sphere right) {
		return !left.Equals(right);
	}
}
