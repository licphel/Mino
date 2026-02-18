using Mino.Framework;
using Mino.Graphics.Hardware;
using Mino.Graphics.Sprite;
using Mino.Mathematics;
using Mino.Mathematics.Planar;

namespace Mino.Graphics.Gui;

/// <summary>
///		Brush canvas context.
/// </summary>
public class CanvasContext {
	public readonly Brush Brush;
	public readonly TimeStep Step;
	public readonly float Partial;

	public CanvasContext(Brush brush, float partial, TimeStep step = default) {
		Brush = brush;
		Partial = partial;
		Step = step;
	}
	
	/// <summary>
	///		Canvas camera.
	/// </summary>
	public virtual CameraPlanar Camera {
		get => Brush.Camera;
	}

	/// <summary>
	///		Canvas viewport.
	/// </summary>
	public virtual Box2 Viewport {
		get => Brush.CurrentViewport;
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
