namespace Mino.Mathematics;

/// <summary>
///     Immutable column major ordered matrix 2x2.
/// </summary>
public readonly struct Matrix2x2 : Matrix<Matrix2x2> {
	public static readonly Matrix2x2 Identity = new Matrix2x2();

	public readonly float M00 = 1.0F;
	public readonly float M10 = 0.0F;
	public readonly float M01 = 0.0F;
	public readonly float M11 = 1.0F;

	public Matrix2x2() {
	}

	public Matrix2x2(float m00, float m10, float m01, float m11) {
		M00 = m00;
		M10 = m10;
		M01 = m01;
		M11 = m11;
	}

	/// <summary>
	///     Gets the determinant of the matrix.
	/// </summary>
	public float Determinant {
		get => M00 * M11 - M01 * M10;
	}

	/// <summary>
	///     Multiplies this matrix by another matrix.
	/// </summary>
	/// <param name="other">The matrix to multiply by.</param>
	/// <returns>The product matrix.</returns>
	public Matrix2x2 Multiply(in Matrix2x2 other) {
		return new Matrix2x2(
			M00 * other.M00 + M01 * other.M10,
			M10 * other.M00 + M11 * other.M10,
			M00 * other.M01 + M01 * other.M11,
			M10 * other.M01 + M11 * other.M11
		);
	}

	/// <summary>
	///     Transforms a vector by this matrix.
	/// </summary>
	/// <param name="vector">The vector to transform.</param>
	/// <returns>The transformed vector.</returns>
	public Vector2 Transform(in Vector2 vector) {
		return new Vector2(
			M00 * vector.X + M01 * vector.Y,
			M10 * vector.X + M11 * vector.Y
		);
	}

	/// <summary>
	///     Transforms a vector by this matrix.
	/// </summary>
	/// <param name="x">X coordinate of the vector.</param>
	/// <param name="y">Y coordinate of the vector.</param>
	/// <param name="xo">Output X coordinate of the transformed vector.</param>
	/// <param name="yo">Output Y coordinate of the transformed vector.</param>
	public void Transform(float x, float y, out float xo, out float yo) {
		xo = M00 * x + M01 * y;
		yo = M10 * x + M11 * y;
	}

	/// <summary>
	///     Computes the inverse of this matrix.
	///     Returns identity if matrix is singular.
	/// </summary>
	/// <returns>The inverse matrix, or identity if singular.</returns>
	public Matrix2x2 Invert() {
		float det = Determinant;
		if (Comparison.DoEqual(0.0F, det)) {
			return Identity;
		}
		float invDet = 1.0F / det;
		return new Matrix2x2(
			invDet * M11,
			invDet * -M10,
			invDet * -M01,
			invDet * M00
		);
	}

	/// <summary>
	///     Returns the transpose of this matrix.
	/// </summary>
	/// <returns>The transposed matrix.</returns>
	public Matrix2x2 Transpose() {
		return new Matrix2x2(
			M00, M01,
			M10, M11
		);
	}

	/// <summary>
	///     Scales this matrix by the given scalar.
	/// </summary>
	/// <param name="scalar">The scalar to multiply by.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix2x2 Scale(float scalar) {
		return new Matrix2x2(
			M00 * scalar, M10 * scalar,
			M01 * scalar, M11 * scalar
		);
	}

	/// <summary>
	///     Scales this matrix by the given vector components.
	/// </summary>
	/// <param name="scalarX">Scaling factor for the X components.</param>
	/// <param name="scalarY">Scaling factor for the Y components.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix2x2 Scale(float scalarX, float scalarY) {
		return new Matrix2x2(
			M00 * scalarX, M10 * scalarX,
			M01 * scalarY, M11 * scalarY
		);
	}

	/// <summary>
	///     Scales this matrix by the given vector.
	/// </summary>
	/// <param name="scalar">The vector containing scaling factors.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix2x2 Scale(in Vector2 scalar) {
		return Scale(scalar.X, scalar.Y);
	}

	/// <summary>
	///     Applies shear transformation to this matrix.
	/// </summary>
	/// <param name="shearX">Shear factor along the X axis.</param>
	/// <param name="shearY">Shear factor along the Y axis.</param>
	/// <returns>The sheared matrix.</returns>
	public Matrix2x2 Shear(float shearX, float shearY) {
		return new Matrix2x2(
			M00 + shearY * M01,
			M10 + shearY * M11,
			M01 + shearX * M00,
			M11 + shearX * M10
		);
	}

	/// <summary>
	///     Applies shear transformation to this matrix.
	/// </summary>
	/// <param name="shear">The shear vector.</param>
	/// <returns>The sheared matrix.</returns>
	public Matrix2x2 Shear(in Vector2 shear) {
		return Shear(shear.X, shear.Y);
	}

	/// <summary>
	///     Applies rotation transformation to this matrix.
	/// </summary>
	/// <param name="radians">Rotation angle in radians.</param>
	/// <returns>The rotated matrix.</returns>
	public Matrix2x2 Rotate(float radians) {
		FastTrigonometric.Get(radians, out float sin, out float cos);
		return new Matrix2x2(
			M00 * cos + M01 * sin,
			M10 * cos + M11 * sin,
			M00 * -sin + M01 * cos,
			M10 * -sin + M11 * cos
		);
	}

	/// <summary>
	///     Converts to a 3x2 matrix by adding translation column.
	/// </summary>
	/// <param name="translationX">Translation in X direction.</param>
	/// <param name="translationY">Translation in Y direction.</param>
	/// <returns>A 3x2 matrix.</returns>
	public Matrix3x2 ToMatrix3x2(float translationX = 0.0F, float translationY = 0.0F) {
		return new Matrix3x2(
			M00, M10,
			M01, M11,
			translationX, translationY
		);
	}

	/// <summary>
	///     Converts to a 3x2 matrix by adding translation vector.
	/// </summary>
	/// <param name="translation">The translation vector.</param>
	/// <returns>A 3x2 matrix.</returns>
	public Matrix3x2 ToMatrix3x2(in Vector2 translation) {
		return ToMatrix3x2(translation.X, translation.Y);
	}

	/// <summary>
	///     Converts to a 3x3 matrix.
	/// </summary>
	/// <returns>A 3x3 matrix.</returns>
	public Matrix3x3 ToMatrix3x3() {
		return new Matrix3x3(
			M00, M10, 0.0F,
			M01, M11, 0.0F,
			0.0F, 0.0F, 1.0F
		);
	}

	/// <summary>
	///     Converts to a 4x4 matrix.
	/// </summary>
	/// <returns>A 4x4 matrix.</returns>
	public Matrix4x4 ToMatrix4x4() {
		return new Matrix4x4(
			M00, M10, 0.0F, 0.0F,
			M01, M11, 0.0F, 0.0F,
			0.0F, 0.0F, 1.0F, 0.0F,
			0.0F, 0.0F, 0.0F, 1.0F
		);
	}

	/// <summary>
	///     Creates a scaling matrix.
	/// </summary>
	/// <param name="scalar">Uniform scaling factor.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix2x2 CreateScale(float scalar) {
		return new Matrix2x2(scalar, 0.0F, 0.0F, scalar);
	}

	/// <summary>
	///     Creates a scaling matrix.
	/// </summary>
	/// <param name="scalarX">Scaling factor in the X direction.</param>
	/// <param name="scalarY">Scaling factor in the Y direction.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix2x2 CreateScale(float scalarX, float scalarY) {
		return new Matrix2x2(scalarX, 0.0F, 0.0F, scalarY);
	}

	/// <summary>
	///     Creates a scaling matrix.
	/// </summary>
	/// <param name="scalar">The scaling vector.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix2x2 CreateScale(in Vector2 scalar) {
		return CreateScale(scalar.X, scalar.Y);
	}

	/// <summary>
	///     Creates a rotation matrix.
	/// </summary>
	/// <param name="radians">Rotation angle in radians.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix2x2 CreateRotation(float radians) {
		FastTrigonometric.Get(radians, out float sin, out float cos);
		return new Matrix2x2(cos, sin, -sin, cos);
	}

	/// <summary>
	///     Creates a shear matrix.
	/// </summary>
	/// <param name="shearX">Shear factor along the X axis.</param>
	/// <param name="shearY">Shear factor along the Y axis.</param>
	/// <returns>A shear matrix.</returns>
	public static Matrix2x2 CreateShear(float shearX, float shearY) {
		return new Matrix2x2(1.0F, shearY, shearX, 1.0F);
	}

	/// <summary>
	///     Creates a shear matrix.
	/// </summary>
	/// <param name="shear">The shear vector.</param>
	/// <returns>A shear matrix.</returns>
	public static Matrix2x2 CreateShear(in Vector2 shear) {
		return CreateShear(shear.X, shear.Y);
	}

	/// <summary>
	///     Creates a reflection matrix about the X-axis.
	/// </summary>
	/// <returns>A reflection matrix about the X-axis.</returns>
	public static Matrix2x2 CreateReflectionX() {
		return new Matrix2x2(-1.0F, 0.0F, 0.0F, 1.0F);
	}

	/// <summary>
	///     Creates a reflection matrix about the Y-axis.
	/// </summary>
	/// <returns>A reflection matrix about the Y-axis.</returns>
	public static Matrix2x2 CreateReflectionY() {
		return new Matrix2x2(1.0F, 0.0F, 0.0F, -1.0F);
	}

	/// <summary>
	///     Creates a reflection matrix about a line through the origin with given angle.
	/// </summary>
	/// <param name="angle">Angle of the reflection line in radians.</param>
	/// <returns>A reflection matrix.</returns>
	public static Matrix2x2 CreateReflection(float angle) {
		FastTrigonometric.Get(angle * 2.0F, out float sin, out float cos);
		return new Matrix2x2(cos, sin, sin, -cos);
	}

	/// <summary>
	///     Creates a matrix from scale and rotation.
	/// </summary>
	/// <param name="rotation">Rotation angle in radians.</param>
	/// <param name="scale">Scaling vector.</param>
	/// <returns>A transformation matrix.</returns>
	public static Matrix2x2 CreateTransform(float rotation, in Vector2 scale) {
		FastTrigonometric.Get(rotation, out float sin, out float cos);
		return new Matrix2x2(
			scale.X * cos, scale.X * sin,
			scale.Y * -sin, scale.Y * cos
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
	public static Matrix2x2 CreateOrthographic(float left, float right, float bottom, float top) {
		float xScale = 2.0F / (right - left);
		float yScale = 2.0F / (top - bottom);
		return CreateScale(xScale, yScale);
	}

	/// <summary>
	///     Linearly interpolates between two matrices.
	/// </summary>
	/// <param name="a">Start matrix.</param>
	/// <param name="b">End matrix.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>The interpolated matrix.</returns>
	public static Matrix2x2 Lerp(in Matrix2x2 a, in Matrix2x2 b, float t) {
		t = Math.Clamp(t, 0.0F, 1.0F);
		return new Matrix2x2(
			a.M00 + (b.M00 - a.M00) * t,
			a.M10 + (b.M10 - a.M10) * t,
			a.M01 + (b.M01 - a.M01) * t,
			a.M11 + (b.M11 - a.M11) * t
		);
	}

	public static Matrix2x2 operator *(in Matrix2x2 a, in Matrix2x2 b) {
		return a.Multiply(b);
	}

	public static Vector2 operator *(in Matrix2x2 v, in Vector2 vec) {
		return v.Transform(vec);
	}

	public static Matrix2x2 operator *(in Matrix2x2 v, float scalar) {
		return v.Scale(scalar);
	}

	public static Matrix2x2 operator *(float scalar, in Matrix2x2 v) {
		return v.Scale(scalar);
	}

	public static Matrix2x2 operator /(in Matrix2x2 v, float scalar) {
		return v.Scale(1.0F / scalar);
	}

	public static Matrix2x2 operator -(in Matrix2x2 v) {
		return v.Scale(-1.0F);
	}
}
