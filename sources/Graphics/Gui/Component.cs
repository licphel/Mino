#region
using Mino.Mathematics;
#endregion

namespace Mino.Graphics.Gui;

/// <summary>
///     User interface component base class.
/// </summary>
public abstract class Component {
	private readonly List<Component> _children = new List<Component>();
	private Dictionary<string, object?> _attrMap = new Dictionary<string, object?>();

	public Action<Component, CanvasContext>? OnUpdate;
	public Action<Component, CanvasContext>? OnDraw;
	public Action<Component, CanvasContext>? OnResolve;

	/// <summary>
	///     Whether the component is under the cursor.
	/// </summary>
	public bool Hovering { get; protected set; }

	/// <summary>
	///     Affiliated canvas.
	///     Ensured nonnull after added into a face.
	/// </summary>
	public Canvas Canvas {
		get {
			var fn = (Func<Canvas>) GetAttribute("CanvasFactory")!;
			return fn.Invoke();
		}
	}

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
	public IReadOnlyList<Component> Children {
		get => _children;
	}

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
	/// <exception cref="Error">Thrown if the child is already another comp's child.</exception>
	public void AddChild(Component child) {
		if (child.Parent != null) {
			throw new Error("multiple parent");
		}

		_children.Add(child);
		child.Parent = this;
		child.InitHooks();

		/*
		 * We use this 'cascade' factory to implement deferred canvas injection.
		 *
		 * Canvas0
		 * |- Face: CanvasFactory1 = () => Canvas0
		 *		|- Comp 1 CanvasFactory2 = () => CanvasFactory1.Invoke()
		 *			|- ...
		 */
		child.SetAttribute("CanvasFactory", () => Canvas);
	}

	/// <summary>
	///     Removes a child.
	/// </summary>
	/// <param name="child">Child to remove</param>
	public void RemoveChild(Component child) {
		if (_children.Remove(child)) {
			child.Parent = null;
			child.FreeHooks();
		}
	}

	/// <summary>
	///     Clears all children.
	/// </summary>
	public void ClearChildren() {
		foreach (Component child in _children) {
			child.Parent = null;
		}
		_children.Clear();
	}

	/// <summary>
	///     Updates the component.
	/// </summary>
	/// <param name="ctx">Current canvas context.</param>
	public virtual void Update(CanvasContext ctx) {
		OnUpdate?.Invoke(this, ctx);

		foreach (Component child in _children) {
			child.Update(ctx);
		}

		Hovering = IsAccessible(ctx.Cursor);
	}

	/// <summary>
	///     Draws the component.
	/// </summary>
	/// <param name="ctx">Current canvas context.</param>
	public virtual void Draw(CanvasContext ctx) {
		OnDraw?.Invoke(this, ctx);

		foreach (Component child in _children) {
			child.Draw(ctx);
		}
	}

	/// <summary>
	///     Called on resolved.
	/// </summary>
	/// <param name="ctx">Current canvas context.</param>
	public virtual void Resolve(CanvasContext ctx) {
		OnResolve?.Invoke(this, ctx);

		foreach (Component child in _children) {
			child.Resolve(ctx);
		}
	}

	/// <summary>
	///     Called on tooltip appending.
	/// </summary>
	/// <param name="ctx">Tooltip context.</param>
	public virtual void AppendTooltip(TooltipContext ctx) {
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
	/// <param name="cursor">Cursor position.</param>
	/// <returns>True is accessible, otherwise false.</returns>
	public virtual bool IsAccessible(in Vector2 cursor) {
		if (!Contains(cursor)) {
			return false;
		}

		if (Parent != null) {
			if (Parent.Contains(cursor) && !Parent.IsAccessible(cursor)) {
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
