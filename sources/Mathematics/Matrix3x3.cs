using Mino.Mathematics.Stereo;

namespace Mino.Mathematics;

/// <summary>
///     Immutable column major ordered matrix 3x3.
/// </summary>
public readonly struct Matrix3x3 {
	public static readonly Matrix3x3 Identity = new Matrix3x3();

	public readonly float M00 = 1.0F;
	public readonly float M10 = 0.0F;
	public readonly float M20 = 0.0F;
	public readonly float M01 = 0.0F;
	public readonly float M11 = 1.0F;
	public readonly float M21 = 0.0F;
	public readonly float M02 = 0.0F;
	public readonly float M12 = 0.0F;
	public readonly float M22 = 1.0F;

	public Matrix3x3() {
	}

	public Matrix3x3(float m00, float m10, float m20, float m01, float m11, float m21, float m02,
		float m12, float m22) {
		M00 = m00;
		M10 = m10;
		M20 = m20;
		M01 = m01;
		M11 = m11;
		M21 = m21;
		M02 = m02;
		M12 = m12;
		M22 = m22;
	}

	/// <summary>
	///     Gets the determinant of the matrix.
	/// </summary>
	public float Determinant {
		get => M00 * (M11 * M22 - M21 * M12)
			- M01 * (M10 * M22 - M20 * M12)
			+ M02 * (M10 * M21 - M20 * M11);
	}

	/// <summary>
	///     Multiplies this matrix by another matrix.
	/// </summary>
	/// <param name="other">The matrix to multiply by.</param>
	/// <returns>The product matrix.</returns>
	public Matrix3x3 Multiply(in Matrix3x3 other) {
		return new Matrix3x3(
			M00 * other.M00 + M01 * other.M10 + M02 * other.M20,
			M10 * other.M00 + M11 * other.M10 + M12 * other.M20,
			M20 * other.M00 + M21 * other.M10 + M22 * other.M20,
			M00 * other.M01 + M01 * other.M11 + M02 * other.M21,
			M10 * other.M01 + M11 * other.M11 + M12 * other.M21,
			M20 * other.M01 + M21 * other.M11 + M22 * other.M21,
			M00 * other.M02 + M01 * other.M12 + M02 * other.M22,
			M10 * other.M02 + M11 * other.M12 + M12 * other.M22,
			M20 * other.M02 + M21 * other.M12 + M22 * other.M22
		);
	}

	/// <summary>
	///     Transforms a point by this matrix (including perspective division).
	/// </summary>
	/// <param name="x">X coordinate of the point.</param>
	/// <param name="y">Y coordinate of the point.</param>
	/// <param name="z">Z coordinate of the point.</param>
	/// <param name="xo">Output X coordinate of the transformed point.</param>
	/// <param name="yo">Output Y coordinate of the transformed point.</param>
	/// <param name="zo">Output Z coordinate of the transformed point.</param>
	public void Transform(float x, float y, float z, out float xo, out float yo, out float zo) {
		xo = M00 * x + M01 * y + M02 * z;
		yo = M10 * x + M11 * y + M12 * z;
		zo = M20 * x + M21 * y + M22 * z;
	}

	/// <summary>
	///     Transforms a vector by this matrix.
	/// </summary>
	/// <param name="vec">The vector to transform.</param>
	/// <returns>The transformed vector.</returns>
	public Vector3 Transform(in Vector3 vec) {
		Transform(vec.X, vec.Y, vec.Z, out float xo, out float yo, out float zo);
		return new Vector3(xo, yo, zo);
	}

	/// <summary>
	///     Computes the inverse of this matrix.
	///     Returns identity if matrix is singular.
	/// </summary>
	/// <returns>The inverse matrix, or identity if singular.</returns>
	public Matrix3x3 Invert() {
		float det = Determinant;
		if (Comparison.DoEqual(0.0F, det)) {
			return Identity;
		}
		float invDet = 1.0F / det;
		return new Matrix3x3(
			(M11 * M22 - M21 * M12) * invDet,
			(M12 * M20 - M22 * M10) * invDet,
			(M10 * M21 - M20 * M11) * invDet,
			(M02 * M21 - M01 * M22) * invDet,
			(M00 * M22 - M02 * M20) * invDet,
			(M01 * M20 - M00 * M21) * invDet,
			(M01 * M12 - M02 * M11) * invDet,
			(M02 * M10 - M00 * M12) * invDet,
			(M00 * M11 - M01 * M10) * invDet
		);
	}

	/// <summary>
	///     Scales this matrix by the given factors.
	/// </summary>
	/// <param name="scalarX">Scaling factor for the X components.</param>
	/// <param name="scalarY">Scaling factor for the Y components.</param>
	/// <param name="scalarZ">Scaling factor for the Z components.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix3x3 Scale(float scalarX, float scalarY, float scalarZ) {
		return new Matrix3x3(
			M00 * scalarX, M10 * scalarX, M20 * scalarX,
			M01 * scalarY, M11 * scalarY, M21 * scalarY,
			M02 * scalarZ, M12 * scalarZ, M22 * scalarZ
		);
	}

	/// <summary>
	///     Scales this matrix uniformly.
	/// </summary>
	/// <param name="scalar">Uniform scaling factor.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix3x3 Scale(float scalar) {
		return Scale(scalar, scalar, scalar);
	}

	/// <summary>
	///     Scales this matrix by the given vector.
	/// </summary>
	/// <param name="scalar">The scaling vector.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix3x3 Scale(in Vector3 scalar) {
		return Scale(scalar.X, scalar.Y, scalar.Z);
	}

	/// <summary>
	///     Applies shear transformation to this matrix.
	/// </summary>
	/// <param name="xy">Shear factor for XY plane.</param>
	/// <param name="xz">Shear factor for XZ plane.</param>
	/// <param name="yx">Shear factor for YX plane.</param>
	/// <param name="yz">Shear factor for YZ plane.</param>
	/// <param name="zx">Shear factor for ZX plane.</param>
	/// <param name="zy">Shear factor for ZY plane.</param>
	/// <returns>The sheared matrix.</returns>
	public Matrix3x3 Shear(float xy, float xz, float yx, float yz, float zx, float zy) {
		return new Matrix3x3(
			M00 + M01 * xy + M02 * xz,
			M10 + M11 * xy + M12 * xz,
			M20 + M21 * xy + M22 * xz,
			M01 + M00 * yx + M02 * yz,
			M11 + M10 * yx + M12 * yz,
			M21 + M20 * yx + M22 * yz,
			M02 + M00 * zx + M01 * zy,
			M12 + M10 * zx + M11 * zy,
			M22 + M20 * zx + M21 * zy
		);
	}

	/// <summary>
	///     Rotates this matrix around the X axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>The rotated matrix.</returns>
	public Matrix3x3 RotateX(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix3x3(
			M00,
			M10 * cos + M20 * sin,
			M10 * -sin + M20 * cos,
			M01,
			M11 * cos + M21 * sin,
			M11 * -sin + M21 * cos,
			M02,
			M12 * cos + M22 * sin,
			M12 * -sin + M22 * cos
		);
	}

	/// <summary>
	///     Rotates this matrix around the Y axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>The rotated matrix.</returns>
	public Matrix3x3 RotateY(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix3x3(
			M00 * cos + M02 * -sin,
			M10 * cos + M12 * -sin,
			M20 * cos + M22 * -sin,
			M01,
			M11,
			M21,
			M00 * sin + M02 * cos,
			M10 * sin + M12 * cos,
			M20 * sin + M22 * cos
		);
	}

	/// <summary>
	///     Rotates this matrix around the Z axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>The rotated matrix.</returns>
	public Matrix3x3 RotateZ(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix3x3(
			M00 * cos + M01 * sin,
			M10 * cos + M11 * sin,
			M20 * cos + M21 * sin,
			M00 * -sin + M01 * cos,
			M10 * -sin + M11 * cos,
			M20 * -sin + M21 * cos,
			M02,
			M12,
			M22
		);
	}

	/// <summary>
	///     Rotates this matrix around an arbitrary axis.
	/// </summary>
	/// <param name="axis">The axis of rotation.</param>
	/// <param name="angle">Rotation angle in radians.</param>
	/// <returns>The rotated matrix.</returns>
	public Matrix3x3 Rotate(in Vector3 axis, float angle) {
		return Multiply(new Quaternion(axis, angle).ToMatrix3x3());
	}

	/// <summary>
	///     Returns the transpose of this matrix.
	/// </summary>
	/// <returns>The transposed matrix.</returns>
	public Matrix3x3 Transpose() {
		return new Matrix3x3(
			M00, M01, M02,
			M10, M11, M12,
			M20, M21, M22
		);
	}

	/// <summary>
	///     Converts to a 2x2 matrix (drops the Z components).
	/// </summary>
	/// <returns>A 2x2 matrix.</returns>
	public Matrix2x2 ToMatrix2x2() {
		return new Matrix2x2(
			M00, M10,
			M01, M11
		);
	}

	/// <summary>
	///     Converts to a 3x2 matrix (drops the third row).
	/// </summary>
	/// <returns>A 3x2 matrix.</returns>
	public Matrix3x2 ToMatrix3x2() {
		return new Matrix3x2(
			M00, M10,
			M01, M11,
			M02, M12
		);
	}

	/// <summary>
	///     Converts to a 4x4 matrix.
	/// </summary>
	/// <returns>A 4x4 matrix.</returns>
	public Matrix4x4 ToMatrix4x4() {
		return new Matrix4x4(
			M00, M10, M20, 0.0F,
			M01, M11, M21, 0.0F,
			M02, M12, M22, 0.0F,
			0.0F, 0.0F, 0.0F, 1.0F
		);
	}

	/// <summary>
	///     Creates a scaling matrix.
	/// </summary>
	/// <param name="scalarX">Scaling factor in the X direction.</param>
	/// <param name="scalarY">Scaling factor in the Y direction.</param>
	/// <param name="scalarZ">Scaling factor in the Z direction.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix3x3 CreateScale(float scalarX, float scalarY, float scalarZ) {
		return new Matrix3x3(
			scalarX, 0.0F, 0.0F,
			0.0F, scalarY, 0.0F,
			0.0F, 0.0F, scalarZ
		);
	}

	/// <summary>
	///     Creates a uniform scaling matrix.
	/// </summary>
	/// <param name="scalar">Uniform scaling factor.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix3x3 CreateScale(float scalar) {
		return CreateScale(scalar, scalar, scalar);
	}

	/// <summary>
	///     Creates a scaling matrix.
	/// </summary>
	/// <param name="scalar">The scaling vector.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix3x3 CreateScale(in Vector3 scalar) {
		return CreateScale(scalar.X, scalar.Y, scalar.Z);
	}

	/// <summary>
	///     Creates a rotation matrix around the X axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix3x3 CreateRotationX(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix3x3(
			1.0F, 0.0F, 0.0F,
			0.0F, cos, sin,
			0.0F, -sin, cos
		);
	}

	/// <summary>
	///     Creates a rotation matrix around the Y axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix3x3 CreateRotationY(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix3x3(
			cos, 0.0F, -sin,
			0.0F, 1.0F, 0.0F,
			sin, 0.0F, cos
		);
	}

	/// <summary>
	///     Creates a rotation matrix around the Z axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix3x3 CreateRotationZ(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix3x3(
			cos, sin, 0.0F,
			-sin, cos, 0.0F,
			0.0F, 0.0F, 1.0F
		);
	}

	/// <summary>
	///     Creates a rotation matrix around an arbitrary axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <param name="axis">The axis of rotation.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix3x3 CreateRotation(float rad, in Vector3 axis) {
		return new Quaternion(axis, rad).ToMatrix3x3();
	}

	/// <summary>
	///     Creates a matrix from a quaternion.
	/// </summary>
	/// <param name="q">The quaternion.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix3x3 CreateByQuaternion(in Quaternion q) {
		return q.ToMatrix3x3();
	}

	/// <summary>
	///     Creates a shear matrix.
	/// </summary>
	/// <param name="xy">Shear factor for XY plane.</param>
	/// <param name="xz">Shear factor for XZ plane.</param>
	/// <param name="yx">Shear factor for YX plane.</param>
	/// <param name="yz">Shear factor for YZ plane.</param>
	/// <param name="zx">Shear factor for ZX plane.</param>
	/// <param name="zy">Shear factor for ZY plane.</param>
	/// <returns>A shear matrix.</returns>
	public static Matrix3x3 CreateShear(float xy, float xz = 0.0F, float yx = 0.0F,
		float yz = 0.0F, float zx = 0.0F, float zy = 0.0F) {
		return new Matrix3x3(
			1.0F, yx, zx,
			xy, 1.0F, zy,
			xz, yz, 1.0F
		);
	}

	/// <summary>
	///     Creates a transformation matrix from translation, rotation, and scale.
	/// </summary>
	/// <param name="translation">The translation vector.</param>
	/// <param name="rotation">Rotation angle in radians.</param>
	/// <param name="scalar">Scaling vector.</param>
	/// <returns>A transformation matrix.</returns>
	public static Matrix3x3 CreateTransform(in Vector2 translation, float rotation,
		in Vector2 scalar) {
		FastTrigonometric.Get(rotation, out float sin, out float cos);

		return new Matrix3x3(
			scalar.X * cos, scalar.X * sin, 0.0F,
			scalar.Y * -sin, scalar.Y * cos, 0.0F,
			translation.X, translation.Y, 1.0F
		);
	}

	/// <summary>
	///     Creates an orthographic projection matrix for 2D.
	/// </summary>
	/// <param name="left">Left boundary of the viewport.</param>
	/// <param name="right">Right boundary of the viewport.</param>
	/// <param name="bottom">Bottom boundary of the viewport.</param>
	/// <param name="top">Top boundary of the viewport.</param>
	/// <returns>An orthographic projection matrix.</returns>
	public static Matrix3x3 CreateOrthographic(float left, float right, float bottom, float top) {
		float xo = 2.0F / (right - left);
		float yo = 2.0F / (top - bottom);

		return new Matrix3x3(
			xo, 0.0F, 0.0F,
			0.0F, yo, 0.0F,
			0.0F, 0.0F, 1.0F
		);
	}

	/// <summary>
	///     Linearly interpolates between two matrices.
	/// </summary>
	/// <param name="a">Start matrix.</param>
	/// <param name="b">End matrix.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>The interpolated matrix.</returns>
	public static Matrix3x3 Lerp(in Matrix3x3 a, in Matrix3x3 b, float t) {
		t = Math.Clamp(t, 0.0F, 1.0F);
		return new Matrix3x3(
			a.M00 + (b.M00 - a.M00) * t,
			a.M10 + (b.M10 - a.M10) * t,
			a.M20 + (b.M20 - a.M20) * t,
			a.M01 + (b.M01 - a.M01) * t,
			a.M11 + (b.M11 - a.M11) * t,
			a.M21 + (b.M21 - a.M21) * t,
			a.M02 + (b.M02 - a.M02) * t,
			a.M12 + (b.M12 - a.M12) * t,
			a.M22 + (b.M22 - a.M22) * t
		);
	}


	public static Matrix3x3 operator *(in Matrix3x3 a, in Matrix3x3 b) {
		return a.Multiply(b);
	}

	public static Vector3 operator *(in Matrix3x3 v, in Vector3 vec) {
		return v.Transform(vec);
	}

	public static Matrix3x3 operator *(in Matrix3x3 v, float scalar) {
		return v.Scale(scalar);
	}

	public static Matrix3x3 operator *(float scalar, in Matrix3x3 v) {
		return v.Scale(scalar);
	}

	public static Matrix3x3 operator /(in Matrix3x3 v, float scalar) {
		return v.Scale(1.0F / scalar);
	}

	public static Matrix3x3 operator -(in Matrix3x3 v) {
		return v.Scale(-1.0F);
	}
}
