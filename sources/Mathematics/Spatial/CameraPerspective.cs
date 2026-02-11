namespace Mino.Mathematics.Spatial;

/// <summary>
///     Represents a 3D perspective camera.
/// </summary>
public class CameraPerspective : Camera {
	private float _aspectRatio;
	private float _fov;

	/// <summary>
	///     Field of view (FOV).
	/// </summary>
	public float FieldOfView {
		get => _fov;
		set {
			if (Comparison.DoEqual(_fov, value)) {
				return;
			}
			_fov = Math.Clamp(value, 0.01F, MathF.PI - 0.01F);
			_dirty = true;
		}
	}

	/// <summary>
	///     Aspect ratio (width / height).
	/// </summary>
	public float AspectRatio {
		get => _aspectRatio;
		set {
			if (Comparison.DoEqual(_aspectRatio, value)) {
				return;
			}
			_aspectRatio = Math.Max(value, 0.01F);
			_dirty = true;
		}
	}

	/// <summary>
	///     Sets fov and aspect.
	/// </summary>
	/// <param name="fieldOfView">Fov value.</param>
	/// <param name="aspectRatio">Aspect value.</param>
	public void SetPerspective(float fieldOfView, float aspectRatio) {
		FieldOfView = fieldOfView;
		AspectRatio = aspectRatio;
	}

	protected override Matrix4x4 getProjectionMatrix() {
		return Matrix4x4.CreatePerspective(_fov, _aspectRatio, NearPlane, FarPlane);
	}
}
