using Mino.Mathematics;

namespace Mino.Graphics;

/// <summary>
///     Represents a part of a texture.
/// </summary>
public readonly struct TexturePart : IEquatable<TexturePart> {
	public readonly Texture Src;
	public readonly Box3 Region;

	public TexturePart(Texture src, in Box3 region) {
		Src = src;
		Region = region;
	}

	/// <summary>
	///     Creates a texture part based on another part. It will be transformed to a absolute region.
	/// </summary>
	/// <param name="part">A texture part.</param>
	/// <param name="region">A region in the given part.</param>
	/// <exception cref="InvalidOperationException">Thrown if the region is outside of the given part.</exception>
	public TexturePart(in TexturePart part, in Box3 region) {
		Src = part.Src;
		Region = region.Translate(part.Region.Min);

		if (!part.Region.Contains(Region)) {
			throw new InvalidOperationException("Region is outside of the parent part.");
		}
	}

	/// <summary>
	///     Gets two clamping U-V vectors of the part.
	/// </summary>
	/// <param name="a">Negative-side U-V.</param>
	/// <param name="b">Positive-side U-V.</param>
	public void GetCoordinates2D(out Vector2 a, out Vector2 b) {
		a = new Vector2(Region.MinX / Src.Width, Region.MinY / Src.Height);
		b = new Vector2(Region.MaxX / Src.Width, Region.MaxY / Src.Height);
	}

	/// <summary>
	///     Gets two clamping U-V-W vectors of the part.
	/// </summary>
	/// <param name="a">Negative-side U-V-W.</param>
	/// <param name="b">Positive-side U-V-W.</param>
	public void GetCoordinates3D(out Vector3 a, out Vector3 b) {
		a = new Vector3(Region.MinX / Src.Width, Region.MinY / Src.Height, Region.MinZ / Src.Depth);
		b = new Vector3(Region.MaxX / Src.Width, Region.MaxY / Src.Height, Region.MaxZ / Src.Depth);
	}

	// Implicit cast Texture -> TexturePart.
	public static implicit operator TexturePart(Texture texture) {
		return new TexturePart(texture, Box3.Create(0.0F, 0.0F, 0.0F, texture.Width, texture.Height, texture.Depth));
	}

	public bool Equals(TexturePart other) {
		return Src.Equals(other.Src) && Region.Equals(other.Region);
	}

	public override bool Equals(object? obj) {
		return obj is TexturePart other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Src, Region);
	}

	public static bool operator ==(TexturePart left, TexturePart right) {
		return left.Equals(right);
	}

	public static bool operator !=(TexturePart left, TexturePart right) {
		return !left.Equals(right);
	}
}
