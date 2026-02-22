#region
using Mino.Audio;
using Mino.Audio.Desc;
using Mino.Input;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///     A GUI top canvas.
/// </summary>
public class Canvas {
	public static readonly Key[] Keymap = [
		Key.Get(Key.MouseLeft) // Focus key
	];

	/// <summary>
	///     Currently focused component.
	/// </summary>
	public HashSet<Component?> Focused { get; } = new HashSet<Component?>();

	/// <summary>
	///     Present faces.
	/// </summary>
	public readonly List<Face> Presents = new List<Face>();

	/// <summary>
	///     Current gui system sound emitter.
	/// </summary>
	public Emitter SoundEmitter { get; set; } = new Emitter("gui");

	/// <summary>
	///     Plays a sound in GUI emitter.
	/// </summary>
	/// <param name="line">Source line.</param>
	public void PlaySound(DataLine? line) {
		if (line == null) {
			return;
		}
		ClipDesc cDesc = ClipDesc.FromDataLine(line);
		Clip clip = AudioSystem.Create<Clip>(cDesc);
		SoundEmitter.Play(clip);
	}

	/// <summary>
	///     Displays the face.
	/// </summary>
	/// <param name="face">Face to display.</param>
	public void Display(Face face) {
		if (!Presents.Contains(face)) {
			Presents.Add(face);
		}
		face.SetAttribute("CanvasFactory", () => this);
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
	/// <param name="ctx">Current canvas context.</param>
	public void Update(CanvasContext ctx) {
		foreach (Face face in Presents) {
			face.Update(ctx);
		}

		// Reflush focused component.
		if (Keymap[0].Press) {
			Focused.Clear();
			updateFocus(Presents, ctx.Cursor);
		}
	}

	/// <summary>
	///     Draws all present faces.
	/// </summary>
	/// <param name="ctx">Current canvas context.</param>
	public void Draw(CanvasContext ctx) {
		foreach (Face face in Presents) {
			face.Draw(ctx);
		}
	}

	private void updateFocus(IReadOnlyList<Component> root, in Vector2 cursor) {
		foreach (Component comp in root) {
			if (comp.IsAccessible(cursor)) {
				Focused.Add(comp);
				if (comp.Children.Count > 0) {
					updateFocus(comp.Children, cursor);
				}
			}
		}
	}
}
