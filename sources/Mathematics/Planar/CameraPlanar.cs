#region
using Mino.Graphics;
#endregion

namespace Mino.Mathematics.Planar;

/// <summary>
///     Represents a 2D orthographic camera for 2D GUI rendering (Y-Down, top-left origin).
/// </summary>
public sealed class CameraPlanar {
	private float _height = 1.0F;
	private bool _isDirty = true;
	private Vector2 _position;
	private Matrix4x4 _viewProjectionMatrix;
	private float _width = 1.0F;
	private float _zoom = 1.0F;

	/// <summary>
	///     Camera position in 2D world coordinates (top-left origin).
	/// </summary>
	public Vector2 Position {
		get => _position;
		set {
			if (_position == value) {
				return;
			}
			_position = value;
			_isDirty = true;
		}
	}

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
			_isDirty = true;
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
			_isDirty = true;
		}
	}

	/// <summary>
	///     Zoom level of the camera.
	/// </summary>
	public float Zoom {
		get => _zoom;
		set {
			if (Comparison.DoEqual(_zoom, value)) {
				return;
			}
			_zoom = Math.Max(value, 0.01F);
			_isDirty = true;
		}
	}

	/// <summary>
	///     Aspect ratio (width / height).
	/// </summary>
	public float AspectRatio {
		get => _width / _height;
	}

	/// <summary>
	///     The view-projection matrix for 2D rendering (Y-Down).
	/// </summary>
	public Matrix4x4 ViewProjectionMatrix {
		get {
			if (_isDirty) {
				checkedRebuild();
			}
			return _viewProjectionMatrix;
		}
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

	/// <summary>
	///     Translates the camera.
	/// </summary>
	/// <param name="translation">Translation vector.</param>
	public void Translate(in Vector2 translation) {
		Position += translation;
	}

	/// <summary>
	///     Transforms world coordinates to screen coordinates.
	/// </summary>
	/// <param name="worldPosition">World position. (Y-Down)</param>
	/// <param name="viewport">Current viewport.</param>
	/// <returns>Screen coordinates. (Y-Down)</returns>
	public Vector2 Project(in Vector2 worldPosition, in Box2 viewport) {
		Matrix4x4 vpMatrix = ViewProjectionMatrix;

		Vector4 clipPos = vpMatrix * new Vector4(worldPosition.X, worldPosition.Y, 0.0F, 1.0F);

		if (Comparison.DoEqual(clipPos.W, 0.0F)) {
			return Vector2.Zero;
		}

		float ndcX = clipPos.X / clipPos.W;
		float ndcY = clipPos.Y / clipPos.W;

		float screenX = (ndcX + 1.0F) * 0.5F * viewport.Width + viewport.MinX;
		float screenY = (1.0F - ndcY) * 0.5F * viewport.Height + viewport.MinY;

		return new Vector2(screenX, screenY);
	}

	/// <summary>
	///     Transforms screen coordinates to world coordinates.
	/// </summary>
	/// <param name="screenPosition">Screen position. (Y-Down)</param>
	/// <param name="viewport">Current viewport.</param>
	/// <returns>World coordinates. (Y-Down)</returns>
	public Vector2 Unproject(in Vector2 screenPosition, in Box2 viewport) {
		float ndcX = (screenPosition.X - viewport.MinX) / viewport.Width * 2.0F - 1.0F;
		float ndcY = 1.0F - (screenPosition.Y - viewport.MinY) / viewport.Height * 2.0F;

		Matrix4x4 invVpMatrix = ViewProjectionMatrix.Invert();

		Vector4 worldPos = invVpMatrix * new Vector4(ndcX, ndcY, 0.0F, 1.0F);

		float invW = 1.0F / worldPos.W;
		return new Vector2(worldPos.X * invW, worldPos.Y * invW);
	}

	private void checkedRebuild() {
		float halfWidth = _width * 0.5F / _zoom;
		float halfHeight = _height * 0.5F / _zoom;

		float left = _position.X - halfWidth;
		float right = _position.X + halfWidth;
		float top = _position.Y - halfHeight;
		float bottom = _position.Y + halfHeight;

		_viewProjectionMatrix = new Matrix4x4(
			2.0F / (right - left), 0.0F, 0.0F, 0.0F,
			0.0F, 2.0F / (top - bottom), 0.0F, 0.0F,
			0.0F, 0.0F, 1.0F, 0.0F,
			-(right + left) / (right - left), -(top + bottom) / (top - bottom), 0.0F, 1.0F
		);
		_isDirty = false;
	}

	/// <summary>
	///     Gets a window-sized camera.
	/// </summary>
	/// <param name="camera">Target camera.</param>
	/// <returns>A normal camera.</returns>
	public static CameraPlanar Normal(CameraPlanar camera) {
		Vector2 size = RenderSystem.GetWindow().Size;

		if (size.X <= 1E-3F || size.Y <= 1E-3F) {
			// Avoid NaN.
			size = new Vector2(0.1F, 0.1F);
		}

		camera.SetOrthographic(size.X, size.Y);
		camera.Position = size / 2.0F;
		return camera;
	}

	/// <summary>
	///     Gets a resolved-to-size camera.
	/// </summary>
	/// <param name="camera">Target camera.</param>
	/// <param name="onlyInt">If true, the camera resolution will be limited to integer.</param>
	/// <param name="fixedResolution">Positive if you want a fixed resolution.</param>
	/// <returns>A resolved camera.</returns>
	public static CameraPlanar Resolved(CameraPlanar camera, bool onlyInt = false, float fixedResolution = -1.0F) {
		Vector2 size = RenderSystem.GetWindow().Size;

		if (size.X <= 1E-3F || size.Y <= 1E-3F) {
			// Avoid NaN.
			size = new Vector2(0.1F, 0.1F);
		}

		float factor = fixedResolution;
		if (fixedResolution <= 0) {
			factor = 0.5F;
			while (size.X / (factor + 0.5F) >= 800.0F && size.Y / (factor + 0.5F) >= 450.0F) {
				factor += 0.5F;
			}

			if (onlyInt && (int) (factor * 2) % 2 != 0 && factor - 0.5F > 0) {
				factor -= 0.5F;
			}
		}

		camera.SetOrthographic(size.X / factor, size.Y / factor);
		camera.Position = new Vector2(size.X / factor, size.Y / factor) / 2.0F;
		return camera;
	}

	/// <summary>
	///     Gets a world camera.
	/// </summary>
	/// <param name="camera">Target camera.</param>
	/// <param name="center">Sight center.</param>
	/// <param name="horiSight">Sight horizontal size.</param>
	/// <returns>A world camera.</returns>
	public static CameraPlanar World(CameraPlanar camera, in Vector2 center, float horiSight) {
		Vector2 size = RenderSystem.GetWindow().Size;

		if (size.X <= 1E-3F || size.Y <= 1E-3F) {
			// Avoid NaN.
			size = new Vector2(0.1F, 0.1F);
		}

		if (horiSight <= 1E-3F) {
			// Avoid NaN.
			horiSight = 0.1F;
		}

		// Fit to window ratio.
		camera.SetOrthographicByAspect(horiSight, size.X / size.Y);
		camera.Position = center;
		return camera;
	}
}
