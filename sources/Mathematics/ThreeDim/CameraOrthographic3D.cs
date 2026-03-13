namespace Mino.Mathematics.ThreeDim;

/// <summary>
///     Represents a 3D orthographic camera.
/// </summary>
public class CameraOrthographic3D : Camera3D {
	private float _height;
	private float _width;
	private float _zoom = 1.0F;

	/// <summary>
	///     Width of the orthographic projection.
	/// </summary>
	public float Width {
		get => _width;
		set {
			if (Comparison.DoEqual(_width, value)) {
				return;
			}
			_width = Math.Max(value, 0.01F);
			_dirty = true;
		}
	}

	/// <summary>
	///     Height of the orthographic projection.
	/// </summary>
	public float Height {
		get => _height;
		set {
			if (Comparison.DoEqual(_height, value)) {
				return;
			}
			_height = Math.Max(value, 0.01F);
			_dirty = true;
		}
	}

	/// <summary>
	///     Zoom of the camera.
	/// </summary>
	public float Zoom {
		get => _zoom;
		set {
			if (Comparison.DoEqual(_zoom, value)) {
				return;
			}
			_zoom = Math.Max(value, 0.01F);
			_dirty = true;
		}
	}

	/// <summary>
	///     Aspect ratio (width / height).
	/// </summary>
	public float AspectRatio {
		get => _width / _height;
	}

	/// <summary>
	///     Sets orthographic params.
	/// </summary>
	/// <param name="width">Width of projection.</param>
	/// <param name="height">Height of projection.</param>
	public void SetOrthographic(float width, float height) {
		Width = width;
		Height = height;
	}

	/// <summary>
	///     Sets orthographic params by width and ratio.
	/// </summary>
	/// <param name="width">Width of projection.</param>
	/// <param name="aspectRatio">Ratio of the projection.</param>
	public void SetOrthographicByAspect(float width, float aspectRatio) {
		Width = width;
		Height = width / aspectRatio;
	}

	protected override Matrix4x4 getProjectionMatrix() {
		float halfWidth = _width * 0.5F / _zoom;
		float halfHeight = _height * 0.5F / _zoom;

		return Matrix4x4.CreateOrthographic(
			-halfWidth, halfWidth,
			-halfHeight, halfHeight,
			NearPlane, FarPlane
		);
	}
}
