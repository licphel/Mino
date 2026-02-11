namespace Mino.Mathematics;

/// <summary>
///     Immutable 2D bounding box.
/// </summary>
public readonly struct Box2 : IEquatable<Box2> {
	public static readonly Box2 Empty = CreateCentral(float.NaN, float.NaN, 0.0F, 0.0F);

	public readonly Vector2 Min;
	public readonly Vector2 Max;

	/// <summary>
	///     Initializes a new instance of the <see cref="Box2" /> struct.
	/// </summary>
	/// <param name="min">Minimum corner.</param>
	/// <param name="max">Maximum corner.</param>
	/// <exception cref="Error">Thrown when min is greater than max.</exception>
	public Box2(in Vector2 min, in Vector2 max) {
		Min = min;
		Max = max;
		if (min.X > max.X || min.Y > max.Y) {
			throw new Error("max < min");
		}
	}

	/// <summary>
	///     Gets the minimum X coordinate.
	/// </summary>
	public float MinX {
		get => Min.X;
	}

	/// <summary>
	///     Gets the maximum X coordinate.
	/// </summary>
	public float MaxX {
		get => Max.X;
	}

	/// <summary>
	///     Gets the minimum Y coordinate.
	/// </summary>
	public float MinY {
		get => Min.Y;
	}

	/// <summary>
	///     Gets the maximum Y coordinate.
	/// </summary>
	public float MaxY {
		get => Max.Y;
	}

	/// <summary>
	///     Size of the box.
	/// </summary>
	public Vector2 Center {
		get => (Max + Min) * 0.5F;
	}

	/// <summary>
	///     Gets the X coordinate of the center.
	/// </summary>
	public float CentralX {
		get => (MinX + MaxX) * 0.5F;
	}

	/// <summary>
	///     Gets the Y coordinate of the center.
	/// </summary>
	public float CentralY {
		get => (MinY + MaxY) * 0.5F;
	}

	/// <summary>
	///     Size of the box.
	/// </summary>
	public Vector2 Size {
		get => Max - Min;
	}

	/// <summary>
	///     Gets the width of the bounding box.
	/// </summary>
	public float Width {
		get => MaxX - MinX;
	}

	/// <summary>
	///     Gets the height of the bounding box.
	/// </summary>
	public float Height {
		get => MaxY - MinY;
	}

	/// <summary>
	///     Gets the area of the bounding box.
	/// </summary>
	public float Area {
		get => Width * Height;
	}

	/// <summary>
	///     Checks if this bounding box intersects with another.
	/// </summary>
	/// <param name="other">The other bounding box to test.</param>
	/// <returns>True if the boxes intersect, otherwise false.</returns>
	public bool Intersects(in Box2 other) {
		return MinX <= other.MaxX && MaxX >= other.MinX && MinY <= other.MaxY && MaxY >= other.MinY;
	}

	/// <summary>
	///     Checks if a point is contained within this bounding box.
	/// </summary>
	/// <param name="x">X coordinate of the point.</param>
	/// <param name="y">Y coordinate of the point.</param>
	/// <returns>True if the point is inside the box, otherwise false.</returns>
	public bool Contains(float x, float y) {
		return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
	}

	/// <summary>
	///     Checks if a point is contained within this bounding box.
	/// </summary>
	/// <param name="pos">The point to test.</param>
	/// <returns>True if the point is inside the box, otherwise false.</returns>
	public bool Contains(in Vector2 pos) {
		return Contains(pos.X, pos.Y);
	}

	/// <summary>
	///     Checks if a box is contained within this bounding box.
	/// </summary>
	/// <param name="box">The box to test.</param>
	/// <returns>True if the box is inside this box, otherwise false.</returns>
	public bool Contains(in Box2 box) {
		return Contains(box.Min) && Contains(box.Max);
	}

	/// <summary>
	///     Gets the intersection of two bounding boxes.
	/// </summary>
	/// <param name="a">First bounding box.</param>
	/// <param name="b">Second bounding box.</param>
	/// <returns>The intersection bounding box, or Empty if they don't intersect.</returns>
	public static Box2 GetIntersection(in Box2 a, in Box2 b) {
		if (!a.Intersects(b)) {
			return Empty;
		}
		return new Box2(Vector2.Max(a.Min, b.Min), Vector2.Min(a.Max, b.Max));
	}

	/// <summary>
	///     Gets the union of two bounding boxes.
	/// </summary>
	/// <param name="a">First bounding box.</param>
	/// <param name="b">Second bounding box.</param>
	/// <returns>The union bounding box.</returns>
	public static Box2 GetUnion(in Box2 a, in Box2 b) {
		return new Box2(Vector2.Min(a.Min, b.Min), Vector2.Max(a.Max, b.Max));
	}

	/// <summary>
	///     Inflates the bounding box by the specified delta.
	/// </summary>
	/// <param name="delta">The amount to inflate on all sides.</param>
	/// <returns>A new inflated bounding box.</returns>
	public Box2 Inflate(in Vector2 delta) {
		return new Box2(Min - delta, Max + delta);
	}

	/// <summary>
	///     Inflates the bounding box by the specified amounts.
	/// </summary>
	/// <param name="dx">Amount to inflate horizontally.</param>
	/// <param name="dy">Amount to inflate vertically.</param>
	/// <returns>A new inflated bounding box.</returns>
	public Box2 Inflate(float dx, float dy) {
		return Inflate(new Vector2(dx, dy));
	}

	/// <summary>
	///     Scales the bounding box by the specified scalar.
	/// </summary>
	/// <param name="scalar">The scaling factor.</param>
	/// <returns>A new scaled bounding box.</returns>
	public Box2 Scale(in Vector2 scalar) {
		return new Box2(Min.Scale(scalar), Max.Scale(scalar));
	}

	/// <summary>
	///     Scales the bounding box by the specified amounts.
	/// </summary>
	/// <param name="scalarX">Scaling factor in the X direction.</param>
	/// <param name="scalarY">Scaling factor in the Y direction.</param>
	/// <returns>A new scaled bounding box.</returns>
	public Box2 Scale(float scalarX, float scalarY) {
		return Scale(new Vector2(scalarX, scalarY));
	}

	/// <summary>
	///     Translates the bounding box by the specified translation vector.
	/// </summary>
	/// <param name="translation">The translation vector.</param>
	/// <returns>A new translated bounding box.</returns>
	public Box2 Translate(in Vector2 translation) {
		return new Box2(Min + translation, Max + translation);
	}

	/// <summary>
	///     Translates the bounding box by the specified amounts.
	/// </summary>
	/// <param name="dx">Translation in the X direction.</param>
	/// <param name="dy">Translation in the Y direction.</param>
	/// <returns>A new translated bounding box.</returns>
	public Box2 Translate(float dx, float dy) {
		return Translate(new Vector2(dx, dy));
	}

	/// <summary>
	///     Creates a bounding box from position and size.
	/// </summary>
	/// <param name="x">X coordinate of the minimum corner.</param>
	/// <param name="y">Y coordinate of the minimum corner.</param>
	/// <param name="width">Width of the box.</param>
	/// <param name="height">Height of the box.</param>
	/// <returns>A new bounding box.</returns>
	public static Box2 Create(float x, float y, float width, float height) {
		return new Box2(new Vector2(x, y), new Vector2(x + width, y + height));
	}

	/// <summary>
	///     Creates a bounding box from position and size.
	/// </summary>
	/// <param name="pos">Position of the minimum corner.</param>
	/// <param name="size">Size of the box.</param>
	/// <returns>A new bounding box.</returns>
	public static Box2 Create(in Vector2 pos, in Vector2 size) {
		return new Box2(pos, pos + size);
	}

	/// <summary>
	///     Creates a bounding box centered at the specified position.
	/// </summary>
	/// <param name="centerX">X coordinate of the center.</param>
	/// <param name="centerY">Y coordinate of the center.</param>
	/// <param name="width">Width of the box.</param>
	/// <param name="height">Height of the box.</param>
	/// <returns>A new centered bounding box.</returns>
	public static Box2 CreateCentral(float centerX, float centerY, float width, float height) {
		return Create(centerX - width * 0.5F, centerY - height * 0.5F, width, height);
	}

	/// <summary>
	///     Creates a bounding box centered at the specified position.
	/// </summary>
	/// <param name="pos">Center position.</param>
	/// <param name="size">Size of the box.</param>
	/// <returns>A new centered bounding box.</returns>
	public static Box2 CreateCentral(in Vector2 pos, in Vector2 size) {
		return Create(pos - size * 0.5F, size);
	}

	/// <summary>
	///     Creates a bounding box from two arbitrary points.
	/// </summary>
	/// <param name="x1">X coordinate of the first point.</param>
	/// <param name="y1">Y coordinate of the first point.</param>
	/// <param name="x2">X coordinate of the second point.</param>
	/// <param name="y2">Y coordinate of the second point.</param>
	/// <returns>A new bounding box enclosing both points.</returns>
	public static Box2 CreateByPoints(float x1, float y1, float x2, float y2) {
		return Create(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1));
	}

	/// <summary>
	///     Creates a bounding box from two arbitrary points.
	/// </summary>
	/// <param name="p1">First point.</param>
	/// <param name="p2">Second point.</param>
	/// <returns>A new bounding box enclosing both points.</returns>
	public static Box2 CreateByPoints(in Vector2 p1, in Vector2 p2) {
		return new Box2(Vector2.Min(p1, p2), Vector2.Max(p1, p2));
	}

	// Implicit cast Box3 -> Box2.
	public static implicit operator Box2(in Box3 box3) {
		return new Box2(box3.Min, box3.Max);
	}

	public override string ToString() {
		return $"{Min} -> {Max} [{Width}, {Height}]";
	}

	public bool Equals(Box2 other) {
		return Min.Equals(other.Min) && Max.Equals(other.Max);
	}

	public override bool Equals(object? obj) {
		return obj is Box2 other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Min, Max);
	}

	public static bool operator ==(Box2 a, Box2 b) {
		return a.Equals(b);
	}

	public static bool operator !=(Box2 a, Box2 b) {
		return !a.Equals(b);
	}
}
