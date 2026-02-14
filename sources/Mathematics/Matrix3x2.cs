namespace Mino.Mathematics;

/// <summary>
///     Immutable column major ordered affine matrix 3x2.
/// </summary>
public readonly struct Matrix3x2 : Matrix<Matrix3x2> {
	public static readonly Matrix3x2 Identity = new Matrix3x2();

	public readonly float M00 = 1.0F;
	public readonly float M10 = 0.0F;
	public readonly float M01 = 0.0F;
	public readonly float M11 = 1.0F;
	public readonly float M02 = 0.0F;
	public readonly float M12 = 0.0F;

	public Matrix3x2() {
	}

	public Matrix3x2(float m00, float m10, float m01, float m11, float m02, float m12) {
		M00 = m00;
		M10 = m10;
		M01 = m01;
		M11 = m11;
		M02 = m02;
		M12 = m12;
	}

	/// <summary>
	///     Gets the determinant of the 2x2 linear transformation part.
	/// </summary>
	public float Determinant {
		get => M00 * M11 - M01 * M10;
	}

	/// <summary>
	///     Multiplies this matrix by another matrix.
	/// </summary>
	/// <param name="other">The matrix to multiply by.</param>
	/// <returns>The product matrix.</returns>
	public Matrix3x2 Multiply(in Matrix3x2 other) {
		return new Matrix3x2(
			M00 * other.M00 + M01 * other.M10,
			M10 * other.M00 + M11 * other.M10,
			M00 * other.M01 + M01 * other.M11,
			M10 * other.M01 + M11 * other.M11,
			M00 * other.M02 + M01 * other.M12 + M02,
			M10 * other.M02 + M11 * other.M12 + M12
		);
	}

	/// <summary>
	///     Transforms a point by this matrix.
	/// </summary>
	/// <param name="x">X coordinate of the point.</param>
	/// <param name="y">Y coordinate of the point.</param>
	/// <param name="xo">Output X coordinate of the transformed point.</param>
	/// <param name="yo">Output Y coordinate of the transformed point.</param>
	public void Transform(float x, float y, out float xo, out float yo) {
		xo = M00 * x + M01 * y + M02;
		yo = M10 * x + M11 * y + M12;
	}

	/// <summary>
	///     Transforms a vector by this matrix.
	/// </summary>
	/// <param name="vec">The vector to transform.</param>
	/// <returns>The transformed vector.</returns>
	public Vector2 Transform(in Vector2 vec) {
		Transform(vec.X, vec.Y, out float xo, out float yo);
		return new Vector2(xo, yo);
	}

	/// <summary>
	///     Transforms a bounding box by this matrix.
	/// </summary>
	/// <param name="rect">The bounding box to transform.</param>
	/// <returns>The transformed bounding box.</returns>
	public Box2 Transform(in Box2 rect) {
		Transform(rect.MinX, rect.MaxY, out float x1, out float y1);
		Transform(rect.MaxX, rect.MaxY, out float x2, out float y2);
		Transform(rect.MaxX, rect.MinY, out float x3, out float y3);
		Transform(rect.MinX, rect.MinY, out float x4, out float y4);
		float minX = Math.Min(Math.Min(x1, x2), Math.Min(x3, x4));
		float minY = Math.Min(Math.Min(y1, y2), Math.Min(y3, y4));
		float maxX = Math.Max(Math.Max(x1, x2), Math.Max(x3, x4));
		float maxY = Math.Max(Math.Max(y1, y2), Math.Max(y3, y4));
		return Box2.CreateByPoints(minX, minY, maxX, maxY);
	}

	/// <summary>
	///     Computes the inverse of this affine matrix.
	///     Returns identity if matrix is singular.
	/// </summary>
	/// <returns>The inverse matrix, or identity if singular.</returns>
	public Matrix3x2 Invert() {
		float det = Determinant;
		if (Comparison.DoEqual(0.0F, det)) {
			return Identity;
		}
		float invDet = 1.0F / det;
		return new Matrix3x2(
			invDet * M11,
			invDet * -M10,
			invDet * -M01,
			invDet * M00,
			invDet * (M01 * M12 - M11 * M02),
			invDet * (M10 * M02 - M00 * M12)
		);
	}

	/// <summary>
	///     Translates the matrix by the specified amounts.
	/// </summary>
	/// <param name="x">Translation in X direction.</param>
	/// <param name="y">Translation in Y direction.</param>
	/// <returns>The translated matrix.</returns>
	public Matrix3x2 Translate(float x, float y) {
		return new Matrix3x2(
			M00, M10,
			M01, M11,
			M02 + M00 * x + M01 * y,
			M12 + M10 * x + M11 * y
		);
	}

	/// <summary>
	///     Translates the matrix by the specified vector.
	/// </summary>
	/// <param name="translation">The translation vector.</param>
	/// <returns>The translated matrix.</returns>
	public Matrix3x2 Translate(in Vector2 translation) {
		return Translate(translation.X, translation.Y);
	}

	/// <summary>
	///     Applies shear transformation to this matrix.
	/// </summary>
	/// <param name="x">Shear factor along the X axis.</param>
	/// <param name="y">Shear factor along the Y axis.</param>
	/// <returns>The sheared matrix.</returns>
	public Matrix3x2 Shear(float x, float y) {
		return new Matrix3x2(
			M00 + y * M01,
			M10 + y * M11,
			M01 + x * M00,
			M11 + x * M10,
			M02, M12
		);
	}

	/// <summary>
	///     Applies shear transformation to this matrix.
	/// </summary>
	/// <param name="shear">The shear vector.</param>
	/// <returns>The sheared matrix.</returns>
	public Matrix3x2 Shear(in Vector2 shear) {
		return Shear(shear.X, shear.Y);
	}

	/// <summary>
	///     Scales this matrix by the given factors.
	/// </summary>
	/// <param name="scalarX">Scaling factor for the X direction.</param>
	/// <param name="scalarY">Scaling factor for the Y direction.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix3x2 Scale(float scalarX, float scalarY) {
		return new Matrix3x2(
			M00 * scalarX,
			M10 * scalarX,
			M01 * scalarY,
			M11 * scalarY,
			M02, M12
		);
	}

	/// <summary>
	///     Scales this matrix uniformly.
	/// </summary>
	/// <param name="scalar">Uniform scaling factor.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix3x2 Scale(float scalar) {
		return Scale(scalar, scalar);
	}

	/// <summary>
	///     Scales this matrix by the given vector.
	/// </summary>
	/// <param name="scalar">The scaling vector.</param>
	/// <returns>The scaled matrix.</returns>
	public Matrix3x2 Scale(in Vector2 scalar) {
		return Scale(scalar.X, scalar.Y);
	}

	/// <summary>
	///     Rotates this matrix.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>The rotated matrix.</returns>
	public Matrix3x2 Rotate(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);
		return new Matrix3x2(
			M00 * cos + M01 * sin,
			M10 * cos + M11 * sin,
			M00 * -sin + M01 * cos,
			M10 * -sin + M11 * cos,
			M02, M12
		);
	}

	/// <summary>
	///     Converts to a 2x2 matrix (drops translation).
	/// </summary>
	/// <returns>A 2x2 matrix.</returns>
	public Matrix2x2 ToMatrix2x2() {
		return new Matrix2x2(
			M00, M10,
			M01, M11
		);
	}

	/// <summary>
	///     Converts to a 3x3 matrix.
	/// </summary>
	/// <returns>A 3x3 matrix.</returns>
	public Matrix3x3 ToMatrix3x3() {
		return new Matrix3x3(
			M00, M10, 0.0F,
			M01, M11, 0.0F,
			M02, M12, 1.0F
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
			M02, M12, 0.0F, 1.0F
		);
	}

	/// <summary>
	///     Creates a translation matrix.
	/// </summary>
	/// <param name="x">Translation in X direction.</param>
	/// <param name="y">Translation in Y direction.</param>
	/// <returns>A translation matrix.</returns>
	public static Matrix3x2 CreateTranslation(float x, float y) {
		return new Matrix3x2(1.0F, 0.0F, 0.0F, 1.0F, x, y);
	}

	/// <summary>
	///     Creates a translation matrix.
	/// </summary>
	/// <param name="translation">The translation vector.</param>
	/// <returns>A translation matrix.</returns>
	public static Matrix3x2 CreateTranslation(in Vector2 translation) {
		return CreateTranslation(translation.X, translation.Y);
	}

	/// <summary>
	///     Creates a uniform scaling matrix.
	/// </summary>
	/// <param name="scalar">Uniform scaling factor.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix3x2 CreateScale(float scalar) {
		return new Matrix3x2(scalar, 0.0F, 0.0F, scalar, 0.0F, 0.0F);
	}

	/// <summary>
	///     Creates a scaling matrix.
	/// </summary>
	/// <param name="scalarX">Scaling factor in the X direction.</param>
	/// <param name="scalarY">Scaling factor in the Y direction.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix3x2 CreateScale(float scalarX, float scalarY) {
		return new Matrix3x2(scalarX, 0.0F, 0.0F, scalarY, 0.0F, 0.0F);
	}

	/// <summary>
	///     Creates a scaling matrix.
	/// </summary>
	/// <param name="scalar">The scaling vector.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix3x2 CreateScale(in Vector2 scalar) {
		return CreateScale(scalar.X, scalar.Y);
	}

	/// <summary>
	///     Creates a scaling matrix centered at a specific point.
	/// </summary>
	/// <param name="scalarX">Scaling factor in the X direction.</param>
	/// <param name="scalarY">Scaling factor in the Y direction.</param>
	/// <param name="center">The center point of scaling.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix3x2 CreateScale(float scalarX, float scalarY, in Vector2 center) {
		float tx = center.X * (1.0F - scalarX);
		float ty = center.Y * (1.0F - scalarY);
		return new Matrix3x2(scalarX, 0.0F, 0.0F, scalarY, tx, ty);
	}

	/// <summary>
	///     Creates a scaling matrix centered at a specific point.
	/// </summary>
	/// <param name="scalar">The scaling vector.</param>
	/// <param name="center">The center point of scaling.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix3x2 CreateScale(in Vector2 scalar, in Vector2 center) {
		return CreateScale(scalar.X, scalar.Y, center);
	}

	/// <summary>
	///     Creates a uniform scaling matrix centered at a specific point.
	/// </summary>
	/// <param name="scalar">Uniform scaling factor.</param>
	/// <param name="center">The center point of scaling.</param>
	/// <returns>A scaling matrix.</returns>
	public static Matrix3x2 CreateScale(float scalar, in Vector2 center) {
		return CreateScale(scalar, scalar, center);
	}

	/// <summary>
	///     Creates a rotation matrix.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix3x2 CreateRotation(float rad) {
		FastTrigonometric.Get(rad, out float sin, out float cos);
		return new Matrix3x2(cos, sin, -sin, cos, 0.0F, 0.0F);
	}

	/// <summary>
	///     Creates a rotation matrix around a specific center point.
	/// </summary>
	/// <param name="rad">Rotation angle in radians.</param>
	/// <param name="center">The center of rotation.</param>
	/// <returns>A rotation matrix.</returns>
	public static Matrix3x2 CreateRotation(float rad, in Vector2 center) {
		FastTrigonometric.Get(rad, out float sin, out float cos);
		float cosMinusOne = cos - 1.0F;
		float tx = center.X * cosMinusOne + center.Y * sin;
		float ty = -center.X * sin + center.Y * cosMinusOne;
		return new Matrix3x2(cos, sin, -sin, cos, tx, ty);
	}

	/// <summary>
	///     Creates a shear matrix.
	/// </summary>
	/// <param name="shearX">Shear factor along the X axis.</param>
	/// <param name="shearY">Shear factor along the Y axis.</param>
	/// <returns>A shear matrix.</returns>
	public static Matrix3x2 CreateShear(float shearX, float shearY) {
		return new Matrix3x2(1.0F, shearY, shearX, 1.0F, 0.0F, 0.0F);
	}

	/// <summary>
	///     Creates a shear matrix.
	/// </summary>
	/// <param name="shear">The shear vector.</param>
	/// <returns>A shear matrix.</returns>
	public static Matrix3x2 CreateShear(in Vector2 shear) {
		return CreateShear(shear.X, shear.Y);
	}

	/// <summary>
	///     Creates a transformation matrix from position, rotation, and scale.
	/// </summary>
	/// <param name="position">The position (translation).</param>
	/// <param name="rotation">Rotation angle in radians.</param>
	/// <param name="scalar">Scaling vector.</param>
	/// <returns>A transformation matrix.</returns>
	public static Matrix3x2
		CreateTransform(in Vector2 position, float rotation, in Vector2 scalar) {
		FastTrigonometric.Get(rotation, out float sin, out float cos);
		return new Matrix3x2(
			scalar.X * cos, scalar.X * sin, scalar.Y * -sin, scalar.Y * cos, position.X,
			position.Y);
	}

	/// <summary>
	///     Creates an orthographic projection matrix.
	/// </summary>
	/// <param name="left">Left boundary of the viewport.</param>
	/// <param name="right">Right boundary of the viewport.</param>
	/// <param name="bottom">Bottom boundary of the viewport.</param>
	/// <param name="top">Top boundary of the viewport.</param>
	/// <returns>An orthographic projection matrix.</returns>
	public static Matrix3x2 CreateOrthographic(float left, float right, float bottom, float top) {
		float xo = 2.0F / (right - left);
		float yo = 2.0F / (top - bottom);
		return new Matrix3x2(xo, 0.0F, 0.0F, yo, 0.0F, 0.0F);
	}

	/// <summary>
	///     Creates a matrix that maps one rectangle to another.
	/// </summary>
	/// <param name="src">Source rectangle.</param>
	/// <param name="dst">Destination rectangle.</param>
	/// <returns>A mapping matrix.</returns>
	public static Matrix3x2 CreateRectMapping(in Box2 src, in Box2 dst) {
		float scalarX = (dst.MaxX - dst.MinX) / (src.MaxX - src.MinX);
		float scalarY = (dst.MaxY - dst.MinY) / (src.MaxY - src.MinY);
		float transX = dst.MinX - src.MinX * scalarX;
		float transY = dst.MinY - src.MinY * scalarY;
		return new Matrix3x2(scalarX, 0, 0, scalarY, transX, transY);
	}

	/// <summary>
	///     Linearly interpolates between two matrices.
	/// </summary>
	/// <param name="start">Start matrix.</param>
	/// <param name="end">End matrix.</param>
	/// <param name="t">Interpolation factor (0-1).</param>
	/// <returns>The interpolated matrix.</returns>
	public static Matrix3x2 Lerp(in Matrix3x2 start, in Matrix3x2 end, float t) {
		return new Matrix3x2(
			start.M00 + (end.M00 - start.M00) * t,
			start.M10 + (end.M10 - start.M10) * t,
			start.M01 + (end.M01 - start.M01) * t,
			start.M11 + (end.M11 - start.M11) * t,
			start.M02 + (end.M02 - start.M02) * t,
			start.M12 + (end.M12 - start.M12) * t
		);
	}

	public static Matrix3x2 operator *(in Matrix3x2 a, in Matrix3x2 b) {
		return a.Multiply(b);
	}

	public static Vector2 operator *(in Matrix3x2 v, in Vector2 vec) {
		return v.Transform(vec);
	}

	public static Matrix3x2 operator *(in Matrix3x2 v, float scalar) {
		return v.Scale(scalar);
	}

	public static Matrix3x2 operator *(float scalar, in Matrix3x2 v) {
		return v.Scale(scalar);
	}

	public static Matrix3x2 operator /(in Matrix3x2 v, float scalar) {
		return v.Scale(1.0F / scalar);
	}

	public static Matrix3x2 operator -(in Matrix3x2 v) {
		return v.Scale(-1.0F);
	}
}
