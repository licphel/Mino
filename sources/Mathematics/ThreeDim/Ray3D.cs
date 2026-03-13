using Mino.Utility;

namespace Mino.Mathematics.ThreeDim;

/// <summary>
///     Immutable ray 3D.
/// </summary>
public readonly struct Ray3D {
	public readonly Vector3 Origin;
	public readonly Vector3 Direction;

	/// <summary>
	///     Initializes a new ray.
	/// </summary>
	/// <param name="origin">Ray origin point.</param>
	/// <param name="direction">Ray direction (will be normalized).</param>
	/// <exception cref="Crash">Thrown when direction is zero vector.</exception>
	public Ray3D(in Vector3 origin, in Vector3 direction) {
		Origin = origin;
		if (Comparison.DoEqual(0.0F, direction.LengthSquared)) {
			throw new Crash("Zero directional vector");
		}
		Direction = direction.Normalize();
	}

	/// <summary>
	///     Gets a point along the ray at distance t.
	/// </summary>
	/// <param name="t">Distance from origin (should be >= 0).</param>
	/// <returns>Point at distance t along the ray.</returns>
	public Vector3 GetPoint(float t) {
		return Origin + Direction * t;
	}

	/// <summary>
	///     Gets the distance from a point to this ray.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>The shortest distance from point to ray.</returns>
	public float DistanceTo(in Vector3 point) {
		return MathF.Sqrt(DistanceSquaredTo(point));
	}

	/// <summary>
	///     Gets the squared distance from a point to this ray.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>The squared shortest distance from point to ray.</returns>
	public float DistanceSquaredTo(in Vector3 point) {
		Vector3 originToPoint = point - Origin;
		float projectionLength = originToPoint.Dot(Direction);

		if (projectionLength < 0) {
			return originToPoint.LengthSquared;
		}

		Vector3 projectedPoint = Origin + Direction * projectionLength;
		return Vector3.DistanceSquared(point, projectedPoint);
	}

	/// <summary>
	///     Finds the closest point on this ray to a given point.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>The closest point on the ray.</returns>
	public Vector3 GetClosestPoint(in Vector3 point) {
		Vector3 originToPoint = point - Origin;
		float projectionLength = Math.Max(originToPoint.Dot(Direction), 0.0F);
		return Origin + Direction * projectionLength;
	}

	/// <summary>
	///     Checks if this ray intersects with a sphere.
	/// </summary>
	/// <param name="center">Sphere center.</param>
	/// <param name="radius">Sphere radius.</param>
	/// <param name="t">Output intersection distance (closest if two intersections).</param>
	/// <returns>True if ray intersects the sphere.</returns>
	public bool IntersectsSphere(in Vector3 center, float radius, out float t) {
		Vector3 oc = Origin - center;
		float a = Direction.LengthSquared;
		float b = 2.0F * oc.Dot(Direction);
		float c = oc.LengthSquared - radius * radius;

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
	///     Checks if this ray intersects with an axis-aligned bounding box.
	/// </summary>
	/// <param name="box">The bounding box to test.</param>
	/// <param name="tMin">Output minimum intersection distance.</param>
	/// <param name="tMax">Output maximum intersection distance.</param>
	/// <returns>True if ray intersects the box.</returns>
	public bool IntersectsBox(in Box3 box, out float tMin, out float tMax) {
		tMin = 0.0F;
		tMax = float.MaxValue;

		for (int i = 0; i < 3; i++) {
			float invD = 1.0F / Direction[i];
			float t0 = (box.Min[i] - Origin[i]) * invD;
			float t1 = (box.Max[i] - Origin[i]) * invD;

			if (invD < 0.0F) {
				(t0, t1) = (t1, t0);
			}

			tMin = Math.Max(tMin, t0);
			tMax = Math.Min(tMax, t1);

			if (tMax <= tMin) {
				return false;
			}
		}

		return tMin >= 0 || tMax >= 0;
	}

	/// <summary>
	///     Checks if this ray intersects with a triangle.
	/// </summary>
	/// <param name="v0">First triangle vertex.</param>
	/// <param name="v1">Second triangle vertex.</param>
	/// <param name="v2">Third triangle vertex.</param>
	/// <param name="t">Output intersection distance.</param>
	/// <param name="u">Output barycentric coordinate u.</param>
	/// <param name="v">Output barycentric coordinate v.</param>
	/// <param name="backfaceCulling">If true, ignores intersections from back side.</param>
	/// <returns>True if ray intersects the triangle.</returns>
	public bool IntersectsTriangle(in Vector3 v0, in Vector3 v1, in Vector3 v2,
		out float t, out float u, out float v, bool backfaceCulling = true) {
		t = u = v = 0.0F;

		Vector3 edge1 = v1 - v0;
		Vector3 edge2 = v2 - v0;
		Vector3 h = Direction.Cross(edge2);

		float a = edge1.Dot(h);

		if (backfaceCulling && Comparison.DoEqual(0.0F, a)) {
			return false;
		}

		if (Comparison.DoEqual(0.0F, a)) {
			return false;
		}

		float f = 1.0F / a;
		Vector3 s = Origin - v0;
		u = f * s.Dot(h);

		if (u is < 0.0F or > 1.0F) {
			return false;
		}

		Vector3 q = s.Cross(edge1);
		v = f * Direction.Dot(q);
		if (v < 0.0F || u + v > 1.0F) {
			return false;
		}

		t = f * edge2.Dot(q);
		return t >= 0.0F;
	}

	/// <summary>
	///     Transforms this ray by a matrix.
	/// </summary>
	/// <param name="matrix">Transformation matrix.</param>
	/// <returns>Transformed ray.</returns>
	/// <remarks>
	///     Direction is transformed without translation, then re-normalized.
	/// </remarks>
	public Ray3D Transform(in Matrix4x4 matrix) {
		Vector3 newOrigin = matrix.Transform(Origin);
		Vector3 newDirection = matrix.ToMatrix3x3().Transform(Direction).Normalize();
		return new Ray3D(newOrigin, newDirection);
	}

	/// <summary>
	///     Creates a ray from two points.
	/// </summary>
	/// <param name="from">Starting point.</param>
	/// <param name="to">Ending point.</param>
	/// <returns>A ray from 'from' pointing toward 'to'.</returns>
	public static Ray3D CreateFromPoints(in Vector3 from, in Vector3 to) {
		return new Ray3D(from, to - from);
	}

	/// <summary>
	///     Creates a ray from screen coordinates (for picking).
	/// </summary>
	/// <param name="screenX">Screen X coordinate (pixels).</param>
	/// <param name="screenY">Screen Y coordinate (pixels).</param>
	/// <param name="screenWidth">Screen width (pixels).</param>
	/// <param name="screenHeight">Screen height (pixels).</param>
	/// <param name="projectionMatrix">Projection matrix.</param>
	/// <param name="viewMatrix">View matrix.</param>
	/// <returns>World space ray for picking.</returns>
	public static Ray3D CreateFromScreen(float screenX, float screenY, float screenWidth,
		float screenHeight,
		in Matrix4x4 projectionMatrix, in Matrix4x4 viewMatrix) {
		float ndcX = 2.0F * screenX / screenWidth - 1.0F;
		float ndcY = 1.0F - 2.0F * screenY / screenHeight;
		Vector3 nearPoint = new Vector3(ndcX, ndcY, -1.0F);
		Vector3 farPoint = new Vector3(ndcX, ndcY, 1.0F);

		// Transform to world space.
		Matrix4x4 invViewProj = (projectionMatrix * viewMatrix).Invert();
		Vector3 worldNear = invViewProj.Transform(nearPoint);
		Vector3 worldFar = invViewProj.Transform(farPoint);

		return CreateFromPoints(worldNear, worldFar);
	}

	/// <summary>
	///     Reflects this ray off a surface with given normal.
	/// </summary>
	/// <param name="normal">Surface normal (must be normalized).</param>
	/// <param name="point">Reflection point on surface.</param>
	/// <returns>Reflected ray.</returns>
	public Ray3D Reflect(in Vector3 normal, in Vector3 point) {
		Vector3 reflectedDir = Direction.Reflect(normal);
		return new Ray3D(point, reflectedDir);
	}

	/// <summary>
	///     Refracts this ray through a surface.
	/// </summary>
	/// <param name="normal">Surface normal (must be normalized).</param>
	/// <param name="point">Refraction point.</param>
	/// <param name="ior">Index of refraction (n2/n1).</param>
	/// <returns>Refracted ray, or null if total internal reflection.</returns>
	public Ray3D? Refract(in Vector3 normal, in Vector3 point, float ior) {
		float cosI = -normal.Dot(Direction);
		float sinT2 = ior * ior * (1.0F - cosI * cosI);

		if (sinT2 > 1.0F) {
			return null; // Total internal reflection
		}

		float cosT = MathF.Sqrt(1.0F - sinT2);
		Vector3 refractedDir = ior * Direction + (ior * cosI - cosT) * normal;

		return new Ray3D(point, refractedDir);
	}

	/// <summary>
	///     Linearly interpolates between two rays.
	/// </summary>
	/// <param name="a">Start ray.</param>
	/// <param name="b">End ray.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>Interpolated ray.</returns>
	public static Ray3D Lerp(in Ray3D a, in Ray3D b, float t) {
		Vector3 origin = Vector3.Lerp(a.Origin, b.Origin, t);
		Vector3 direction = Vector3.Lerp(a.Direction, b.Direction, t).Normalize();
		return new Ray3D(origin, direction);
	}

	public override string ToString() {
		return $"{Origin}: {Direction}";
	}
}
