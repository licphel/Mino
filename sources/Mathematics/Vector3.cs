#region
using System.Runtime.CompilerServices;
using Mino.Mathematics.ThreeDim;
#endregion

namespace Mino.Mathematics;

/// <summary>
///     Immutable vector 3D.
/// </summary>
public readonly struct Vector3 : IEquatable<Vector3> {
	public static readonly Vector3 Zero = new Vector3(0.0F, 0.0F, 0.0F);
	public static readonly Vector3 UnitX = new Vector3(1.0F, 0.0F, 0.0F);
	public static readonly Vector3 UnitY = new Vector3(0.0F, 1.0F, 0.0F);
	public static readonly Vector3 UnitZ = new Vector3(0.0F, 0.0F, 1.0F);

	public readonly float X = 0.0F;
	public readonly float Y = 0.0F;
	public readonly float Z = 0.0F;

	public Vector3() {
	}

	public Vector3(float x, float y, float z) {
		X = x;
		Y = y;
		Z = z;
	}

	/// <summary>
	///     Gets the vector component at the specified index (0=X, 1=Y, 2=Z).
	/// </summary>
	/// <param name="index">Index of the component.</param>
	/// <returns>The component value.</returns>
	public float this[int index] {
		get => Unsafe.Add(ref Unsafe.As<Vector3, float>(ref Unsafe.AsRef(in this)), index);
	}

	/// <summary>
	///     Gets the length (magnitude) of the vector.
	/// </summary>
	public float Length {
		get => MathF.Sqrt(LengthSquared);
	}

	/// <summary>
	///     Gets the squared length of the vector.
	/// </summary>
	public float LengthSquared {
		get => X * X + Y * Y + Z * Z;
	}

	/// <summary>
	///     Adds another vector to this vector.
	/// </summary>
	/// <param name="other">The vector to add.</param>
	/// <returns>The sum vector.</returns>
	public Vector3 Add(in Vector3 other) {
		return new Vector3(X + other.X, Y + other.Y, Z + other.Z);
	}

	/// <summary>
	///     Subtracts another vector from this vector.
	/// </summary>
	/// <param name="other">The vector to subtract.</param>
	/// <returns>The difference vector.</returns>
	public Vector3 Subtract(in Vector3 other) {
		return new Vector3(X - other.X, Y - other.Y, Z - other.Z);
	}

	/// <summary>
	///     Negates this vector.
	/// </summary>
	/// <returns>The negated vector.</returns>
	public Vector3 Negate() {
		return new Vector3(-X, -Y, -Z);
	}

	/// <summary>
	///     Multiplies this vector by a scalar.
	/// </summary>
	/// <param name="scalar">The scalar to multiply by.</param>
	/// <returns>The scaled vector.</returns>
	public Vector3 Multiply(float scalar) {
		return new Vector3(X * scalar, Y * scalar, Z * scalar);
	}

	/// <summary>
	///     Divides this vector by a scalar.
	/// </summary>
	/// <param name="scalar">The scalar to divide by.</param>
	/// <returns>The scaled vector.</returns>
	public Vector3 Divide(float scalar) {
		return new Vector3(X / scalar, Y / scalar, Z / scalar);
	}

	/// <summary>
	///     Scales this vector component-wise by another vector.
	/// </summary>
	/// <param name="other">The scaling vector.</param>
	/// <returns>The scaled vector.</returns>
	public Vector3 Scale(in Vector3 other) {
		return new Vector3(X * other.X, Y * other.Y, Z * other.Z);
	}

	/// <summary>
	///     Computes the cross product with another vector.
	/// </summary>
	/// <param name="v">The other vector.</param>
	/// <returns>The cross product vector.</returns>
	public Vector3 Cross(in Vector3 v) {
		return new Vector3(Y * v.Z - Z * v.Y, Z * v.X - X * v.Z, X * v.Y - Y * v.X);
	}

	/// <summary>
	///     Computes the dot product with another vector.
	/// </summary>
	/// <param name="v">The other vector.</param>
	/// <returns>The dot product.</returns>
	public float Dot(in Vector3 v) {
		return X * v.X + Y * v.Y + Z * v.Z;
	}

	/// <summary>
	///     Normalizes this vector to unit length.
	/// </summary>
	/// <returns>The normalized vector, or zero vector if length is zero.</returns>
	public Vector3 Normalize() {
		float length = Length;
		if (Comparison.DoEqual(0.0F, length)) {
			return Zero;
		}
		return this / length;
	}

	/// <summary>
	///     Reflects this vector across a normal.
	/// </summary>
	/// <param name="normal">The reflection normal (must be normalized).</param>
	/// <returns>The reflected vector.</returns>
	public Vector3 Reflect(in Vector3 normal) {
		return this - 2.0F * Dot(normal) * normal;
	}

	/// <summary>
	///     Projects this vector onto another vector.
	/// </summary>
	/// <param name="onNormal">The vector to project onto.</param>
	/// <returns>The projection vector, or zero if onNormal is zero.</returns>
	public Vector3 Project(in Vector3 onNormal) {
		float lengthSq = onNormal.LengthSquared;
		if (Comparison.DoEqual(0.0F, lengthSq)) {
			return Zero;
		}
		return onNormal * (Dot(onNormal) / lengthSq);
	}

	/// <summary>
	///     Rejects this vector from another vector (perpendicular component).
	/// </summary>
	/// <param name="onNormal">The vector to reject from.</param>
	/// <returns>The rejection vector.</returns>
	public Vector3 Reject(in Vector3 onNormal) {
		return this - Project(onNormal);
	}

	/// <summary>
	///     Projects this vector onto a plane defined by its normal.
	/// </summary>
	/// <param name="planeNormal">The plane normal (must be normalized).</param>
	/// <returns>The projected vector onto the plane.</returns>
	public Vector3 ProjectOnPlane(in Vector3 planeNormal) {
		return this - Project(planeNormal);
	}

	/// <summary>
	///     Computes the distance between two points.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The distance between the points.</returns>
	public static float Distance(in Vector3 a, in Vector3 b) {
		return MathF.Sqrt(DistanceSquared(a, b));
	}

	/// <summary>
	///     Computes the squared distance between two points.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The squared distance between the points.</returns>
	public static float DistanceSquared(in Vector3 a, in Vector3 b) {
		float dx = a.X - b.X;
		float dy = a.Y - b.Y;
		float dz = a.Z - b.Z;
		return dx * dx + dy * dy + dz * dz;
	}

	/// <summary>
	///     Computes the Manhattan distance between two points.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The Manhattan distance.</returns>
	public static float ManhattanDistance(in Vector3 a, in Vector3 b) {
		return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z);
	}

	/// <summary>
	///     Computes the angle between two vectors.
	/// </summary>
	/// <param name="a">First vector.</param>
	/// <param name="b">Second vector.</param>
	/// <returns>The angle in radians.</returns>
	public static float GetAngle(in Vector3 a, in Vector3 b) {
		float denominator = MathF.Sqrt(a.LengthSquared * b.LengthSquared);
		if (Comparison.DoEqual(0.0F, denominator)) {
			return 0.0F;
		}
		float clamped = Math.Clamp(a.Dot(b) / denominator, -1.0F, 1.0F);
		return MathF.Acos(clamped);
	}

	/// <summary>
	///     Rotates this vector by a quaternion.
	/// </summary>
	/// <param name="rotation">The rotation quaternion.</param>
	/// <returns>The rotated vector.</returns>
	public Vector3 Rotate(in Quaternion rotation) {
		return rotation.Rotate(this);
	}

	/// <summary>
	///     Rotates this vector around an axis.
	/// </summary>
	/// <param name="axis">The axis of rotation.</param>
	/// <param name="angle">Rotation angle in radians.</param>
	/// <returns>The rotated vector.</returns>
	public Vector3 Rotate(in Vector3 axis, float angle) {
		return new Quaternion(axis, angle).Rotate(this);
	}

	/// <summary>
	///     Rotates this vector around a center point by a quaternion.
	/// </summary>
	/// <param name="center">The center of rotation.</param>
	/// <param name="rotation">The rotation quaternion.</param>
	/// <returns>The rotated vector.</returns>
	public Vector3 Rotate(in Vector3 center, in Quaternion rotation) {
		Vector3 offset = this - center;
		Vector3 rotated = rotation.Rotate(offset);
		return center + rotated;
	}

	/// <summary>
	///     Rotates this vector around a center point by an axis-angle rotation.
	/// </summary>
	/// <param name="center">The center of rotation.</param>
	/// <param name="axis">The axis of rotation.</param>
	/// <param name="angle">Rotation angle in radians.</param>
	/// <returns>The rotated vector.</returns>
	public Vector3 Rotate(in Vector3 center, in Vector3 axis, float angle) {
		return Rotate(center, new Quaternion(axis, angle));
	}

	/// <summary>
	///     Returns a vector with the minimum components of two vectors.
	/// </summary>
	/// <param name="a">First vector.</param>
	/// <param name="b">Second vector.</param>
	/// <returns>The component-wise minimum vector.</returns>
	public static Vector3 Min(in Vector3 a, in Vector3 b) {
		return new Vector3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
	}

	/// <summary>
	///     Returns a vector with the maximum components of two vectors.
	/// </summary>
	/// <param name="a">First vector.</param>
	/// <param name="b">Second vector.</param>
	/// <returns>The component-wise maximum vector.</returns>
	public static Vector3 Max(in Vector3 a, in Vector3 b) {
		return new Vector3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
	}

	/// <summary>
	///     Clamps a vector's components between min and max vectors.
	/// </summary>
	/// <param name="vector3">The vector to clamp.</param>
	/// <param name="min">Minimum vector.</param>
	/// <param name="max">Maximum vector.</param>
	/// <returns>The clamped vector.</returns>
	public static Vector3 Clamp(in Vector3 vector3, in Vector3 min, in Vector3 max) {
		return new Vector3(
			Math.Clamp(vector3.X, min.X, max.X), Math.Clamp(vector3.Y, min.Y, max.Y),
			Math.Clamp(vector3.Z, min.Z, max.Z));
	}

	/// <summary>
	///     Linearly interpolates between two vectors.
	/// </summary>
	/// <param name="a">Start vector.</param>
	/// <param name="b">End vector.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>The interpolated vector.</returns>
	public static Vector3 Lerp(in Vector3 a, in Vector3 b, float t) {
		t = Math.Clamp(t, 0.0F, 1.0F);
		return new Vector3(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
	}

	/// <summary>
	///     Creates a directional vector from a quaternion and length.
	/// </summary>
	/// <param name="q">The rotation quaternion.</param>
	/// <param name="length">Length of the vector (default 1.0).</param>
	/// <returns>A directional vector.</returns>
	public static Vector3 CreateDirectional(in Quaternion q, float length = 1.0F) {
		float x2 = q.X * 2.0F;
		float y2 = q.Y * 2.0F;
		float z2 = q.Z * 2.0F;
		float xx2 = q.X * x2;
		float xz2 = q.X * z2;
		float yy2 = q.Y * y2;
		float yz2 = q.Y * z2;
		float wx2 = q.W * x2;
		float wy2 = q.W * y2;
		return new Vector3(xz2 + wy2, yz2 - wx2, 1.0F - (xx2 + yy2)) * length;
	}

	// Implicit cast Vector2 -> Vector3.
	public static implicit operator Vector3(in Vector2 vec2) {
		return new Vector3(vec2.X, vec2.Y, 0.0F);
	}

	// Implicit cast Vector4 -> Vector3.
	public static implicit operator Vector3(in Vector4 vec4) {
		return new Vector3(vec4.X, vec4.Y, vec4.Z);
	}

	public bool Equals(Vector3 other) {
		return Comparison.DoEqual(X, other.X)
			&& Comparison.DoEqual(Y, other.Y)
			&& Comparison.DoEqual(Z, other.Z);
	}

	public override bool Equals(object? obj) {
		return obj is Vector3 other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(X, Y, Z);
	}

	public override string ToString() {
		return $"({X:F3}, {Y:F3}, {Z:F3})";
	}

	public static bool operator ==(in Vector3 a, in Vector3 b) {
		return a.Equals(b);
	}

	public static bool operator !=(in Vector3 a, in Vector3 b) {
		return !a.Equals(b);
	}

	public static Vector3 operator +(in Vector3 a, in Vector3 b) {
		return a.Add(b);
	}

	public static Vector3 operator -(in Vector3 a, in Vector3 b) {
		return a.Subtract(b);
	}

	public static Vector3 operator -(in Vector3 v) {
		return v.Negate();
	}

	public static Vector3 operator *(in Vector3 v, float scalar) {
		return v.Multiply(scalar);
	}

	public static Vector3 operator *(float scalar, in Vector3 v) {
		return v.Multiply(scalar);
	}

	public static Vector3 operator /(in Vector3 a, float scalar) {
		return a.Divide(scalar);
	}
}
