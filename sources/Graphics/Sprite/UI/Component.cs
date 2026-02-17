#region
using Mino.Framework;
using Mino.Input;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Sprite.UI;

/// <summary>
///     User interface component base class.
/// </summary>
public abstract class Component {
	private readonly List<Component> _children = new List<Component>();

	/// <summary>
	///     Name of the component.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	///     Parent of the component.
	/// </summary>
	public Component? Parent { get; private set; }

	/// <summary>
	///     Max bounding box of the component. This won't determine the visual size.
	/// </summary>
	public Box2 BoundingBox { get; set; }

	/// <summary>
	///     Whether the component will be rendered.
	/// </summary>
	public bool IsVisible { get; set; } = true;

	/// <summary>
	///     Whether the component will make effects.
	/// </summary>
	public bool IsInteractive { get; set; } = true;

	/// <summary>
	///		Depth of the component.
	/// </summary>
	public float Depth { get; set; } = 1.0F;

	/// <summary>
	///     Children of the component.
	/// </summary>
	public IReadOnlyList<Component> Children {
		get => _children;
	}

	public Action<Component, TimeStep>? OnUpdate;
	public Action<Component, Brush>? OnDraw;
	public Action<Component, MappingContext>? OnRemap;

	public virtual bool Contains(in Vector2 point) {
		return BoundingBox.Contains(point);
	}

	/// <summary>
	///     Adds a child to the component.
	/// </summary>
	/// <param name="child">Child to add.</param>
	/// <exception cref="Error">Thrown if the child is already another comp's child.</exception>
	public virtual void AddChild(Component child) {
		if (child.Parent != null) {
			throw new Error("multiple parent");
		}

		_children.Add(child);
		child.Parent = this;
	}

	/// <summary>
	///     Removes a child.
	/// </summary>
	/// <param name="child">Child to remove</param>
	public virtual void RemoveChild(Component child) {
		if (_children.Remove(child)) {
			child.Parent = null;
		}
	}

	/// <summary>
	///     Clears all children.
	/// </summary>
	public virtual void ClearChildren() {
		foreach (Component child in _children) {
			child.Parent = null;
		}
		_children.Clear();
	}

	/// <summary>
	///     Updates the component.
	/// </summary>
	/// <param name="step">Timestep.</param>
	public virtual void Update(TimeStep step) {
		if (!IsInteractive) {
			return;
		}

		OnUpdate?.Invoke(this, step);

		foreach (Component child in _children) {
			child.Update(step);
		}
	}

	/// <summary>
	///     Draws the component.
	/// </summary>
	/// <param name="brush">Drawing brush.</param>
	public virtual void Draw(Brush brush) {
		if (!IsVisible) {
			return;
		}

		OnDraw?.Invoke(this, brush);

		foreach (Component child in _children) {
			child.Draw(brush);
		}
	}

	/// <summary>
	///     Currently focused component.
	/// </summary>
	public static Component? Focused { get; internal set; }
	
	/// <summary>
	///		Checks if a component is accessible by the cursor.
	/// </summary>
	/// <param name="cursor">Cursor position.</param>
	/// <returns>True is accessible, otherwise false.</returns>
	public bool IsAccessible(in Vector2 cursor) {
		if (Parent == null) {
			return Contains(cursor);
		}
		Component? comp = Parent;
		
		// First layer: we check depth.
		foreach (Component child in comp.Children) {
			if (child != this && child.Depth < Depth && child.Contains(cursor)) {
				return false;
			}
		}
		
		// Other layers: depth is ignored.
		while (comp != null) {
			foreach (Component child in comp.Children) {
				if (child.Contains(cursor)) {
					return false;
				}
			}
			comp = comp.Parent;
		}

		return true;
	}
}
