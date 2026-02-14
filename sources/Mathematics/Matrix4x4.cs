using Mino.Mathematics.Spatial;

namespace Mino.Mathematics;

/// <summary>
///     Immutable column major ordered matrix 4x4.
/// </summary>
public readonly struct Matrix4x4 : Matrix<Matrix4x4> {
	public static readonly Matrix4x4 Identity = new Matrix4x4();

	public readonly float M00 = 1.0F;
	public readonly float M10 = 0.0F;
	public readonly float M20 = 0.0F;
	public readonly float M30 = 0.0F;
	public readonly float M01 = 0.0F;
	public readonly float M11 = 1.0F;
	public readonly float M21 = 0.0F;
	public readonly float M31 = 0.0F;
	public readonly float M02 = 0.0F;
	public readonly float M12 = 0.0F;
	public readonly float M22 = 1.0F;
	public readonly float M32 = 0.0F;
	public readonly float M03 = 0.0F;
	public readonly float M13 = 0.0F;
	public readonly float M23 = 0.0F;
	public readonly float M33 = 1.0F;

	public Matrix4x4() {
	}

	public Matrix4x4(
		float m00, float m10, float m20, float m30,
		float m01, float m11, float m21, float m31,
		float m02, float m12, float m22, float m32,
		float m03, float m13, float m23, float m33) {
		M00 = m00;
		M10 = m10;
		M20 = m20;
		M30 = m30;
		M01 = m01;
		M11 = m11;
		M21 = m21;
		M31 = m31;
		M02 = m02;
		M12 = m12;
		M22 = m22;
		M32 = m32;
		M03 = m03;
		M13 = m13;
		M23 = m23;
		M33 = m33;
	}

	/// <summary>
	///     Gets the determinant of the matrix.
	/// </summary>
	public float Determinant {
		get => M30 * M21 * M12 * M03 - M20 * M31 * M12 * M03
			- M30 * M11 * M22 * M03 + M10 * M31 * M22 * M03
			+ M20 * M11 * M32 * M03 - M10 * M21 * M32 * M03
			- M30 * M21 * M02 * M13 + M20 * M31 * M02 * M13
			+ M30 * M01 * M22 * M13 - M00 * M31 * M22 * M13
			- M20 * M01 * M32 * M13 + M00 * M21 * M32 * M13
			+ M30 * M11 * M02 * M23 - M10 * M31 * M02 * M23
			- M30 * M01 * M12 * M23 + M00 * M31 * M12 * M23
			+ M10 * M01 * M32 * M23 - M00 * M11 * M32 * M23
			- M20 * M11 * M02 * M33 + M10 * M21 * M02 * M33
			+ M20 * M01 * M12 * M33 - M00 * M21 * M12 * M33
			- M10 * M01 * M22 * M33 + M00 * M11 * M22 * M33;
	}

	/// <summary>
	///     Multiplies this matrix by another matrix.
	/// </summary>
	/// <param name="other">The matrix to multiply by.</param>
	/// <returns>The product matrix.</returns>
	public Matrix4x4 Multiply(in Matrix4x4 other) {
		return new Matrix4x4(
			M00 * other.M00 + M01 * other.M10 + M02 * other.M20 + M03 * other.M30,
			M10 * other.M00 + M11 * other.M10 + M12 * other.M20 + M13 * other.M30,
			M20 * other.M00 + M21 * other.M10 + M22 * other.M20 + M23 * other.M30,
			M30 * other.M00 + M31 * other.M10 + M32 * other.M20 + M33 * other.M30,
			M00 * other.M01 + M01 * other.M11 + M02 * other.M21 + M03 * other.M31,
			M10 * other.M01 + M11 * other.M11 + M12 * other.M21 + M13 * other.M31,
			M20 * other.M01 + M21 * other.M11 + M22 * other.M21 + M23 * other.M31,
			M30 * other.M01 + M31 * other.M11 + M32 * other.M21 + M33 * other.M31,
			M00 * other.M02 + M01 * other.M12 + M02 * other.M22 + M03 * other.M32,
			M10 * other.M02 + M11 * other.M12 + M12 * other.M22 + M13 * other.M32,
			M20 * other.M02 + M21 * other.M12 + M22 * other.M22 + M23 * other.M32,
			M30 * other.M02 + M31 * other.M12 + M32 * other.M22 + M33 * other.M32,
			M00 * other.M03 + M01 * other.M13 + M02 * other.M23 + M03 * other.M33,
			M10 * other.M03 + M11 * other.M13 + M12 * other.M23 + M13 * other.M33,
			M20 * other.M03 + M21 * other.M13 + M22 * other.M23 + M23 * other.M33,
			M30 * other.M03 + M31 * other.M13 + M32 * other.M23 + M33 * other.M33
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
		float w = M30 * x + M31 * y + M32 * z + M33;
		xo = (M00 * x + M01 * y + M02 * z + M03) / w;
		yo = (M10 * x + M11 * y + M12 * z + M13) / w;
		zo = (M20 * x + M21 * y + M22 * z + M23) / w;
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
	///     Transforms a 4D vector by this matrix.
	/// </summary>
	/// <param name="x">X coordinate of the vector.</param>
	/// <param name="y">Y coordinate of the vector.</param>
	/// <param name="z">Z coordinate of the vector.</param>
	/// <param name="w">W coordinate of the vector.</param>
	/// <param name="xo">Output X coordinate of the transformed vector.</param>
	/// <param name="yo">Output Y coordinate of the transformed vector.</param>
	/// <param name="zo">Output Z coordinate of the transformed vector.</param>
	/// <param name="wo">Output W coordinate of the transformed vector.</param>
	public void Transform(float x, float y, float z, float w,
		out float xo, out float yo, out float zo, out float wo) {
		xo = M00 * x + M01 * y + M02 * z + M03 * w;
		yo = M10 * x + M11 * y + M12 * z + M13 * w;
		zo = M20 * x + M21 * y + M22 * z + M23 * w;
		wo = M30 * x + M31 * y + M32 * z + M33 * w;
	}

	/// <summary>
	///     Transforms a 4D vector by this matrix.
	/// </summary>
	/// <param name="vec">The 4D vector to transform.</param>
	/// <returns>The transformed 4D vector.</returns>
	public Vector4 Transform(in Vector4 vec) {
		Transform(vec.X, vec.Y, vec.Z, vec.W, out float xo, out float yo, out float zo, out float wo);
		return new Vector4(xo, yo, zo, wo);
	}

	/// <summary>
	///     Computes the inverse of this matrix.
	///     Returns identity if matrix is singular.
	/// </summary>
	/// <returns>The inverse matrix, or identity if singular.</returns>
	public Matrix4x4 Invert() {
		float det = Determinant;
		if (Comparison.DoEqual(0.0F, det)) {
			return Identity;
		}
		float invDet = 1.0F / det;
		float m00 = M12 * M23 * M31 - M13 * M22 * M31 + M13 * M21 * M32 - M11 * M23 * M32
			- M12 * M21 * M33 + M11 * M22 * M33;
		float m01 = M03 * M22 * M31 - M02 * M23 * M31 - M03 * M21 * M32 + M01 * M23 * M32
			+ M02 * M21 * M33 - M01 * M22 * M33;
		float m02 = M02 * M13 * M31 - M03 * M12 * M31 + M03 * M11 * M32 - M01 * M13 * M32
			- M02 * M11 * M33 + M01 * M12 * M33;
		float m03 = M03 * M12 * M21 - M02 * M13 * M21 - M03 * M11 * M22 + M01 * M13 * M22
			+ M02 * M11 * M23 - M01 * M12 * M23;
		float m10 = M13 * M22 * M30 - M12 * M23 * M30 - M13 * M20 * M32 + M10 * M23 * M32
			+ M12 * M20 * M33 - M10 * M22 * M33;
		float m11 = M02 * M23 * M30 - M03 * M22 * M30 + M03 * M20 * M32 - M00 * M23 * M32
			- M02 * M20 * M33 + M00 * M22 * M33;
		float m12 = M03 * M12 * M30 - M02 * M13 * M30 - M03 * M10 * M32 + M00 * M13 * M32
			+ M02 * M10 * M33 - M00 * M12 * M33;
		float m13 = M02 * M13 * M20 - M03 * M12 * M20 + M03 * M10 * M22 - M00 * M13 * M22
			- M02 * M10 * M23 + M00 * M12 * M23;
		float m20 = M11 * M23 * M30 - M13 * M21 * M30 + M13 * M20 * M31 - M10 * M23 * M31
			- M11 * M20 * M33 + M10 * M21 * M33;
		float m21 = M03 * M21 * M30 - M01 * M23 * M30 - M03 * M20 * M31 + M00 * M23 * M31
			+ M01 * M20 * M33 - M00 * M21 * M33;
		float m22 = M01 * M13 * M30 - M03 * M11 * M30 + M03 * M10 * M31 - M00 * M13 * M31
			- M01 * M10 * M33 + M00 * M11 * M33;
		float m23 = M03 * M11 * M20 - M01 * M13 * M20 - M03 * M10 * M21 + M00 * M13 * M21
			+ M01 * M10 * M23 - M00 * M11 * M23;
		float m30 = M12 * M21 * M30 - M11 * M22 * M30 - M12 * M20 * M31 + M10 * M22 * M31
			+ M11 * M20 * M32 - M10 * M21 * M32;
		float m31 = M01 * M22 * M30 - M02 * M21 * M30 + M02 * M20 * M31 - M00 * M22 * M31
			- M01 * M20 * M32 + M00 * M21 * M32;
		float m32 = M02 * M11 * M30 - M01 * M12 * M30 - M02 * M10 * M31 + M00 * M12 * M31
			+ M01 * M10 * M32 - M00 * M11 * M32;
		float m33 = M01 * M12 * M20 - M02 * M11 * M20 + M02 * M10 * M21 - M00 * M12 * M21
			- M01 * M10 * M22 + M00 * M11 * M22;
		return new Matrix4x4(
			m00 * invDet, m10 * invDet, m20 * invDet, m30 * invDet,
			m01 * invDet, m11 * invDet, m21 * invDet, m31 * invDet,
			m02 * invDet, m12 * invDet, m22 * invDet, m32 * invDet,
			m03 * invDet, m13 * invDet, m23 * invDet, m33 * invDet
		);
	}

	/// <summary>
	///     Translates the matrix by the specified amounts.
	/// </summary>
	/// <param name="x">Translation in X direction.</param>
	/// <param name="y">Translation in Y direction.</param>
	/// <param name="z">Translation in Z direction.</param>
	/// <returns>The translated matrix.</returns>
	public Matrix4x4 Translate(float x, float y, float z) {
		return new Matrix4x4(
			M00, M10, M20, M30,
			M01, M11, M21, M31,
			M02, M12, M22, M32,
			M03 + M00 * x + M01 * y + M02 * z,
			M13 + M10 * x + M11 * y + M12 * z,
			M23 + M20 * x + M21 * y + M22 * z,
			M33 + M30 * x + M31 * y + M32 * z
		);
	}

	/// <summary>
	///     Translates the matrix by the specified vector.
	/// </summary>
	/// <param name="translation">The translation vector.</param>
	/// <returns>The translated matrix.</returns>
	public Matrix4x4 Translate(Vector3 translation) {
		return Translate(translation.X, translation.Y, translation.Z);
	}

	/// <summary>
	///     Scales this matrix by the given factors.
	/// </summary>
	/// <param name="scalarX">Scaling factor for the X direction.</param>
	/// <param name="scalarY">Scaling factor for the Y direction.</param>
	/// <param name="scalarZ">Scaling factor for the Z direction.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix4x4 Scale(float scalarX, float scalarY, float scalarZ) {
		return new Matrix4x4(
			M00 * scalarX, M10 * scalarX, M20 * scalarX, M30 * scalarX,
			M01 * scalarY, M11 * scalarY, M21 * scalarY, M31 * scalarY,
			M02 * scalarZ, M12 * scalarZ, M22 * scalarZ, M32 * scalarZ,
			M03, M13, M23, M33
		);
	}

	/// <summary>
	///     Scales this matrix uniformly.
	/// </summary>
	/// <param name="scalar">Uniform scaling factor.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix4x4 Scale(float scalar) {
		return Scale(scalar, scalar, scalar);
	}

	/// <summary>
	///     Scales this matrix by the given vector.
	/// </summary>
	/// <param name="scalar">The scaling vector.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix4x4 Scale(Vector3 scalar) {
		return Scale(scalar.X, scalar.Y, scalar.Z);
	}

	/// <summary>
	///     Rotates this matrix around an arbitrary axis.
	/// </summary>
	/// <param name="axis">The axis of rotation.</param>
	/// <param name="angle">Rotation angle in radians.</param>
	/// <returns>The rotated matrix.</returns>
	public Matrix4x4 Rotate(Vector3 axis, float angle) {
		return Multiply(new Quaternion(axis, angle).ToMatrix4x4());
	}

	/// <summary>
	///     Rotates this matrix around the X axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>The rotated matrix.</returns>
	public Matrix4x4 RotateX(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix4x4(
			M00, M10, M20, M30,
			M01 * cos + M02 * sin,
			M11 * cos + M12 * sin,
			M21 * cos + M22 * sin,
			M31 * cos + M32 * sin,
			M01 * -sin + M02 * cos,
			M11 * -sin + M12 * cos,
			M21 * -sin + M22 * cos,
			M31 * -sin + M32 * cos,
			M03, M13, M23, M33
		);
	}

	/// <summary>
	///     Rotates this matrix around the Y axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>The rotated matrix.</returns>
	public Matrix4x4 RotateY(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix4x4(
			M00 * cos + M02 * -sin,
			M10 * cos + M12 * -sin,
			M20 * cos + M22 * -sin,
			M30 * cos + M32 * -sin,
			M01, M11, M21, M31,
			M00 * sin + M02 * cos,
			M10 * sin + M12 * cos,
			M20 * sin + M22 * cos,
			M30 * sin + M32 * cos,
			M03, M13, M23, M33
		);
	}

	/// <summary>
	///     Rotates this matrix around the Z axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>The rotated matrix.</returns>
	public Matrix4x4 RotateZ(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix4x4(
			M00 * cos + M01 * sin,
			M10 * cos + M11 * sin,
			M20 * cos + M21 * sin,
			M30 * cos + M31 * sin,
			M00 * -sin + M01 * cos,
			M10 * -sin + M11 * cos,
			M20 * -sin + M21 * cos,
			M30 * -sin + M31 * cos,
			M02, M12, M22, M32,
			M03, M13, M23, M33
		);
	}

	/// <summary>
	///     Returns the transpose of this matrix.
	/// </summary>
	/// <returns>The transposed matrix.</returns>
	public Matrix4x4 Transpose() {
		return new Matrix4x4(
			M00, M01, M02, M03,
			M10, M11, M12, M13,
			M20, M21, M22, M23,
			M30, M31, M32, M33
		);
	}

	/// <summary>
	///     Converts to a 2x2 matrix (extracts the top-left 2x2 submatrix).
	/// </summary>
	/// <returns>A 2x2 matrix.</returns>
	public Matrix2x2 ToMatrix2x2() {
		return new Matrix2x2(
			M00, M10,
			M01, M11
		);
	}

	/// <summary>
	///     Converts to a 3x3 matrix (extracts the top-left 3x3 submatrix).
	/// </summary>
	/// <returns>A 3x3 matrix.</returns>
	public Matrix3x3 ToMatrix3x3() {
		return new Matrix3x3(
			M00, M10, M20,
			M01, M11, M21,
			M02, M12, M22
		);
	}

	/// <summary>
	///     Converts to a 3x2 matrix (extracts the top-left 3x2 submatrix and translation).
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
	///     Creates a translation matrix.
	/// </summary>
	/// <param name="x">Translation in X direction.</param>
	/// <param name="y">Translation in Y direction.</param>
	/// <param name="z">Translation in Z direction.</param>
	/// <returns>A translation matrix.</returns>
	public static Matrix4x4 CreateTranslation(float x, float y, float z) {
		return new Matrix4x4(
			1.0F, 0.0F, 0.0F, 0.0F,
			0.0F, 1.0F, 0.0F, 0.0F,
			0.0F, 0.0F, 1.0F, 0.0F,
			x, y, z, 1.0F
		);
	}

	/// <summary>
	///     Creates a translation matrix.
	/// </summary>
	/// <param name="translation">The translation vector.</param>
	/// <returns>A translation matrix.</returns>
	public static Matrix4x4 CreateTranslation(in Vector3 translation) {
		return CreateTranslation(translation.X, translation.Y, translation.Z);
	}

	/// <summary>
	///     Creates a scaling matrix.
	/// </summary>
	/// <param name="scalarX">Scaling factor in the X direction.</param>
	/// <param name="scalarY">Scaling factor in the Y direction.</param>
	/// <param name="scalarZ">Scaling factor in the Z direction.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix4x4 CreateScale(float scalarX, float scalarY, float scalarZ) {
		return new Matrix4x4(
			scalarX, 0.0F, 0.0F, 0.0F,
			0.0F, scalarY, 0.0F, 0.0F,
			0.0F, 0.0F, scalarZ, 0.0F,
			0.0F, 0.0F, 0.0F, 1.0F
		);
	}

	/// <summary>
	///     Creates a uniform scaling matrix.
	/// </summary>
	/// <param name="scalar">Uniform scaling factor.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix4x4 CreateScale(float scalar) {
		return CreateScale(scalar, scalar, scalar);
	}

	/// <summary>
	///     Creates a scaling matrix.
	/// </summary>
	/// <param name="scalar">The scaling vector.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix4x4 CreateScale(in Vector3 scalar) {
		return CreateScale(scalar.X, scalar.Y, scalar.Z);
	}

	/// <summary>
	///     Creates a scaling matrix centered at a specific point.
	/// </summary>
	/// <param name="scalarX">Scaling factor in the X direction.</param>
	/// <param name="scalarY">Scaling factor in the Y direction.</param>
	/// <param name="scalarZ">Scaling factor in the Z direction.</param>
	/// <param name="center">The center point of scaling.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix4x4 CreateScale(float scalarX, float scalarY, float scalarZ,
		in Vector3 center) {
		return CreateTranslation(-center.X, -center.Y, -center.Z)
			* CreateScale(scalarX, scalarY, scalarZ)
			* CreateTranslation(center.X, center.Y, center.Z);
	}

	/// <summary>
	///     Creates a rotation matrix around the X axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix4x4 CreateRotationX(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix4x4(
			1.0F, 0.0F, 0.0F, 0.0F,
			0.0F, cos, sin, 0.0F,
			0.0F, -sin, cos, 0.0F,
			0.0F, 0.0F, 0.0F, 1.0F
		);
	}

	/// <summary>
	///     Creates a rotation matrix around the Y axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix4x4 CreateRotationY(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix4x4(
			cos, 0.0F, -sin, 0.0F,
			0.0F, 1.0F, 0.0F, 0.0F,
			sin, 0.0F, cos, 0.0F,
			0.0F, 0.0F, 0.0F, 1.0F
		);
	}

	/// <summary>
	///     Creates a rotation matrix around the Z axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix4x4 CreateRotationZ(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);

		return new Matrix4x4(
			cos, sin, 0.0F, 0.0F,
			-sin, cos, 0.0F, 0.0F,
			0.0F, 0.0F, 1.0F, 0.0F,
			0.0F, 0.0F, 0.0F, 1.0F
		);
	}

	/// <summary>
	///     Creates a rotation matrix around an arbitrary axis.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <param name="axis">The axis of rotation.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix4x4 CreateRotation(float rad, in Vector3 axis) {
		return new Quaternion(axis, rad).ToMatrix4x4();
	}

	/// <summary>
	///     Creates a rotation matrix from Euler angles (pitch, yaw, roll).
	/// </summary>
	/// <param name="pitch">Rotation around the X axis.</param>
	/// <param name="yaw">Rotation around the Y axis.</param>
	/// <param name="roll">Rotation around the Z axis.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix4x4 CreateRotation(float pitch, float yaw, float roll) {
		return CreateRotationX(pitch) * CreateRotationY(yaw) * CreateRotationZ(roll);
	}

	/// <summary>
	///     Creates a rotation matrix from Euler angles.
	/// </summary>
	/// <param name="euler">Euler angles vector (pitch, yaw, roll).</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix4x4 CreateRotation(in Vector3 euler) {
		return CreateRotation(euler.X, euler.Y, euler.Z);
	}

	/// <summary>
	///     Creates a matrix from a quaternion.
	/// </summary>
	/// <param name="q">The quaternion.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix4x4 CreateByQuaternion(in Quaternion q) {
		return q.ToMatrix4x4();
	}

	/// <summary>
	///     Creates a transformation matrix from translation, rotation, and scale.
	/// </summary>
	/// <param name="translation">The translation vector.</param>
	/// <param name="rotation">The rotation quaternion.</param>
	/// <param name="scalar">The scaling vector.</param>
	/// <returns>A transformation matrix.</returns>
	public static Matrix4x4 CreateTransform(in Vector3 translation, in Quaternion rotation,
		in Vector3 scalar) {
		return CreateScale(scalar) * rotation.ToMatrix4x4() * CreateTranslation(translation);
	}

	/// <summary>
	///     Creates a transformation matrix from translation, Euler angles, and scale.
	/// </summary>
	/// <param name="translation">The translation vector.</param>
	/// <param name="eulerAngles">Euler angles vector (pitch, yaw, roll).</param>
	/// <param name="scalar">The scaling vector.</param>
	/// <returns>A transformation matrix.</returns>
	public static Matrix4x4 CreateTransform(in Vector3 translation, in Euler eulerAngles,
		in Vector3 scalar) {
		return CreateTransform(translation, Quaternion.CreateFromEuler(eulerAngles), scalar);
	}

	/// <summary>
	///     Creates a frustum projection matrix.
	/// </summary>
	/// <param name="left">Left clipping plane.</param>
	/// <param name="right">Right clipping plane.</param>
	/// <param name="bottom">Bottom clipping plane.</param>
	/// <param name="top">Top clipping plane.</param>
	/// <param name="near">Near clipping plane distance.</param>
	/// <param name="far">Far clipping plane distance.</param>
	/// <returns>A frustum projection matrix.</returns>
	public static Matrix4x4 CreateFrustum(float left, float right, float bottom, float top,
		float near, float far) {
		float x = 2.0F * near / (right - left);
		float y = 2.0F * near / (top - bottom);
		float a = (right + left) / (right - left);
		float b = (top + bottom) / (top - bottom);
		float c = -(far + near) / (far - near);
		float d = -(2.0F * far * near) / (far - near);

		return new Matrix4x4(
			x, 0.0F, 0.0F, 0.0F,
			0.0F, y, 0.0F, 0.0F,
			a, b, c, -1.0F,
			0.0F, 0.0F, d, 0.0F
		);
	}

	/// <summary>
	///     Creates a view (look-at) matrix.
	/// </summary>
	/// <param name="eye">Position of the camera.</param>
	/// <param name="target">Position the camera is looking at.</param>
	/// <param name="up">Up direction vector.</param>
	/// <returns>A view matrix.</returns>
	public static Matrix4x4 CreateLookAt(in Vector3 eye, in Vector3 target, in Vector3 up) {
		Vector3 zAxis = -(target - eye).Normalize(); // -Z forward.
		Vector3 xAxis = up.Cross(zAxis).Normalize();
		Vector3 yAxis = zAxis.Cross(xAxis);

		return new Matrix4x4(
			xAxis.X, yAxis.X, zAxis.X, 0.0F,
			xAxis.Y, yAxis.Y, zAxis.Y, 0.0F,
			xAxis.Z, yAxis.Z, zAxis.Z, 0.0F,
			-xAxis.Dot(eye), -yAxis.Dot(eye), -zAxis.Dot(eye), 1.0F
		);
	}

	/// <summary>
	///     Creates a view (look-at) matrix.
	/// </summary>
	/// <param name="position">Position of the camera.</param>
	/// <param name="orientation">Orientation of the camera.</param>
	/// <param name="baseFacing">Base facing of the world.</param>
	/// <param name="up">Up direction vector.</param>
	/// <returns>A view matrix.</returns>
	public static Matrix4x4 CreateLookAt(in Vector3 position, in Quaternion orientation, in Vector3 baseFacing,
		in Vector3 up) {
		Vector3 forward = orientation.Rotate(baseFacing);
		return CreateLookAt(position, position + forward, up);
	}

	/// <summary>
	///     Creates a viewport transformation matrix.
	/// </summary>
	/// <param name="x">X coordinate of the viewport.</param>
	/// <param name="y">Y coordinate of the viewport.</param>
	/// <param name="width">Width of the viewport.</param>
	/// <param name="height">Height of the viewport.</param>
	/// <param name="minDepth">Minimum depth value (default 0.0).</param>
	/// <param name="maxDepth">Maximum depth value (default 1.0).</param>
	/// <returns>A viewport matrix.</returns>
	public static Matrix4x4 CreateViewport(float x, float y, float width, float height,
		float minDepth = 0.0F,
		float maxDepth = 1.0F) {
		float halfWidth = width * 0.5F;
		float halfHeight = height * 0.5F;

		return new Matrix4x4(
			halfWidth, 0.0F, 0.0F, 0.0F,
			0.0F, -halfHeight, 0.0F, 0.0F,
			0.0F, 0.0F, maxDepth - minDepth, 0.0F,
			x + halfWidth, y + halfHeight, minDepth, 1.0F
		);
	}

	/// <summary>
	///     Creates an orthographic projection matrix.
	/// </summary>
	/// <param name="left">Left clipping plane.</param>
	/// <param name="right">Right clipping plane.</param>
	/// <param name="bottom">Bottom clipping plane.</param>
	/// <param name="top">Top clipping plane.</param>
	/// <param name="near">Near clipping plane distance.</param>
	/// <param name="far">Far clipping plane distance.</param>
	/// <returns>An orthographic projection matrix.</returns>
	public static Matrix4x4 CreateOrthographic(float left, float right, float bottom, float top,
		float near,
		float far) {
		float xo = 2.0F / (right - left);
		float yo = 2.0F / (top - bottom);
		float zo = -2.0F / (far - near);
		float tx = -(right + left) / (right - left);
		float ty = -(top + bottom) / (top - bottom);
		float tz = -(far + near) / (far - near);

		return new Matrix4x4(
			xo, 0.0F, 0.0F, 0.0F,
			0.0F, yo, 0.0F, 0.0F,
			0.0F, 0.0F, zo, 0.0F,
			tx, ty, tz, 1.0F
		);
	}

	/// <summary>
	///     Creates a perspective projection matrix.
	/// </summary>
	/// <param name="fovY">Vertical field of view in radians.</param>
	/// <param name="aspect">Aspect ratio (width/height).</param>
	/// <param name="near">Near clipping plane distance.</param>
	/// <param name="far">Far clipping plane distance.</param>
	/// <returns>A perspective projection matrix.</returns>
	public static Matrix4x4 CreatePerspective(float fovY, float aspect, float near, float far) {
		float tanHalfFov = MathF.Tan(fovY * 0.5F);
		float range = near - far;

		return new Matrix4x4(
			1.0F / (aspect * tanHalfFov), 0.0F, 0.0F, 0.0F,
			0.0F, 1.0F / tanHalfFov, 0.0F, 0.0F,
			0.0F, 0.0F, (far + near) / range, -1.0F,
			0.0F, 0.0F, 2.0F * far * near / range, 0.0F
		);
	}

	/// <summary>
	///     Linearly interpolates between two matrices.
	/// </summary>
	/// <param name="a">Start matrix.</param>
	/// <param name="b">End matrix.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>The interpolated matrix.</returns>
	public static Matrix4x4 Lerp(in Matrix4x4 a, in Matrix4x4 b, float t) {
		return new Matrix4x4(
			a.M00 + (b.M00 - a.M00) * t,
			a.M10 + (b.M10 - a.M10) * t,
			a.M20 + (b.M20 - a.M20) * t,
			a.M30 + (b.M30 - a.M30) * t,
			a.M01 + (b.M01 - a.M01) * t,
			a.M11 + (b.M11 - a.M11) * t,
			a.M21 + (b.M21 - a.M21) * t,
			a.M31 + (b.M31 - a.M31) * t,
			a.M02 + (b.M02 - a.M02) * t,
			a.M12 + (b.M12 - a.M12) * t,
			a.M22 + (b.M22 - a.M22) * t,
			a.M32 + (b.M32 - a.M32) * t,
			a.M03 + (b.M03 - a.M03) * t,
			a.M13 + (b.M13 - a.M13) * t,
			a.M23 + (b.M23 - a.M23) * t,
			a.M33 + (b.M33 - a.M33) * t
		);
	}

	public static Matrix4x4 operator *(in Matrix4x4 a, in Matrix4x4 b) {
		return a.Multiply(b);
	}

	public static Vector3 operator *(in Matrix4x4 v, in Vector3 vec) {
		return v.Transform(vec);
	}

	public static Vector4 operator *(in Matrix4x4 v, in Vector4 vec) {
		return v.Transform(vec);
	}

	public static Matrix4x4 operator *(in Matrix4x4 v, float scalar) {
		return v.Scale(scalar);
	}

	public static Matrix4x4 operator *(float scalar, in Matrix4x4 v) {
		return v.Scale(scalar);
	}

	public static Matrix4x4 operator /(in Matrix4x4 v, float scalar) {
		return v.Scale(1.0F / scalar);
	}

	public static Matrix4x4 operator -(in Matrix4x4 v) {
		return v.Scale(-1.0F);
	}
}
