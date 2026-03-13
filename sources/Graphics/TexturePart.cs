#region
using Mino.Mathematics;
using Mino.Utility;
using Mino.Utility.Logging;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Represents a part of a texture.
/// </summary>
public readonly struct TexturePart : IEquatable<TexturePart> {
	public readonly FragileTexture Src;
	public readonly Box3 Region;

	public TexturePart(FragileTexture tex) : this(tex, Box3.Create(0.0F, 0.0F, 0.0F, tex.Width, tex.Height, tex.Depth)) {
	}

	public TexturePart(FragileTexture src, in Box3 region) {
		Src = src;
		Region = region;
	}

	/// <summary>
	///     Creates a texture part based on another part. It will be transformed to a absolute region.
	/// </summary>
	/// <param name="part">A texture part.</param>
	/// <param name="region">A region in the given part.</param>
	/// <exception cref="Crash">Thrown if the region is outside of the given part.</exception>
	public TexturePart(in TexturePart part, in Box3 region) {
		Src = part.Src;
		Region = region.Translate(part.Region.Min);

		if (!part.Region.Contains(Region)) {
			Log.Warn("Region is outside of the parent part");
		}
	}

	/// <summary>
	///     Texture part u.
	/// </summary>
	public float U {
		get => Region.MinX;
	}

	/// <summary>
	///     Texture part v.
	/// </summary>
	public float V {
		get => Region.MinY;
	}

	/// <summary>
	///     Texture part width.
	/// </summary>
	public float Width {
		get => Region.Width;
	}

	/// <summary>
	///     Texture part height.
	/// </summary>
	public float Height {
		get => Region.Height;
	}

	/// <summary>
	///     Gets two clamping U-V vectors of the part.
	/// </summary>
	/// <param name="a">Negative-side U-V.</param>
	/// <param name="b">Positive-side U-V.</param>
	public void GetCoordinates2D(out Vector2 a, out Vector2 b) {
		Texture src = Src.Pin();
		a = new Vector2(Region.MinX / src.Width, Region.MinY / src.Height);
		b = new Vector2(Region.MaxX / src.Width, Region.MaxY / src.Height);
	}

	/// <summary>
	///     Gets two clamping U-V-W vectors of the part.
	/// </summary>
	/// <param name="a">Negative-side U-V-W.</param>
	/// <param name="b">Positive-side U-V-W.</param>
	public void GetCoordinates3D(out Vector3 a, out Vector3 b) {
		Texture src = Src.Pin();
		a = new Vector3(Region.MinX / src.Width, Region.MinY / src.Height, Region.MinZ / src.Depth);
		b = new Vector3(Region.MaxX / src.Width, Region.MaxY / src.Height, Region.MaxZ / src.Depth);
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
