namespace Mino.Mathematics;

/// <summary>
///     Immutable 3D bounding box.
/// </summary>
public readonly struct Box3 : IEquatable<Box3> {
	public static readonly Box3 Empty = CreateCentral(
		float.NaN, float.NaN, float.NaN, 0.0F, 0.0F, 0.0F);

	public readonly Vector3 Min;
	public readonly Vector3 Max;

	/// <summary>
	///     Initializes a new instance of the <see cref="Box3" /> struct.
	/// </summary>
	/// <param name="min">Minimum corner.</param>
	/// <param name="max">Maximum corner.</param>
	/// <exception cref="Error">Thrown when min is greater than max.</exception>
	public Box3(in Vector3 min, in Vector3 max) {
		Min = min;
		Max = max;
		if (min.X > max.X || min.Y > max.Y || min.Z > max.Z) {
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
	///     Gets the minimum Z coordinate.
	/// </summary>
	public float MinZ {
		get => Min.Z;
	}

	/// <summary>
	///     Gets the maximum Z coordinate.
	/// </summary>
	public float MaxZ {
		get => Max.Z;
	}

	/// <summary>
	///     Size of the box.
	/// </summary>
	public Vector3 Center {
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
	///     Gets the Z coordinate of the center.
	/// </summary>
	public float CentralZ {
		get => (MinZ + MaxZ) * 0.5F;
	}

	/// <summary>
	///     Size of the box.
	/// </summary>
	public Vector3 Size {
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
	///     Gets the depth of the bounding box.
	/// </summary>
	public float Depth {
		get => MaxZ - MinZ;
	}

	/// <summary>
	///     Gets the volume of the bounding box.
	/// </summary>
	public float Volume {
		get => Width * Height * Depth;
	}

	/// <summary>
	///     Checks if this bounding box intersects with another.
	/// </summary>
	/// <param name="other">The other bounding box to test.</param>
	/// <returns>True if the boxes intersect, otherwise false.</returns>
	public bool Intersects(in Box3 other) {
		return MinX <= other.MaxX && MaxX >= other.MinX
			&& MinY <= other.MaxY && MaxY >= other.MinY
			&& MinZ <= other.MaxZ && MaxZ >= other.MinZ;
	}

	/// <summary>
	///     Checks if a point is contained within this bounding box.
	/// </summary>
	/// <param name="x">X coordinate of the point.</param>
	/// <param name="y">Y coordinate of the point.</param>
	/// <param name="z">Z coordinate of the point.</param>
	/// <returns>True if the point is inside the box, otherwise false.</returns>
	public bool Contains(float x, float y, float z) {
		return x >= MinX && x <= MaxX
			&& y >= MinY && y <= MaxY
			&& z >= MinZ && z <= MaxZ;
	}

	/// <summary>
	///     Checks if a point is contained within this bounding box.
	/// </summary>
	/// <param name="pos">The point to test.</param>
	/// <returns>True if the point is inside the box, otherwise false.</returns>
	public bool Contains(in Vector3 pos) {
		return Contains(pos.X, pos.Y, pos.Z);
	}

	/// <summary>
	///     Checks if a box is contained within this bounding box.
	/// </summary>
	/// <param name="box">The box to test.</param>
	/// <returns>True if the box is inside this box, otherwise false.</returns>
	public bool Contains(in Box3 box) {
		return Contains(box.Min) && Contains(box.Max);
	}

	/// <summary>
	///     Gets the intersection of two bounding boxes.
	/// </summary>
	/// <param name="a">First bounding box.</param>
	/// <param name="b">Second bounding box.</param>
	/// <returns>The intersection bounding box, or Empty if they don't intersect.</returns>
	public static Box3 GetIntersection(in Box3 a, in Box3 b) {
		if (!a.Intersects(b)) {
			return Empty;
		}
		return new Box3(Vector3.Max(a.Min, b.Min), Vector3.Min(a.Max, b.Max));
	}

	/// <summary>
	///     Gets the union of two bounding boxes.
	/// </summary>
	/// <param name="a">First bounding box.</param>
	/// <param name="b">Second bounding box.</param>
	/// <returns>The union bounding box.</returns>
	public static Box3 GetUnion(in Box3 a, in Box3 b) {
		return new Box3(Vector3.Min(a.Min, b.Min), Vector3.Max(a.Max, b.Max));
	}

	/// <summary>
	///     Inflates the bounding box by the specified delta.
	/// </summary>
	/// <param name="delta">The amount to inflate on all sides.</param>
	/// <returns>A new inflated bounding box.</returns>
	public Box3 Inflate(in Vector3 delta) {
		return new Box3(Min - delta, Max + delta);
	}

	/// <summary>
	///     Inflates the bounding box by the specified amounts.
	/// </summary>
	/// <param name="dx">Amount to inflate in the X direction.</param>
	/// <param name="dy">Amount to inflate in the Y direction.</param>
	/// <param name="dz">Amount to inflate in the Z direction.</param>
	/// <returns>A new inflated bounding box.</returns>
	public Box3 Inflate(float dx, float dy, float dz) {
		return Inflate(new Vector3(dx, dy, dz));
	}

	/// <summary>
	///     Scales the bounding box by the specified scalar.
	/// </summary>
	/// <param name="scalar">The scaling factor.</param>
	/// <returns>A new scaled bounding box.</returns>
	public Box3 Scale(in Vector3 scalar) {
		return new Box3(Min.Scale(scalar), Max.Scale(scalar));
	}

	/// <summary>
	///     Scales the bounding box by the specified amounts.
	/// </summary>
	/// <param name="scalarX">Scaling factor in the X direction.</param>
	/// <param name="scalarY">Scaling factor in the Y direction.</param>
	/// <param name="scalarZ">Scaling factor in the Z direction.</param>
	/// <returns>A new scaled bounding box.</returns>
	public Box3 Scale(float scalarX, float scalarY, float scalarZ) {
		return Scale(new Vector3(scalarX, scalarY, scalarZ));
	}
	
	/// <summary>
	///     Scales the bounding box by the specified amounts.
	/// </summary>
	/// <param name="scalar">Scaling factor.</param>
	/// <returns>A new scaled bounding box.</returns>
	public Box3 Scale(float scalar) {
		return Scale(scalar, scalar, scalar);
	}

	/// <summary>
	///     Translates the bounding box by the specified translation vector.
	/// </summary>
	/// <param name="translation">The translation vector.</param>
	/// <returns>A new translated bounding box.</returns>
	public Box3 Translate(in Vector3 translation) {
		return new Box3(Min + translation, Max + translation);
	}

	/// <summary>
	///     Translates the bounding box by the specified amounts.
	/// </summary>
	/// <param name="dx">Translation in the X direction.</param>
	/// <param name="dy">Translation in the Y direction.</param>
	/// <param name="dz">Translation in the Z direction.</param>
	/// <returns>A new translated bounding box.</returns>
	public Box3 Translate(float dx, float dy, float dz) {
		return Translate(new Vector3(dx, dy, dz));
	}

	/// <summary>
	///     Creates a bounding box from position and size.
	/// </summary>
	/// <param name="x">X coordinate of the minimum corner.</param>
	/// <param name="y">Y coordinate of the minimum corner.</param>
	/// <param name="z">Z coordinate of the minimum corner.</param>
	/// <param name="width">Width of the box.</param>
	/// <param name="height">Height of the box.</param>
	/// <param name="depth">Depth of the box.</param>
	/// <returns>A new bounding box.</returns>
	public static Box3 Create(float x, float y, float z, float width, float height, float depth) {
		return new Box3(new Vector3(x, y, z), new Vector3(x + width, y + height, z + depth));
	}

	/// <summary>
	///     Creates a bounding box from position and size.
	/// </summary>
	/// <param name="pos">Position of the minimum corner.</param>
	/// <param name="size">Size of the box.</param>
	/// <returns>A new bounding box.</returns>
	public static Box3 Create(in Vector3 pos, in Vector3 size) {
		return new Box3(pos, pos + size);
	}

	/// <summary>
	///     Creates a bounding box centered at the specified position.
	/// </summary>
	/// <param name="centerX">X coordinate of the center.</param>
	/// <param name="centerY">Y coordinate of the center.</param>
	/// <param name="centerZ">Z coordinate of the center.</param>
	/// <param name="width">Width of the box.</param>
	/// <param name="height">Height of the box.</param>
	/// <param name="depth">Depth of the box.</param>
	/// <returns>A new centered bounding box.</returns>
	public static Box3 CreateCentral(float centerX, float centerY, float centerZ, float width,
		float height, float depth) {
		return Create(
			centerX - width * 0.5F, centerY - height * 0.5F, centerZ - depth * 0.5F, width, height,
			depth);
	}

	/// <summary>
	///     Creates a bounding box centered at the specified position.
	/// </summary>
	/// <param name="pos">Center position.</param>
	/// <param name="size">Size of the box.</param>
	/// <returns>A new centered bounding box.</returns>
	public static Box3 CreateCentral(in Vector3 pos, in Vector3 size) {
		return Create(pos - size * 0.5F, size);
	}

	/// <summary>
	///     Creates a bounding box from two arbitrary points.
	/// </summary>
	/// <param name="x1">X coordinate of the first point.</param>
	/// <param name="y1">Y coordinate of the first point.</param>
	/// <param name="z1">Z coordinate of the first point.</param>
	/// <param name="x2">X coordinate of the second point.</param>
	/// <param name="y2">Y coordinate of the second point.</param>
	/// <param name="z2">Z coordinate of the second point.</param>
	/// <returns>A new bounding box enclosing both points.</returns>
	public static Box3 CreateByPoints(float x1, float y1, float z1, float x2, float y2, float z2) {
		return Create(
			Math.Min(x1, x2), Math.Min(y1, y2), Math.Min(z1, z2), Math.Abs(x2 - x1),
			Math.Abs(y2 - y1), Math.Abs(z2 - z1));
	}

	/// <summary>
	///     Creates a bounding box from two arbitrary points.
	/// </summary>
	/// <param name="p1">First point.</param>
	/// <param name="p2">Second point.</param>
	/// <returns>A new bounding box enclosing both points.</returns>
	public static Box3 CreateByPoints(in Vector3 p1, in Vector3 p2) {
		return new Box3(Vector3.Min(p1, p2), Vector3.Max(p1, p2));
	}

	// Implicit cast Box2 -> Box3.
	public static implicit operator Box3(in Box2 box2) {
		return new Box3(box2.Min, box2.Max);
	}

	public override string ToString() {
		return $"{Min} -> {Max} [{Width}, {Height}, {Depth}]";
	}

	public bool Equals(Box3 other) {
		return Min.Equals(other.Min) && Max.Equals(other.Max);
	}

	public override bool Equals(object? obj) {
		return obj is Box3 other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Min, Max);
	}

	public static bool operator ==(Box3 a, Box3 b) {
		return a.Equals(b);
	}

	public static bool operator !=(Box3 a, Box3 b) {
		return !a.Equals(b);
	}
}
