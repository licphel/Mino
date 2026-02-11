namespace Mino.Mathematics.Spatial;

/// <summary>
///     View frustum defined by six clipping planes.
/// </summary>
public readonly struct Frustum {
	/// <summary>
	///     Frustum clipping planes in order: Left, Right, Bottom, Top, Near, Far.
	/// </summary>
	public enum PlaneIndex {
		Left = 0,
		Right = 1,
		Bottom = 2,
		Top = 3,
		Near = 4,
		Far = 5
	}

	private readonly Plane[] _planes;
	private readonly Vector3[] _corners;

	/// <summary>
	///     Initializes a new frustum from six clipping planes.
	/// </summary>
	/// <param name="planes">Six clipping planes in order: Left, Right, Bottom, Top, Near, Far.</param>
	public Frustum(Plane[] planes) {
		if (planes == null || planes.Length != 6) {
			throw new Error("Frustum requires exactly six planes.", nameof(planes));
		}

		_planes = new Plane[6];
		for (int i = 0; i < 6; i++) {
			_planes[i] = planes[i].Normalize();
		}

		_corners = new Vector3[8];
		genCorners();
	}

	/// <summary>
	///     Initializes a new frustum from view-projection matrix.
	/// </summary>
	/// <param name="viewProjection">Combined view-projection matrix.</param>
	public Frustum(in Matrix4x4 viewProjection) {
		_planes = new Plane[6];
		_corners = new Vector3[8];
		genPlanes(viewProjection);
	}

	/// <summary>
	///     Gets the specified clipping plane.
	/// </summary>
	/// <param name="index">Plane index.</param>
	/// <returns>The clipping plane.</returns>
	public Plane GetPlane(PlaneIndex index) {
		return _planes[(int) index];
	}

	/// <summary>
	///     Gets all six clipping planes.
	/// </summary>
	public ReadOnlySpan<Plane> Planes {
		get => _planes;
	}

	/// <summary>
	///     Gets all eight corner points of the frustum.
	/// </summary>
	public ReadOnlySpan<Vector3> Corners {
		get => _corners;
	}

	/// <summary>
	///     Gets the center point of the frustum.
	/// </summary>
	public Vector3 Center {
		get {
			Vector3 sum = Vector3.Zero;
			foreach (Vector3 corner in _corners) {
				sum += corner;
			}
			return sum / 8.0F;
		}
	}

	/// <summary>
	///     Gets the bounding box that encloses the frustum.
	/// </summary>
	public Box3 BoundingBox {
		get {
			if (_corners.Length == 0) {
				return Box3.Empty;
			}

			Vector3 min = _corners[0];
			Vector3 max = _corners[0];

			for (int i = 1; i < 8; i++) {
				min = Vector3.Min(min, _corners[i]);
				max = Vector3.Max(max, _corners[i]);
			}

			return new Box3(min, max);
		}
	}

	/// <summary>
	///     Checks if a point is inside the frustum.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>True if point is inside or on frustum boundaries.</returns>
	public bool Test(in Vector3 point) {
		for (int i = 0; i < 6; i++) {
			if (_planes[i].GetDistanceToPoint(point) < 0) {
				return false;
			}
		}
		return true;
	}

	/// <summary>
	///     Checks if a sphere is inside, intersecting, or outside the frustum.
	/// </summary>
	/// <param name="sphere">The Sphere to test.</param>
	/// <returns>
	///     -1: Completely outside
	///     0: Intersecting
	///     1: Completely inside
	/// </returns>
	public int Test(in Sphere sphere) {
		int result = 1;

		for (int i = 0; i < 6; i++) {
			float distance = _planes[i].GetDistanceToPoint(sphere.Center);
			if (distance < -sphere.Radius) {
				return -1;
			}
			if (distance < sphere.Radius) {
				result = 0;
			}
		}

		return result;
	}

	/// <summary>
	///     Checks if an axis-aligned bounding box is inside, intersecting, or outside the frustum.
	/// </summary>
	/// <param name="box">The bounding box to test.</param>
	/// <returns>
	///     -1: Completely outside
	///     0: Intersecting
	///     1: Completely inside
	/// </returns>
	public int Test(in Box3 box) {
		int result = 1;

		for (int i = 0; i < 6; i++) {
			Plane plane = _planes[i];

			Vector3 positiveVertex = new Vector3(
				plane.Normal.X > 0 ? box.MaxX : box.MinX,
				plane.Normal.Y > 0 ? box.MaxY : box.MinY,
				plane.Normal.Z > 0 ? box.MaxZ : box.MinZ
			);
			Vector3 negativeVertex = new Vector3(
				plane.Normal.X > 0 ? box.MinX : box.MaxX,
				plane.Normal.Y > 0 ? box.MinY : box.MaxY,
				plane.Normal.Z > 0 ? box.MinZ : box.MaxZ
			);

			if (plane.GetDistanceToPoint(positiveVertex) < 0) {
				return -1;
			}

			if (plane.GetDistanceToPoint(negativeVertex) < 0) {
				result = 0;
			}
		}

		return result;
	}

	/// <summary>
	///     Checks if a ray intersects the frustum.
	/// </summary>
	/// <param name="ray">The ray to test.</param>
	/// <param name="t">Output intersection distance along ray.</param>
	/// <returns>True if ray intersects the frustum.</returns>
	public bool Test(in Ray ray, out float t) {
		float tMin = 0.0F;
		float tMax = float.MaxValue;

		for (int i = 0; i < 6; i++) {
			Plane plane = _planes[i];
			float denom = plane.Normal.Dot(ray.Direction);
			float dist = -(plane.Normal.Dot(ray.Origin) + plane.Distance) / denom;

			if (Comparison.DoEqual(0.0F, denom)) {
				if (plane.GetDistanceToPoint(ray.Origin) < 0) {
					t = float.NaN;
					return false;
				}
			} else {
				if (denom > 0) // Entering
				{
					tMin = Math.Max(tMin, dist);
				} else // Exiting
				{
					tMax = Math.Min(tMax, dist);
				}
			}

			if (tMin > tMax) {
				t = float.NaN;
				return false;
			}
		}

		t = tMin >= 0 ? tMin : tMax;
		return t >= 0 && t <= float.MaxValue;
	}

	/// <summary>
	///     Transforms the frustum by a matrix.
	/// </summary>
	/// <param name="matrix">Transformation matrix.</param>
	/// <returns>Transformed frustum.</returns>
	public Frustum Transform(in Matrix4x4 matrix) {
		var newPlanes = new Plane[6];
		for (int i = 0; i < 6; i++) {
			newPlanes[i] = _planes[i].Transform(matrix);
		}
		return new Frustum(newPlanes);
	}

	/// <summary>
	///     Creates a frustum from perspective projection parameters.
	/// </summary>
	/// <param name="fovY">Vertical field of view in radians.</param>
	/// <param name="aspect">Aspect ratio (width/height).</param>
	/// <param name="near">Near clipping plane distance.</param>
	/// <param name="far">Far clipping plane distance.</param>
	/// <returns>A perspective frustum.</returns>
	public static Frustum CreatePerspective(float fovY, float aspect, float near, float far) {
		float tanHalfFov = MathF.Tan(fovY * 0.5F);
		float nearHeight = near * tanHalfFov;
		float nearWidth = nearHeight * aspect;
		float farHeight = far * tanHalfFov;
		float farWidth = farHeight * aspect;
		Vector3 nearCenter = new Vector3(0, 0, -near);
		Vector3 farCenter = new Vector3(0, 0, -far);

		Vector3[] corners = [
			nearCenter + new Vector3(-nearWidth, -nearHeight, 0),
			nearCenter + new Vector3(nearWidth, -nearHeight, 0),
			nearCenter + new Vector3(nearWidth, nearHeight, 0),
			nearCenter + new Vector3(-nearWidth, nearHeight, 0),
			farCenter + new Vector3(-farWidth, -farHeight, 0),
			farCenter + new Vector3(farWidth, -farHeight, 0),
			farCenter + new Vector3(farWidth, farHeight, 0),
			farCenter + new Vector3(-farWidth, farHeight, 0)
		];

		var planes = new Plane[6];
		planes[0] = Plane.CreateFromPoints(corners[4], corners[7], corners[3]);
		planes[1] = Plane.CreateFromPoints(corners[5], corners[1], corners[2]);
		planes[2] = Plane.CreateFromPoints(corners[4], corners[0], corners[1]);
		planes[3] = Plane.CreateFromPoints(corners[7], corners[6], corners[5]);
		planes[4] = Plane.CreateFromPoints(corners[0], corners[3], corners[2]);
		planes[5] = Plane.CreateFromPoints(corners[4], corners[5], corners[6]);

		return new Frustum(planes);
	}

	/// <summary>
	///     Creates a frustum from orthographic projection parameters.
	/// </summary>
	/// <param name="left">Left clipping plane.</param>
	/// <param name="right">Right clipping plane.</param>
	/// <param name="bottom">Bottom clipping plane.</param>
	/// <param name="top">Top clipping plane.</param>
	/// <param name="near">Near clipping plane distance.</param>
	/// <param name="far">Far clipping plane distance.</param>
	/// <returns>An orthographic frustum.</returns>
	public static Frustum CreateOrthographic(float left, float right, float bottom, float top,
		float near, float far) {
		var planes = new Plane[6];
		planes[0] = new Plane(Vector3.UnitX, left); // Left
		planes[1] = new Plane(-Vector3.UnitX, -right); // Right
		planes[2] = new Plane(Vector3.UnitY, bottom); // Bottom
		planes[3] = new Plane(-Vector3.UnitY, -top); // Top
		planes[4] = new Plane(Vector3.UnitZ, near); // Near
		planes[5] = new Plane(-Vector3.UnitZ, -far); // Far

		return new Frustum(planes);
	}

	/// <summary>
	///     Creates a frustum from view and projection matrices.
	/// </summary>
	/// <param name="view">View matrix.</param>
	/// <param name="projection">Projection matrix.</param>
	/// <returns>A view frustum.</returns>
	public static Frustum CreateFromMatrices(in Matrix4x4 view, in Matrix4x4 projection) {
		return new Frustum(projection * view);
	}

	/// <summary>
	///     Checks if this frustum is similar to another frustum within tolerance.
	/// </summary>
	/// <param name="other">Other frustum to compare.</param>
	/// <param name="positionTolerance">Position tolerance.</param>
	/// <param name="normalTolerance">Normal tolerance in radians.</param>
	/// <returns>True if frustums are similar.</returns>
	public bool IsSimilarTo(in Frustum other, float positionTolerance = 1E-4F,
		float normalTolerance = 1E-4F) {
		for (int i = 0; i < 6; i++) {
			if (!_planes[i].IsSimilarTo(other._planes[i], positionTolerance, normalTolerance)) {
				return false;
			}
		}
		return true;
	}

	private void genPlanes(in Matrix4x4 m) {
		Vector4 row0 = new Vector4(m.M00, m.M01, m.M02, m.M03);
		Vector4 row1 = new Vector4(m.M10, m.M11, m.M12, m.M13);
		Vector4 row2 = new Vector4(m.M20, m.M21, m.M22, m.M23);
		Vector4 row3 = new Vector4(m.M30, m.M31, m.M32, m.M33);

		var planesEq = new Vector4[6];
		planesEq[0] = row3 + row0; // left
		planesEq[1] = row3 - row0; // right
		planesEq[2] = row3 + row1; // bottom
		planesEq[3] = row3 - row1; // top
		planesEq[4] = row3 + row2; // near
		planesEq[5] = row3 - row2; // far

		for (int i = 0; i < 6; i++) {
			Vector4 eq = planesEq[i];
			Vector3 normal = new Vector3(eq.X, eq.Y, eq.Z);
			float length = normal.Length;

			if (length > 0) {
				normal /= length;
				float d = eq.W / length;
				_planes[i] = new Plane(normal, d).Normalize();
			} else {
				_planes[i] = new Plane(Vector3.UnitZ, 0).Normalize();
			}
		}

		genCorners();
	}

	private void genCorners() {
		_corners[0] = intersectOf(_planes[4], _planes[0], _planes[2]);
		_corners[1] = intersectOf(_planes[4], _planes[1], _planes[2]);
		_corners[2] = intersectOf(_planes[4], _planes[1], _planes[3]);
		_corners[3] = intersectOf(_planes[4], _planes[0], _planes[3]);
		_corners[4] = intersectOf(_planes[5], _planes[0], _planes[2]);
		_corners[5] = intersectOf(_planes[5], _planes[1], _planes[2]);
		_corners[6] = intersectOf(_planes[5], _planes[1], _planes[3]);
		_corners[7] = intersectOf(_planes[5], _planes[0], _planes[3]);
	}

	private static Vector3 intersectOf(in Plane p1, in Plane p2, in Plane p3) {
		Matrix3x3 mat = new Matrix3x3(
			p1.Normal.X, p1.Normal.Y, p1.Normal.Z,
			p2.Normal.X, p2.Normal.Y, p2.Normal.Z,
			p3.Normal.X, p3.Normal.Y, p3.Normal.Z
		);

		float det = mat.Determinant;
		if (Comparison.DoEqual(0.0F, det)) {
			return Vector3.Zero;
		}

		Vector3 b = new Vector3(-p1.Distance, -p2.Distance, -p3.Distance);
		return mat.Invert().Transform(b);
	}
}
