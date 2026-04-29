#region
using Mino.Audio;
using Mino.Audio.Desc;
using Mino.Desktop;
using Mino.Framework;
using Mino.Graphics.Sprite;
using Mino.Input;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///     A GUI canvas containing all components.
/// </summary>
public class Canvas {
	public static readonly Key[] Keymap = [
		Key.Get(Key.MouseLeft) // Focus key
	];
	
	public HashSet<Component?> Focused { get; } = new HashSet<Component?>();
	public readonly List<Face> Presents = new List<Face>();
	public Emitter? Emitter;
	public float Partial;
	public Vector2 Cursor;
	public Vector2 Size;

	/// <summary>
	///     Plays a sound in GUI emitter.
	/// </summary>
	/// <param name="line">Source line.</param>
	public void PlaySound(DataLine? line) {
		if (line == null || Emitter == null) {
			return;
		}
		ClipDesc cDesc = ClipDesc.FromDataLine(line);
		Clip clip = AudioSystem.Create<Clip>(cDesc);
		Emitter.Play(clip);
	}

	/// <summary>
	///     Displays the face.
	/// </summary>
	/// <param name="face">Face to display.</param>
	public void Display(Face face) {
		if (!Presents.Contains(face)) {
			Presents.Add(face);
		}
		face._canvasSupplier = () => this;
		face.RequestResolve();
		face.InitHooks();
	}

	/// <summary>
	///     Closes the face.
	/// </summary>
	/// <param name="face">Face to close.</param>
	public void Close(Face face) {
		Presents.Remove(face);
		face.Parent?.RemoveChild(face);
		face.FreeHooks();
	}

	/// <summary>
	///     Updates all present faces.
	/// </summary>
	/// <param name="step">Time steps.</param>
	public void Update(in TimeStep step) {
		foreach (Face face in Presents) {
			face.Update(step);
		}

		// Reflush focused component.
		if (Keymap[0].Press) {
			Focused.Clear();
			updateFocus(Presents);
		}
	}

	/// <summary>
	///     Draws all present faces.
	/// </summary>
	/// <param name="brush">Brush for drawing.</param>
	/// <param name="partial">Partial ticks.</param>
	public void Draw(Brush brush, float partial) {
		// Update context.
		Window window = RenderSystem.GetWindow();
		Vector2 rawCursor = window.Cursor;
		
		Partial = partial;
		Cursor = brush.Camera.Unproject(rawCursor, brush.CurrentViewport);
		Size = new Vector2(brush.Camera.Width, brush.Camera.Height);
		
		foreach (Face face in Presents) {
			face.Draw(brush, partial);
		}
	}

	private void updateFocus(IReadOnlyList<Component> root) {
		foreach (Component comp in root) {
			if (comp.IsAccessible()) {
				Focused.Add(comp);
				
				// Recursively check all that satisfies the condition.
				if (comp.Children.Count > 0) {
					updateFocus(comp.Children);
				}
			}
		}
	}
}
