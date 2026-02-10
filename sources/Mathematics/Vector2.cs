using System.Runtime.CompilerServices;

namespace Mino.Mathematics;

/// <summary>
///     Immutable vector 2D.
/// </summary>
public readonly struct Vector2 : IEquatable<Vector2> {
	public static readonly Vector2 Zero = new Vector2(0.0F, 0.0F);
	public static readonly Vector2 UnitX = new Vector2(1.0F, 0.0F);
	public static readonly Vector2 UnitY = new Vector2(0.0F, 1.0F);

	public readonly float X = 0.0F;
	public readonly float Y = 0.0F;

	public Vector2() {
	}

	public Vector2(float x, float y) {
		X = x;
		Y = y;
	}

	/// <summary>
	///     Gets the vector component at the specified index (0=X, 1=Y).
	/// </summary>
	/// <param name="index">Index of the component.</param>
	/// <returns>The component value.</returns>
	public float this[int index] {
		get => Unsafe.Add(ref Unsafe.As<Vector2, float>(ref Unsafe.AsRef(in this)), index);
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
		get => X * X + Y * Y;
	}

	/// <summary>
	///     Adds another vector to this vector.
	/// </summary>
	/// <param name="other">The vector to add.</param>
	/// <returns>The sum vector.</returns>
	public Vector2 Add(in Vector2 other) {
		return new Vector2(X + other.X, Y + other.Y);
	}

	/// <summary>
	///     Subtracts another vector from this vector.
	/// </summary>
	/// <param name="other">The vector to subtract.</param>
	/// <returns>The difference vector.</returns>
	public Vector2 Subtract(in Vector2 other) {
		return new Vector2(X - other.X, Y - other.Y);
	}

	/// <summary>
	///     Negates this vector.
	/// </summary>
	/// <returns>The negated vector.</returns>
	public Vector2 Negate() {
		return new Vector2(-X, -Y);
	}

	/// <summary>
	///     Multiplies this vector by a scalar.
	/// </summary>
	/// <param name="scalar">The scalar to multiply by.</param>
	/// <returns>The scaled vector.</returns>
	public Vector2 Multiply(float scalar) {
		return new Vector2(X * scalar, Y * scalar);
	}

	/// <summary>
	///     Divides this vector by a scalar.
	/// </summary>
	/// <param name="scalar">The scalar to divide by.</param>
	/// <returns>The scaled vector.</returns>
	public Vector2 Divide(float scalar) {
		return new Vector2(X / scalar, Y / scalar);
	}

	/// <summary>
	///     Scales this vector component-wise by another vector.
	/// </summary>
	/// <param name="other">The scaling vector.</param>
	/// <returns>The scaled vector.</returns>
	public Vector2 Scale(in Vector2 other) {
		return new Vector2(X * other.X, Y * other.Y);
	}

	/// <summary>
	///     Computes the cross product (2D cross product returns a scalar).
	/// </summary>
	/// <param name="v">The other vector.</param>
	/// <returns>The cross product scalar.</returns>
	public float Cross(in Vector2 v) {
		return X * v.Y - Y * v.X;
	}

	/// <summary>
	///     Computes the dot product with another vector.
	/// </summary>
	/// <param name="v">The other vector.</param>
	/// <returns>The dot product.</returns>
	public float Dot(in Vector2 v) {
		return X * v.X + Y * v.Y;
	}

	/// <summary>
	///     Normalizes this vector to unit length.
	/// </summary>
	/// <returns>The normalized vector, or zero vector if length is zero.</returns>
	public Vector2 Normalize() {
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
	public Vector2 Reflect(in Vector2 normal) {
		return this - 2.0F * Dot(normal) * normal;
	}

	/// <summary>
	///     Projects this vector onto another vector.
	/// </summary>
	/// <param name="onNormal">The vector to project onto.</param>
	/// <returns>The projection vector, or zero if onNormal is zero.</returns>
	public Vector2 Project(in Vector2 onNormal) {
		float lengthSq = onNormal.LengthSquared;
		if (Comparison.DoEqual(0.0F, lengthSq)) {
			return Zero;
		}
		float dot = Dot(onNormal);
		return onNormal * (dot / lengthSq);
	}

	/// <summary>
	///     Rejects this vector from another vector (perpendicular component).
	/// </summary>
	/// <param name="onNormal">The vector to reject from.</param>
	/// <returns>The rejection vector.</returns>
	public Vector2 Reject(in Vector2 onNormal) {
		return this - Project(onNormal);
	}

	/// <summary>
	///     Rotates this vector by the specified angle.
	/// </summary>
	/// <param name="angle">Rotation angle in radians.</param>
	/// <returns>The rotated vector.</returns>
	public Vector2 Rotate(float angle) {
		FastTrigonometric.Get(angle, out float sin, out float cos);
		return new Vector2(X * cos - Y * sin, X * sin + Y * cos);
	}

	/// <summary>
	///     Rotates this vector around a center point by the specified angle.
	/// </summary>
	/// <param name="center">The center of rotation.</param>
	/// <param name="angle">Rotation angle in radians.</param>
	/// <returns>The rotated vector.</returns>
	public Vector2 Rotate(in Vector2 center, float angle) {
		Vector2 offset = this - center;
		Vector2 rotated = offset.Rotate(angle);
		return center + rotated;
	}

	/// <summary>
	///     Computes the distance between two points.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The distance between the points.</returns>
	public static float Distance(in Vector2 a, in Vector2 b) {
		return MathF.Sqrt(DistanceSquared(a, b));
	}

	/// <summary>
	///     Computes the squared distance between two points.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The squared distance between the points.</returns>
	public static float DistanceSquared(in Vector2 a, in Vector2 b) {
		float dx = a.X - b.X;
		float dy = a.Y - b.Y;
		return dx * dx + dy * dy;
	}

	/// <summary>
	///     Computes the Manhattan distance between two points.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The Manhattan distance.</returns>
	public static float ManhattanDistance(in Vector2 a, in Vector2 b) {
		return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
	}

	/// <summary>
	///     Computes the angle between two vectors.
	/// </summary>
	/// <param name="a">First vector.</param>
	/// <param name="b">Second vector.</param>
	/// <returns>The angle in radians.</returns>
	public static float GetAngle(in Vector2 a, in Vector2 b) {
		float denominator = MathF.Sqrt(a.LengthSquared * b.LengthSquared);
		if (Comparison.DoEqual(0.0F, denominator)) {
			return 0.0F;
		}
		float clamped = Math.Clamp(a.Dot(b) / denominator, -1.0F, 1.0F);
		return MathF.Acos(clamped);
	}

	/// <summary>
	///     Returns a vector with the minimum components of two vectors.
	/// </summary>
	/// <param name="a">First vector.</param>
	/// <param name="b">Second vector.</param>
	/// <returns>The component-wise minimum vector.</returns>
	public static Vector2 Min(in Vector2 a, in Vector2 b) {
		return new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
	}

	/// <summary>
	///     Returns a vector with the maximum components of two vectors.
	/// </summary>
	/// <param name="a">First vector.</param>
	/// <param name="b">Second vector.</param>
	/// <returns>The component-wise maximum vector.</returns>
	public static Vector2 Max(in Vector2 a, in Vector2 b) {
		return new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
	}

	/// <summary>
	///     Clamps a vector's components between min and max vectors.
	/// </summary>
	/// <param name="v">The vector to clamp.</param>
	/// <param name="min">Minimum vector.</param>
	/// <param name="max">Maximum vector.</param>
	/// <returns>The clamped vector.</returns>
	public static Vector2 Clamp(in Vector2 v, in Vector2 min, in Vector2 max) {
		return new Vector2(Math.Clamp(v.X, min.X, max.X), Math.Clamp(v.Y, min.Y, max.Y));
	}

	/// <summary>
	///     Linearly interpolates between two vectors.
	/// </summary>
	/// <param name="a">Start vector.</param>
	/// <param name="b">End vector.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>The interpolated vector.</returns>
	public static Vector2 Lerp(in Vector2 a, in Vector2 b, float t) {
		t = Math.Clamp(t, 0.0F, 1.0F);
		return new Vector2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
	}

	/// <summary>
	///     Creates a directional vector from an angle and length.
	/// </summary>
	/// <param name="angle">Direction angle in radians.</param>
	/// <param name="length">Length of the vector (default 1.0).</param>
	/// <returns>A directional vector.</returns>
	public static Vector2 CreateDirectional(float angle, float length = 1.0F) {
		FastTrigonometric.Get(angle, out float sin, out float cos);
		return new Vector2(cos, sin) * length;
	}
	
	// Implicit cast Vector3 -> Vector2.
	public static implicit operator Vector2(in Vector3 vec3) {
		return new Vector2(vec3.X, vec3.Y);
	} 
	
	// Implicit cast Vector4 -> Vector2.
	public static implicit operator Vector2(in Vector4 vec4) {
		return new Vector2(vec4.X, vec4.Y);
	} 

	public bool Equals(Vector2 other) {
		return Comparison.DoEqual(X, other.X) && Comparison.DoEqual(Y, other.Y);
	}

	public override bool Equals(object? obj) {
		return obj is Vector2 other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(X, Y);
	}

	public override string ToString() {
		return $"({X:F3}, {Y:F3})";
	}

	public static bool operator ==(in Vector2 a, in Vector2 b) {
		return a.Equals(b);
	}

	public static bool operator !=(in Vector2 a, in Vector2 b) {
		return !a.Equals(b);
	}

	public static Vector2 operator +(in Vector2 a, in Vector2 b) {
		return a.Add(b);
	}

	public static Vector2 operator -(in Vector2 a, in Vector2 b) {
		return a.Subtract(b);
	}

	public static Vector2 operator -(in Vector2 v) {
		return v.Negate();
	}

	public static Vector2 operator *(in Vector2 v, float scalar) {
		return v.Multiply(scalar);
	}

	public static Vector2 operator *(float scalar, in Vector2 v) {
		return v.Multiply(scalar);
	}

	public static Vector2 operator /(in Vector2 a, float scalar) {
		return a.Divide(scalar);
	}
}
