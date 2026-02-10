namespace Mino.Mathematics.Stereo;

/// <summary>
///     Represents a 3D camera.
/// </summary>
public abstract class Camera {
	protected bool _dirty = true;
	private float _far = 1000.0F;
	private Frustum _frustum;
	private float _near = 0.1F;
	private Quaternion _orientation = Quaternion.Identity;
	private Vector3 _position = Vector3.Zero;
	private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
	private Vector3 _target = -Vector3.UnitZ;
	private Vector3 _up = Vector3.Zero;
	private Matrix4x4 _viewMatrix = Matrix4x4.Identity;
	private Matrix4x4 _viewProjectionMatrix = Matrix4x4.Identity;

	/// <summary>
	///     Position of the camera. (Y-Up)
	/// </summary>
	public Vector3 Position {
		get => _position;
		set {
			if (_position == value) {
				return;
			}
			_position = value;
			_dirty = true;
		}
	}

	/// <summary>
	///     Target of the camera. (Y-Up)
	/// </summary>
	public Vector3 Target {
		get => _target;
		set {
			if (_target == value) {
				return;
			}
			_target = value;
			_dirty = true;
		}
	}

	/// <summary>
	///     Up direction of the camera. (Y-Up)
	/// </summary>
	public Vector3 Up {
		get => _up;
		set {
			if (_up == value) {
				return;
			}
			_up = value;
			_dirty = true;
		}
	}

	/// <summary>
	///     Near plane distance.
	/// </summary>
	public float NearPlane {
		get => _near;
		set {
			if (Comparison.DoEqual(_near, value)) {
				return;
			}
			_near = value;
			_dirty = true;
		}
	}

	/// <summary>
	///     Far plane distance.
	/// </summary>
	public float FarPlane {
		get => _far;
		set {
			if (Comparison.DoEqual(_far, value)) {
				return;
			}
			_far = value;
			_dirty = true;
		}
	}

	/// <summary>
	///     Target directional vector. (Y-Up)
	/// </summary>
	public Vector3 Forward {
		get => (Target - Position).Normalize();
	}

	/// <summary>
	///     Right directional vector. (Y-Up)
	/// </summary>
	public Vector3 Right {
		get => Forward.Cross(Up).Normalize();
	}

	/// <summary>
	///     The view matrix. (Y-Up)
	/// </summary>
	public Matrix4x4 ViewMatrix {
		get {
			checkedRebuild();
			return _viewMatrix;
		}
	}

	/// <summary>
	///     The projection matrix. (Y-Up)
	/// </summary>
	public Matrix4x4 ProjectionMatrix {
		get {
			checkedRebuild();
			return _projectionMatrix;
		}
	}

	/// <summary>
	///     The V-P matrix. (Y-Up)
	/// </summary>
	public Matrix4x4 ViewProjectionMatrix {
		get {
			checkedRebuild();
			return _viewProjectionMatrix;
		}
	}

	/// <summary>
	///     The frustum.
	/// </summary>
	public Frustum Frustum {
		get {
			checkedRebuild();
			return _frustum;
		}
	}

	/// <summary>
	///     Gets the raw projection matrix, depending on implementation.
	/// </summary>
	/// <returns>A new raw projection matrix.</returns>
	protected abstract Matrix4x4 getProjectionMatrix();

	/// <summary>
	///     Sets clipping planes of this camera.
	/// </summary>
	/// <param name="near">Near plane dist.</param>
	/// <param name="far">Far plane dist.</param>
	public void SetClippingPlanes(float near, float far) {
		NearPlane = near;
		FarPlane = far;
		_dirty = true;
	}

	/// <summary>
	///     Translates the camera.
	/// </summary>
	/// <param name="translation">Translation vector.</param>
	public void Translate(in Vector3 translation) {
		Position += translation;
	}

	/// <summary>
	///     Co-translates the camera (both position and target).
	/// </summary>
	/// <param name="translation">Translation vector.</param>
	public void Cotranslate(in Vector3 translation) {
		Position += translation;
		Target += translation;
	}

	/// <summary>
	///     Sets the rotation of this camera by current position using quaternion rotation.
	/// </summary>
	/// <param name="rotation">Absolute rotation quaternion.</param>
	/// <param name="baseLookAt">Look-at base.</param>
	/// <param name="baseUp">World up direction.</param>
	public void SetRotation(in Quaternion rotation, in Vector3 baseLookAt, in Vector3 baseUp) {
		Vector3 newForward = rotation.Rotate(baseLookAt);
		Target = Position + newForward;
		Up = rotation.Rotate(baseUp);
	}

	/// <summary>
	///     Sets the rotation of this camera by current position using quaternion rotation.
	/// </summary>
	/// <param name="rotation">Absolute rotation quaternion.</param>
	public void SetRotation(in Quaternion rotation) {
		SetRotation(rotation, -Vector3.UnitZ, Vector3.UnitY);
	}

	/// <summary>
	///     Transforms world coordinates to screen coordinates and depth.
	/// </summary>
	/// <param name="worldPosition">World position. (Y-Up)</param>
	/// <param name="viewport">Current viewport. (Y-Down)</param>
	/// <param name="xyd">Output screen x, screen y and depth. (Y-Down)</param>
	/// <returns>Whether the cast is valid.</returns>
	public bool Project(in Vector3 worldPosition, in Box2 viewport, out Vector3 xyd) {
		Vector4 clipPos = ViewProjectionMatrix.Transform(new Vector4(worldPosition, 1.0f));

		if (Comparison.DoEqual(0.0F, clipPos.W)) {
			xyd = Vector3.Zero;
			return false;
		}

		Vector3 ndc = new Vector3(clipPos.X / clipPos.W, clipPos.Y / clipPos.W, clipPos.Z / clipPos.W);
		float outZ = ndc.Z;

		if (ndc.X < -1.0F || ndc.X > 1.0F || ndc.Y < -1.0F || ndc.Y > 1.0F || ndc.Z < -1.0F || ndc.Z > 1.0F) {
			xyd = new Vector3(0.0F, 0.0F, outZ);
			return false;
		}

		float outX = (ndc.X + 1) * 0.5F * viewport.Width + viewport.MinX;
		float outY = ndc.Y * -0.5F * viewport.Height + 0.5F * viewport.Height + viewport.MinY;
		xyd = new Vector3(outX, outY, outZ);
		return true;
	}

	/// <summary>
	///     Transforms screen coordinates to a world ray.
	/// </summary>
	/// <param name="screenPosition">Screen position. (Y-Down)</param>
	/// <param name="viewport">Current viewport. (Y-Down)</param>
	/// <returns>A ray in the world.</returns>
	public Ray Unproject(in Vector2 screenPosition, in Box2 viewport) {
		return Ray.CreateFromScreen(
			screenPosition.X,
			screenPosition.Y,
			viewport.Width,
			viewport.Height,
			ProjectionMatrix,
			ViewMatrix
		);
	}

	private void checkedRebuild() {
		if (!_dirty) {
			return;
		}
		_viewMatrix = Matrix4x4.CreateLookAt(Position, Target, Up);
		_projectionMatrix = getProjectionMatrix();
		_viewProjectionMatrix = _projectionMatrix * _viewMatrix;
		_frustum = new Frustum(_viewProjectionMatrix);
		_dirty = false;
	}
}
