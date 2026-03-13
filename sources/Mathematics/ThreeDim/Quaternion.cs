#region
using System.Runtime.CompilerServices;
using Mino.Utility;
#endregion

namespace Mino.Mathematics.ThreeDim;

/// <summary>
///     Immutable quaternion.
/// </summary>
public readonly struct Quaternion : IEquatable<Quaternion> {
	public static readonly Quaternion Identity = default;

	public readonly float X = 0.0F;
	public readonly float Y = 0.0F;
	public readonly float Z = 0.0F;
	public readonly float W = 1.0F;

	public Quaternion() {
	}

	public Quaternion(float x, float y, float z, float w) {
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="Quaternion" /> struct from axis-angle representation.
	/// </summary>
	/// <param name="axis">Axis of rotation.</param>
	/// <param name="angle">Rotation angle in radians.</param>
	public Quaternion(in Vector3 axis, float angle) {
		float halfAngle = angle * 0.5F;
		FastTrigonometric.Get(halfAngle, out float sin, out float cos);

		Vector3 normalizedAxis = axis.Normalize();
		X = normalizedAxis.X * sin;
		Y = normalizedAxis.Y * sin;
		Z = normalizedAxis.Z * sin;
		W = cos;
	}

	/// <summary>
	///     Gets the quaternion component at the specified index (0=X, 1=Y, 2=Z, 3=W).
	/// </summary>
	/// <param name="index">Index of the component.</param>
	/// <returns>The component value.</returns>
	public float this[int index] {
		get => Unsafe.Add(ref Unsafe.As<Quaternion, float>(ref Unsafe.AsRef(in this)), index);
	}

	/// <summary>
	///     Gets the length (magnitude) of the quaternion.
	/// </summary>
	public float Length {
		get => MathF.Sqrt(LengthSquared);
	}

	/// <summary>
	///     Gets the squared length of the quaternion.
	/// </summary>
	public float LengthSquared {
		get => X * X + Y * Y + Z * Z + W * W;
	}

	/// <summary>
	///     Adds another quaternion to this quaternion.
	/// </summary>
	/// <param name="q">The quaternion to add.</param>
	/// <returns>The sum quaternion.</returns>
	public Quaternion Add(in Quaternion q) {
		return new Quaternion(X + q.X, Y + q.Y, Z + q.Z, W + q.W);
	}

	/// <summary>
	///     Subtracts another quaternion from this quaternion.
	/// </summary>
	/// <param name="q">The quaternion to subtract.</param>
	/// <returns>The difference quaternion.</returns>
	public Quaternion Subtract(in Quaternion q) {
		return new Quaternion(X - q.X, Y - q.Y, Z - q.Z, W - q.W);
	}

	/// <summary>
	///     Negates this quaternion.
	/// </summary>
	/// <returns>The negated quaternion.</returns>
	public Quaternion Negate() {
		return new Quaternion(-X, -Y, -Z, -W);
	}

	/// <summary>
	///     Multiplies this quaternion by another quaternion.
	/// </summary>
	/// <param name="other">The quaternion to multiply by.</param>
	/// <returns>The product quaternion.</returns>
	public Quaternion Multiply(in Quaternion other) {
		return new Quaternion(
			W * other.X + X * other.W + Y * other.Z - Z * other.Y,
			W * other.Y - X * other.Z + Y * other.W + Z * other.X,
			W * other.Z + X * other.Y - Y * other.X + Z * other.W,
			W * other.W - X * other.X - Y * other.Y - Z * other.Z
		);
	}

	/// <summary>
	///     Multiplies this quaternion by a scalar.
	/// </summary>
	/// <param name="scalar">The scalar to multiply by.</param>
	/// <returns>The scaled quaternion.</returns>
	public Quaternion Multiply(float scalar) {
		return new Quaternion(X * scalar, Y * scalar, Z * scalar, W * scalar);
	}

	/// <summary>
	///     Divides this quaternion by a scalar.
	/// </summary>
	/// <param name="scalar">The scalar to divide by.</param>
	/// <returns>The scaled quaternion.</returns>
	public Quaternion Divide(float scalar) {
		return new Quaternion(X / scalar, Y / scalar, Z / scalar, W / scalar);
	}

	/// <summary>
	///     Computes the dot product with another quaternion.
	/// </summary>
	/// <param name="other">The other quaternion.</param>
	/// <returns>The dot product.</returns>
	public float Dot(in Quaternion other) {
		return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
	}

	/// <summary>
	///     Normalizes this quaternion to unit length.
	/// </summary>
	/// <returns>The normalized quaternion.</returns>
	/// <exception cref="Crash">Thrown when the quaternion has zero length.</exception>
	public Quaternion Normalize() {
		float len = Length;
		if (Comparison.DoEqual(0.0F, len)) {
			return Identity;
		}

		float invLen = 1.0F / len;
		return new Quaternion(X * invLen, Y * invLen, Z * invLen, W * invLen);
	}

	/// <summary>
	///     Returns the conjugate of this quaternion.
	/// </summary>
	/// <returns>The conjugate quaternion.</returns>
	public Quaternion Conjugate() {
		return new Quaternion(-X, -Y, -Z, W);
	}

	/// <summary>
	///     Returns the inverse of this quaternion.
	/// </summary>
	/// <returns>The inverse quaternion, or identity if zero length.</returns>
	public Quaternion Invert() {
		float lenSq = LengthSquared;
		if (lenSq == 0.0F) {
			return Identity;
		}

		float invLenSq = 1.0F / lenSq;
		return new Quaternion(-X * invLenSq, -Y * invLenSq, -Z * invLenSq, W * invLenSq);
	}

	/// <summary>
	///     Rotates a vector by this quaternion.
	/// </summary>
	/// <param name="vector">The vector to rotate.</param>
	/// <returns>The rotated vector.</returns>
	public Vector3 Rotate(in Vector3 vector) {
		float x2 = X * 2.0F;
		float y2 = Y * 2.0F;
		float z2 = Z * 2.0F;
		float xx2 = X * x2;
		float xy2 = X * y2;
		float xz2 = X * z2;
		float yy2 = Y * y2;
		float yz2 = Y * z2;
		float zz2 = Z * z2;
		float wx2 = W * x2;
		float wy2 = W * y2;
		float wz2 = W * z2;

		return new Vector3(
			(1.0F - (yy2 + zz2)) * vector.X + (xy2 - wz2) * vector.Y + (xz2 + wy2) * vector.Z,
			(xy2 + wz2) * vector.X + (1.0F - (xx2 + zz2)) * vector.Y + (yz2 - wx2) * vector.Z,
			(xz2 - wy2) * vector.X + (yz2 + wx2) * vector.Y + (1.0F - (xx2 + yy2)) * vector.Z
		);
	}

	/// <summary>
	///     Converts this quaternion to axis-angle representation.
	/// </summary>
	/// <param name="axis">Output axis of rotation.</param>
	/// <param name="angle">Output rotation angle in radians.</param>
	public void ToAxisAngle(out Vector3 axis, out float angle) {
		float lengthSq = X * X + Y * Y + Z * Z;

		if (!Comparison.DoEqual(0.0F, lengthSq)) {
			float invLength = 1.0F / MathF.Sqrt(lengthSq);

			axis = new Vector3(X * invLength, Y * invLength, Z * invLength);
			angle = 2.0F * MathF.Acos(Math.Clamp(W, -1.0F, 1.0F));
		} else {
			axis = Vector3.UnitZ;
			angle = 0.0F;
		}
	}

	/// <summary>
	///     Converts this quaternion to Euler angles (yaw, pitch, roll).
	/// </summary>
	/// <returns>Euler angles vector.</returns>
	public Euler ToEuler() {
		float sinP = 2.0F * (W * Y - Z * X);
		float pitch;
		float yaw;

		// Check the 'gimbal lock' case.
		if (Math.Abs(sinP) > 1.0F - 1E-4F) {
			pitch = MathF.CopySign(MathF.PI / 2.0F, sinP);
			yaw = MathF.Atan2(2.0F * (W * Z + X * Y), 1.0F - 2.0F * (Y * Y + Z * Z));
			return new Euler(yaw, pitch, 0.0F);
		}

		float a = 2.0F * (W * X + Y * Z);
		float b = 1.0F - 2.0F * (X * X + Y * Y);
		float roll = MathF.Atan2(a, b);
		pitch = MathF.Asin(sinP);
		float c = 2.0F * (W * Z + X * Y);
		float d = 1.0F - 2.0F * (Y * Y + Z * Z);
		yaw = MathF.Atan2(c, d);
		return new Euler(yaw, pitch, roll);
	}

	/// <summary>
	///     Converts this quaternion to a 3x3 rotation matrix.
	/// </summary>
	/// <returns>A 3x3 rotation matrix.</returns>
	public Matrix3x3 ToMatrix3x3() {
		float xx = X * X;
		float yy = Y * Y;
		float zz = Z * Z;
		float xy = X * Y;
		float xz = X * Z;
		float yz = Y * Z;
		float wx = W * X;
		float wy = W * Y;
		float wz = W * Z;

		return new Matrix3x3(
			1.0F - 2.0F * (yy + zz),
			2.0F * (xy - wz),
			2.0F * (xz + wy),
			2.0F * (xy + wz),
			1.0F - 2.0F * (xx + zz),
			2.0F * (yz - wx),
			2.0F * (xz - wy),
			2.0F * (yz + wx),
			1.0F - 2.0F * (xx + yy)
		);
	}

	/// <summary>
	///     Converts this quaternion to a 4x4 rotation matrix.
	/// </summary>
	/// <returns>A 4x4 rotation matrix.</returns>
	public Matrix4x4 ToMatrix4x4() {
		return ToMatrix3x3().ToMatrix4x4();
	}

	/// <summary>
	///     Creates a quaternion from Euler angles.
	/// </summary>
	/// <param name="pitch">Rotation around the X axis (pitch).</param>
	/// <param name="yaw">Rotation around the Y axis (yaw).</param>
	/// <param name="roll">Rotation around the Z axis (roll).</param>
	/// <returns>A quaternion representing the rotation.</returns>
	public static Quaternion CreateFromEuler(float yaw, float pitch, float roll) {
		Quaternion pitchQ = CreateAxisX(pitch);
		Quaternion yawQ = CreateAxisY(yaw);
		Quaternion rollQ = CreateAxisZ(roll);
		return yawQ * pitchQ * rollQ;
	}

	/// <summary>
	///     Creates a quaternion from Euler angles.
	/// </summary>
	/// <param name="euler">Euler angles vector (pitch, yaw, roll).</param>
	/// <returns>A quaternion representing the rotation.</returns>
	public static Quaternion CreateFromEuler(in Euler euler) {
		return CreateFromEuler(euler.Yaw, euler.Pitch, euler.Roll);
	}

	/// <summary>
	///     Creates a quaternion that rotates from one direction to another.
	/// </summary>
	/// <param name="from">Starting direction.</param>
	/// <param name="to">Target direction.</param>
	/// <returns>A quaternion representing the rotation.</returns>
	public static Quaternion CreateFromToRotation(in Vector3 from, in Vector3 to) {
		Vector3 v0 = from.Normalize();
		Vector3 a = to.Normalize();
		float dot = v0.Dot(a);

		if (dot > 1.0F - 1E-4F) {
			return Identity;
		}
		if (dot < -1.0F + 1E-4F) {
			Vector3 axis = Vector3.UnitX.Cross(v0);
			if (Comparison.DoEqual(0.0F, axis.LengthSquared)) {
				axis = Vector3.UnitY.Cross(v0);
			}
			axis = axis.Normalize();
			return new Quaternion(axis, MathF.PI);
		}

		Vector3 axisVec = v0.Cross(a);
		float s = MathF.Sqrt((1.0F + dot) * 2.0F);
		float invS = 1.0F / s;

		return new Quaternion(axisVec.X * invS, axisVec.Y * invS, axisVec.Z * invS, s * 0.5F);
	}

	/// <summary>
	///     Creates a quaternion from a 3x3 rotation matrix.
	/// </summary>
	/// <param name="m">The rotation matrix.</param>
	/// <returns>A quaternion representing the rotation.</returns>
	public static Quaternion CreateFromMatrix(in Matrix3x3 m) {
		float trace = m.M00 + m.M11 + m.M22;

		if (trace > 0.0F) {
			float s = MathF.Sqrt(trace + 1.0F) * 2.0F;
			float invS = 1.0F / s;
			return new Quaternion(
				(m.M21 - m.M12) * invS, (m.M02 - m.M20) * invS, (m.M10 - m.M01) * invS, s * 0.25F);
		}

		if (m.M00 > m.M11 && m.M00 > m.M22) {
			float s = MathF.Sqrt(1.0F + m.M00 - m.M11 - m.M22) * 2.0F;
			float invS = 1.0F / s;
			return new Quaternion(
				s * 0.25F, (m.M01 + m.M10) * invS, (m.M02 + m.M20) * invS, (m.M21 - m.M12) * invS);
		}

		if (m.M11 > m.M22) {
			float s = MathF.Sqrt(1.0F + m.M11 - m.M00 - m.M22) * 2.0F;
			float invS = 1.0F / s;
			return new Quaternion(
				(m.M01 + m.M10) * invS, s * 0.25F, (m.M12 + m.M21) * invS, (m.M02 - m.M20) * invS);
		} else {
			float s = MathF.Sqrt(1.0F + m.M22 - m.M00 - m.M11) * 2.0F;
			float invS = 1.0F / s;
			return new Quaternion(
				(m.M02 + m.M20) * invS, (m.M12 + m.M21) * invS, s * 0.25F, (m.M10 - m.M01) * invS);
		}
	}

	/// <summary>
	///     Creates a quaternion from a 4x4 rotation matrix.
	/// </summary>
	/// <param name="m">The rotation matrix.</param>
	/// <returns>A quaternion representing the rotation.</returns>
	public static Quaternion CreateFromMatrix(in Matrix4x4 m) {
		Matrix3x3 rot = new Matrix3x3(
			m.M00, m.M10, m.M20,
			m.M01, m.M11, m.M21,
			m.M02, m.M12, m.M22
		);
		return CreateFromMatrix(rot);
	}

	/// <summary>
	///     Creates a quaternion representing rotation around the X axis.
	/// </summary>
	/// <param name="angle">Rotation angle in radians.</param>
	/// <returns>A quaternion representing the rotation.</returns>
	public static Quaternion CreateAxisX(float angle) {
		float halfAngle = angle * 0.5F;
		FastTrigonometric.Get(halfAngle, out float sin, out float cos);
		return new Quaternion(sin, 0.0F, 0.0F, cos);
	}

	/// <summary>
	///     Creates a quaternion representing rotation around the Y axis.
	/// </summary>
	/// <param name="angle">Rotation angle in radians.</param>
	/// <returns>A quaternion representing the rotation.</returns>
	public static Quaternion CreateAxisY(float angle) {
		float halfAngle = angle * 0.5F;
		FastTrigonometric.Get(halfAngle, out float sin, out float cos);
		return new Quaternion(0.0F, sin, 0.0F, cos);
	}

	/// <summary>
	///     Creates a quaternion representing rotation around the Z axis.
	/// </summary>
	/// <param name="angle">Rotation angle in radians.</param>
	/// <returns>A quaternion representing the rotation.</returns>
	public static Quaternion CreateAxisZ(float angle) {
		float halfAngle = angle * 0.5F;
		FastTrigonometric.Get(halfAngle, out float sin, out float cos);
		return new Quaternion(0.0F, 0.0F, sin, cos);
	}

	/// <summary>
	///     Creates a quaternion from the coordinate basis.
	/// </summary>
	/// <param name="right">Right directional vector.</param>
	/// <param name="up">Up directional vector.</param>
	/// <param name="forward">Forward directional vector.</param>
	/// <returns>A quaternion representing the rotation.</returns>
	public static Quaternion CreateFromBasis(in Vector3 right, in Vector3 up, in Vector3 forward) {
		return CreateFromMatrix(
			new Matrix3x3(
				right.X, up.X, forward.X,
				right.Y, up.Y, forward.Y,
				right.Z, up.Z, forward.Z
			));
	}

	/// <summary>
	///     Linearly interpolates between two quaternions.
	/// </summary>
	/// <param name="a">Start quaternion.</param>
	/// <param name="b">End quaternion.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>The interpolated quaternion.</returns>
	public static Quaternion Lerp(in Quaternion a, in Quaternion b, float t) {
		return new Quaternion(
			a.X + (b.X - a.X) * t,
			a.Y + (b.Y - a.Y) * t,
			a.Z + (b.Z - a.Z) * t,
			a.W + (b.W - a.W) * t
		);
	}

	/// <summary>
	///     Spherically interpolates between two quaternions.
	/// </summary>
	/// <param name="a">Start quaternion.</param>
	/// <param name="b">End quaternion.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>The interpolated quaternion.</returns>
	public static Quaternion Slerp(in Quaternion a, in Quaternion b, float t) {
		float cosOmega = a.Dot(b);

		Quaternion endAdjusted = b;
		if (cosOmega < 0.0F) {
			endAdjusted = new Quaternion(-b.X, -b.Y, -b.Z, -b.W);
			cosOmega = -cosOmega;
		}

		float k0, k1;
		if (cosOmega > 1.0F - 1E-4F) {
			k0 = 1.0F - t;
			k1 = t;
		} else {
			float sinOmega = MathF.Sqrt(1.0F - cosOmega * cosOmega);
			float omega = MathF.Atan2(sinOmega, cosOmega);
			float invSinOmega = 1.0F / sinOmega;
			k0 = MathF.Sin((1.0F - t) * omega) * invSinOmega;
			k1 = MathF.Sin(t * omega) * invSinOmega;
		}

		return new Quaternion(
			a.X * k0 + endAdjusted.X * k1,
			a.Y * k0 + endAdjusted.Y * k1,
			a.Z * k0 + endAdjusted.Z * k1,
			a.W * k0 + endAdjusted.W * k1
		);
	}

	public bool Equals(Quaternion other) {
		return Comparison.DoEqual(X, other.X)
			&& Comparison.DoEqual(Y, other.Y)
			&& Comparison.DoEqual(Z, other.Z)
			&& Comparison.DoEqual(W, other.W);
	}

	public override bool Equals(object? obj) {
		return obj is Quaternion other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(X, Y, Z, W);
	}

	public override string ToString() {
		return $"({X:F3}, {Y:F3}, {Z:F3}, {W:F3})";
	}

	public static bool operator ==(in Quaternion q1, in Quaternion q2) {
		return q1.Equals(q2);
	}

	public static bool operator !=(in Quaternion q1, in Quaternion q2) {
		return !q1.Equals(q2);
	}

	public static Quaternion operator *(in Quaternion q1, in Quaternion q2) {
		return q1.Multiply(q2);
	}

	public static Quaternion operator *(in Quaternion q, float scalar) {
		return q.Multiply(scalar);
	}

	public static Quaternion operator *(float scalar, in Quaternion q) {
		return q.Multiply(scalar);
	}

	public static Quaternion operator /(in Quaternion q, float scalar) {
		return q * (1.0F / scalar);
	}

	public static Quaternion operator +(in Quaternion q1, in Quaternion q2) {
		return q1.Add(q2);
	}

	public static Quaternion operator -(in Quaternion q1, in Quaternion q2) {
		return q1.Subtract(q2);
	}

	public static Quaternion operator -(in Quaternion q) {
		return q.Negate();
	}
}
