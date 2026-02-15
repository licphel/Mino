#region
using System.Runtime.CompilerServices;
#endregion

namespace Mino.Mathematics;

/// <summary>
///     Immutable vector 4D.
/// </summary>
public readonly struct Vector4 : IEquatable<Vector4> {
	public static readonly Vector4 Zero = new Vector4(0.0F, 0.0F, 0.0F, 0.0F);
	public static readonly Vector4 UnitX = new Vector4(1.0F, 0.0F, 0.0F, 0.0F);
	public static readonly Vector4 UnitY = new Vector4(0.0F, 1.0F, 0.0F, 0.0F);
	public static readonly Vector4 UnitZ = new Vector4(0.0F, 0.0F, 1.0F, 0.0F);
	public static readonly Vector4 UnitW = new Vector4(0.0F, 0.0F, 0.0F, 1.0F);

	public readonly float X = 0.0F;
	public readonly float Y = 0.0F;
	public readonly float Z = 0.0F;
	public readonly float W = 0.0F;

	public Vector4() {
	}

	public Vector4(float x, float y, float z, float w) {
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public Vector4(in Vector3 vec3, float w) {
		X = vec3.X;
		Y = vec3.Y;
		Z = vec3.Z;
		W = w;
	}

	/// <summary>
	///     Gets the vector component at the specified index (0=X, 1=Y, 2=Z, 3=W).
	/// </summary>
	/// <param name="index">Index of the component.</param>
	/// <returns>The component value.</returns>
	public float this[int index] {
		get => Unsafe.Add(ref Unsafe.As<Vector4, float>(ref Unsafe.AsRef(in this)), index);
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
		get => X * X + Y * Y + Z * Z + W * W;
	}

	/// <summary>
	///     Adds another vector to this vector.
	/// </summary>
	/// <param name="other">The vector to add.</param>
	/// <returns>The sum vector.</returns>
	public Vector4 Add(in Vector4 other) {
		return new Vector4(X + other.X, Y + other.Y, Z + other.Z, W + other.W);
	}

	/// <summary>
	///     Subtracts another vector from this vector.
	/// </summary>
	/// <param name="other">The vector to subtract.</param>
	/// <returns>The difference vector.</returns>
	public Vector4 Subtract(in Vector4 other) {
		return new Vector4(X - other.X, Y - other.Y, Z - other.Z, W - other.W);
	}

	/// <summary>
	///     Negates this vector.
	/// </summary>
	/// <returns>The negated vector.</returns>
	public Vector4 Negate() {
		return new Vector4(-X, -Y, -Z, -W);
	}

	/// <summary>
	///     Multiplies this vector by a scalar.
	/// </summary>
	/// <param name="scalar">The scalar to multiply by.</param>
	/// <returns>The scaled vector.</returns>
	public Vector4 Multiply(float scalar) {
		return new Vector4(X * scalar, Y * scalar, Z * scalar, W * scalar);
	}

	/// <summary>
	///     Divides this vector by a scalar.
	/// </summary>
	/// <param name="scalar">The scalar to divide by.</param>
	/// <returns>The scaled vector.</returns>
	public Vector4 Divide(float scalar) {
		return new Vector4(X / scalar, Y / scalar, Z / scalar, W / scalar);
	}

	/// <summary>
	///     Scales this vector component-wise by another vector.
	/// </summary>
	/// <param name="other">The scaling vector.</param>
	/// <returns>The scaled vector.</returns>
	public Vector4 Scale(in Vector4 other) {
		return new Vector4(X * other.X, Y * other.Y, Z * other.Z, W * other.W);
	}

	/// <summary>
	///     Computes the dot product with another vector.
	/// </summary>
	/// <param name="other">The other vector.</param>
	/// <returns>The dot product.</returns>
	public float Dot(in Vector4 other) {
		return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
	}

	/// <summary>
	///     Normalizes this vector to unit length.
	/// </summary>
	/// <returns>The normalized vector, or zero vector if length is zero.</returns>
	public Vector4 Normalize() {
		float length = Length;
		if (Comparison.DoEqual(0.0F, length)) {
			return Zero;
		}
		return this / length;
	}

	/// <summary>
	///     Performs homogeneous normalization (divides X, Y, Z by W).
	/// </summary>
	/// <returns>The normalized homogeneous vector, or zero if W is zero.</returns>
	public Vector4 HomogeneousNormalize() {
		if (Comparison.DoEqual(0.0F, W)) {
			return Zero;
		}
		return new Vector4(X / W, Y / W, Z / W, 1.0F);
	}

	/// <summary>
	///     Reflects this vector across a normal.
	/// </summary>
	/// <param name="normal">The reflection normal (must be normalized).</param>
	/// <returns>The reflected vector.</returns>
	public Vector4 Reflect(in Vector4 normal) {
		return this - 2.0F * Dot(normal) * normal;
	}

	/// <summary>
	///     Projects this vector onto another vector.
	/// </summary>
	/// <param name="onNormal">The vector to project onto.</param>
	/// <returns>The projection vector, or zero if onNormal is zero.</returns>
	public Vector4 Project(in Vector4 onNormal) {
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
	public Vector4 Reject(in Vector4 onNormal) {
		return this - Project(onNormal);
	}

	/// <summary>
	///     Computes the distance between two points.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The distance between the points.</returns>
	public static float Distance(in Vector4 a, in Vector4 b) {
		return MathF.Sqrt(DistanceSquared(a, b));
	}

	/// <summary>
	///     Computes the squared distance between two points.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The squared distance between the points.</returns>
	public static float DistanceSquared(in Vector4 a, in Vector4 b) {
		float dx = a.X - b.X;
		float dy = a.Y - b.Y;
		float dz = a.Z - b.Z;
		float dw = a.W - b.W;
		return dx * dx + dy * dy + dz * dz + dw * dw;
	}

	/// <summary>
	///     Computes the Manhattan distance between two points.
	/// </summary>
	/// <param name="a">First point.</param>
	/// <param name="b">Second point.</param>
	/// <returns>The Manhattan distance.</returns>
	public static float ManhattanDistance(in Vector4 a, in Vector4 b) {
		return MathF.Abs(a.X - b.X) + MathF.Abs(a.Y - b.Y) + MathF.Abs(a.Z - b.Z)
			+ MathF.Abs(a.W - b.W);
	}

	/// <summary>
	///     Computes the angle between two vectors.
	/// </summary>
	/// <param name="a">First vector.</param>
	/// <param name="b">Second vector.</param>
	/// <returns>The angle in radians.</returns>
	public static float GetAngle(in Vector4 a, in Vector4 b) {
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
	public static Vector4 Min(in Vector4 a, in Vector4 b) {
		return new Vector4(
			MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y), MathF.Min(a.Z, b.Z), MathF.Min(a.W, b.W));
	}

	/// <summary>
	///     Returns a vector with the maximum components of two vectors.
	/// </summary>
	/// <param name="a">First vector.</param>
	/// <param name="b">Second vector.</param>
	/// <returns>The component-wise maximum vector.</returns>
	public static Vector4 Max(in Vector4 a, in Vector4 b) {
		return new Vector4(
			MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z), MathF.Max(a.W, b.W));
	}

	/// <summary>
	///     Clamps a vector's components between min and max vectors.
	/// </summary>
	/// <param name="value">The vector to clamp.</param>
	/// <param name="min">Minimum vector.</param>
	/// <param name="max">Maximum vector.</param>
	/// <returns>The clamped vector.</returns>
	public static Vector4 Clamp(in Vector4 value, in Vector4 min, in Vector4 max) {
		return new Vector4(
			Math.Clamp(value.X, min.X, max.X), Math.Clamp(value.Y, min.Y, max.Y),
			Math.Clamp(value.Z, min.Z, max.Z), Math.Clamp(value.W, min.W, max.W));
	}

	/// <summary>
	///     Linearly interpolates between two vectors.
	/// </summary>
	/// <param name="a">Start vector.</param>
	/// <param name="b">End vector.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>The interpolated vector.</returns>
	public static Vector4 Lerp(in Vector4 a, in Vector4 b, float t) {
		t = Math.Clamp(t, 0.0F, 1.0F);
		return new Vector4(
			a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t,
			a.W + (b.W - a.W) * t);
	}

	/// <summary>
	///     Creates a color vector from a rgba color.
	/// </summary>
	/// <param name="color">The rgba color.</param>
	/// <returns>The color vector.</returns>
	public static Vector4 CreateColor(in Color color) {
		return Unsafe.As<Color, Vector4>(ref Unsafe.AsRef(in color));
	}

	/// <summary>
	///     Converts the vector as a rgba color.
	/// </summary>
	/// <returns>A rgba color.</returns>
	public Color ToColor() {
		return Unsafe.As<Vector4, Color>(ref Unsafe.AsRef(in this));
	}

	// Implicit cast Vector2 -> Vector4.
	public static implicit operator Vector4(in Vector2 vec2) {
		return new Vector4(vec2.X, vec2.Y, 0.0F, 0.0F);
	}

	// Implicit cast Vector3 -> Vector4.
	public static implicit operator Vector4(in Vector3 vec3) {
		return new Vector4(vec3.X, vec3.Y, vec3.Z, 0.0F);
	}

	public bool Equals(Vector4 other) {
		return Comparison.DoEqual(X, other.X)
			&& Comparison.DoEqual(Y, other.Y)
			&& Comparison.DoEqual(Z, other.Z)
			&& Comparison.DoEqual(W, other.W);
	}

	public override bool Equals(object? obj) {
		return obj is Vector4 other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(X, Y, Z, W);
	}

	public override string ToString() {
		return $"({X:F3}, {Y:F3}, {Z:F3}, {W:F3})";
	}

	public static bool operator ==(in Vector4 a, in Vector4 b) {
		return a.Equals(b);
	}

	public static bool operator !=(in Vector4 a, in Vector4 b) {
		return !a.Equals(b);
	}

	public static Vector4 operator +(in Vector4 a, in Vector4 b) {
		return a.Add(b);
	}

	public static Vector4 operator -(in Vector4 a, in Vector4 b) {
		return a.Subtract(b);
	}

	public static Vector4 operator -(in Vector4 v) {
		return v.Negate();
	}

	public static Vector4 operator *(in Vector4 v, float scalar) {
		return v.Multiply(scalar);
	}

	public static Vector4 operator *(float scalar, in Vector4 v) {
		return v.Multiply(scalar);
	}

	public static Vector4 operator /(in Vector4 v, float scalar) {
		return v.Divide(scalar);
	}
}
