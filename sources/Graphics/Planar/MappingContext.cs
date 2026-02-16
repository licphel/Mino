using Mino.Graphics.RHI;
using Mino.Mathematics;
using Mino.Mathematics.Planar;

namespace Mino.Graphics.Planar;

/// <summary>
///		Brush canvas mapping context.
/// </summary>
public readonly struct MappingContext {
	public readonly CameraPlanar Camera;
	public readonly Box2 Viewport;
	
	public MappingContext(CameraPlanar camera, Box2 viewport) {
		Camera = camera;
		Viewport = viewport;
	}

	/// <summary>
	///		Resolved size.
	/// </summary>
	public Vector2 Size {
		get => new Vector2(Camera.Width, Camera.Height);
	}

	/// <summary>
	///		Cursor pos in the resolved canvas.
	/// </summary>
	public Vector2 Cursor {
		get {
			Vector2 rawCursor = RenderSystem.GetWindow().Cursor;
			return Camera.Unproject(rawCursor, Viewport);
		}
	}
}
