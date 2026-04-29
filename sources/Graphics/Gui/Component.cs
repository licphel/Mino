#region
using Mino.Framework;
using Mino.Graphics.Sprite;
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///     User interface component base class.
/// </summary>
public abstract class Component {
	private Dictionary<string, object?> _attrMap = new Dictionary<string, object?>();

	public Action<Component, TimeStep>? OnUpdate;
	public Action<Component, Brush, float>? OnDraw;
	public Action<Component>? OnResolve;

	internal Func<Canvas> _canvasSupplier = () => null!;

	/// <summary>
	///		Canvas that the component relies.
	/// </summary>
	public Canvas Canvas {
		get => _canvasSupplier();
	}
	
	/// <summary>
	///     Whether the component is under the cursor.
	/// </summary>
	public bool Hovering { get; protected set; }

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
	///     Depth of the component.
	/// </summary>
	public float Depth { get; set; } = 1.0F;

	/// <summary>
	///     Children of the component.
	/// </summary>
	public List<Component> Children { get; } = new List<Component>();

	/// <summary>
	///     Sets an attribute object.
	/// </summary>
	/// <param name="name">Attribute name.</param>
	/// <param name="obj">Set object.</param>
	public void SetAttribute(string name, object? obj) {
		_attrMap[name] = obj;
	}

	/// <summary>
	///     Gets an attribute object.
	/// </summary>
	/// <param name="name">Attribute name.</param>
	/// <returns>An nullable attribute object.</returns>
	public object? GetAttribute(string name) {
		return _attrMap.GetValueOrDefault(name);
	}

	/// <summary>
	///     Checks if a point is contained within the component's region.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>True if the point is inside the box, otherwise false.</returns>
	public virtual bool Contains(in Vector2 point) {
		return BoundingBox.Contains(point);
	}

	/// <summary>
	///     Adds a child to the component.
	/// </summary>
	/// <param name="child">Child to add.</param>
	/// <exception cref="InvalidOperationException">Thrown if the child is already another comp's child.</exception>
	public void AddChild(Component child) {
		if (child.Parent != null) {
			throw new InvalidOperationException("Cannot have multiple parent");
		}

		Children.Add(child);
		child.Parent = this;
		
		/*
		 * We use this 'cascade' factory to implement deferred canvas injection.
		 *
		 * Canvas0
		 * |- Face: _canvasSupplier1 = () => Canvas0
		 *		|- Comp 1 _canvasSupplier2 = () => _canvasSupplier1.Invoke()
		 *			|- ...
		 */
		child._canvasSupplier = () => Canvas;
		child.InitHooks();
	}

	/// <summary>
	///     Removes a child.
	/// </summary>
	/// <param name="child">Child to remove</param>
	public void RemoveChild(Component child) {
		if (Children.Remove(child)) {
			child.Parent = null;
			child.FreeHooks();
		}
	}

	/// <summary>
	///     Clears all children.
	/// </summary>
	public void ClearChildren() {
		foreach (Component child in Children) {
			child.Parent = null;
		}
		Children.Clear();
	}

	/// <summary>
	///     Updates the component.
	/// </summary>
	/// <param name="step">Time Steps.</param>
	public virtual void Update(in TimeStep step) {
		OnUpdate?.Invoke(this, step);

		foreach (Component child in Children) {
			UpdateChild(step, child);
		}

		Hovering = IsAccessible();
	}

	protected virtual void UpdateChild(in TimeStep step, Component child) {
		child.Update(step);
	}

	/// <summary>
	///     Draws the component.
	/// </summary>
	/// <param name="brush">Brush for drawing.</param>
	/// <param name="partial">Partial ticks.</param>
	public virtual void Draw(Brush brush, float partial) {
		OnDraw?.Invoke(this, brush, partial);
		
		foreach (Component child in Children) {
			brush.Depth = child.Depth;
			DrawChild(brush, partial, child);
		}
	}

	protected virtual void DrawChild(Brush brush, float partial, Component child) {
		child.Draw(brush, partial);
	}

	/// <summary>
	///     Called on resolved.
	/// </summary>
	public virtual void Resolve() {
		OnResolve?.Invoke(this);

		foreach (Component child in Children) {
			child.Resolve();
		}
	}

	/// <summary>
	///     Called on tooltip appending.
	/// </summary>
	/// <param name="ctx">Tooltip context.</param>
	public virtual void AppendTooltip(TooltipContext ctx) {
	}

	/// <summary>
	///		Makes the bounding box relative to its parent.
	/// </summary>
	public void FollowParent() {
		Parent?.HandleChildBox(this);
	}

	protected virtual void HandleChildBox(Component child) {
		child.BoundingBox = child.BoundingBox.Translate(BoundingBox.Min);
	}

	// Called when init.
	protected internal virtual void InitHooks() {
		foreach (Component child in Children) {
			child.InitHooks();
		}
	}

	// Called when disposed.
	protected internal virtual void FreeHooks() {
		foreach (Component child in Children) {
			child.FreeHooks();
		}
	}

	/// <summary>
	///     Checks if a component is accessible by the cursor.
	/// </summary>
	/// <returns>True is accessible, otherwise false.</returns>
	public virtual bool IsAccessible() {
		Vector2 cursor = Canvas.Cursor;
		
		if (!Contains(cursor)) {
			return false;
		}

		if (Parent != null) {
			if (Parent.Contains(cursor) && !Parent.IsAccessible()) {
				return false;
			}

			foreach (Component child in Parent.Children) {
				if (child != this && child.Depth < Depth && child.Contains(cursor)) {
					return false;
				}
			}
		}

		return true;
	}
}
