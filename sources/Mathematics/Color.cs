namespace Mino.Mathematics;

/// <summary>
///     Immutable float32 RGBA color.
/// </summary>
public readonly struct Color : IEquatable<Color> {
	public static readonly Color Empty = new Color(0.0F, 0.0F, 0.0F, 0.0F);
	public static readonly Color PureWhite = new Color(1.0F, 1.0F, 1.0F);
	public static readonly Color PureBlack = new Color(0.0F, 0.0F, 0.0F);
	public static readonly Color PureRed = new Color(1.0F, 0.0F, 0.0F);
	public static readonly Color PureGreen = new Color(0.0F, 1.0F, 0.0F);
	public static readonly Color PureBlue = new Color(0.0F, 0.0F, 1.0F);

	public readonly float Red = 1.0F;
	public readonly float Green = 1.0F;
	public readonly float Blue = 1.0F;
	public readonly float Alpha = 1.0F;

	public Color() {
	}

	public Color(float red, float green, float blue, float alpha = 1.0F) {
		Red = red;
		Green = green;
		Blue = blue;
		Alpha = alpha;
	}

	public Color(Color color, float alpha = 1.0F) {
		Red = color.Red;
		Green = color.Green;
		Blue = color.Blue;
		Alpha = alpha;
	}
	
	/// <summary>
	///     Gets an additive merge of the two colors.
	/// </summary>
	/// <param name="other">The other color.</param>
	/// <returns>A new merged color.</returns>
	public Color Add(in Color other) {
		return new Color(
			Red + other.Red, Green + other.Green, Blue + other.Blue, Alpha + other.Alpha);
	}

	/// <summary>
	///     Gets a merge of the two colors.
	/// </summary>
	/// <param name="other">The other color.</param>
	/// <returns>A new merged color.</returns>
	public Color Multiply(in Color other) {
		return new Color(
			Red * other.Red, Green * other.Green, Blue * other.Blue, Alpha * other.Alpha);
	}

	/// <summary>
	///     Multiplies the rgb components by the value.
	/// </summary>
	/// <param name="v">The merging value.</param>
	/// <returns>A new merged color</returns>
	public Color Multiply(float v) {
		return new Color(Red * v, Green * v, Blue * v, Alpha);
	}

	/// <summary>
	///     Get the invert color of the original color.
	/// </summary>
	public Color Invert() {
		return new Color(1.0F - Red, 1.0F - Green, 1.0F - Blue, Alpha);
	}

	/// <summary>
	///     Converts the color to 4 bytes for compression.
	/// </summary>
	public ulong AsHalves() {
		ulong result = 0;
		result |= (ulong) Half.Cast(Red) << 0;
		result |= (ulong) Half.Cast(Green) << 16;
		result |= (ulong) Half.Cast(Blue) << 32;
		result |= (ulong) Half.Cast(Alpha) << 48;
		return result;
	}

	/// <summary>
	///     Converts the color to 4 halves for compression.
	/// </summary>
	public uint AsBytes() {
		uint result = 0;
		result |= (uint) Math.Clamp(Red * 255.0F, 0, 255) << 0;
		result |= (uint) Math.Clamp(Green * 255.0F, 0, 255) << 8;
		result |= (uint) Math.Clamp(Blue * 255.0F, 0, 255) << 16;
		result |= (uint) Math.Clamp(Alpha * 255.0F, 0, 255) << 24;
		return result;
	}

	/// <summary>
	///     Create a rgba color from 0~255 bytes.
	/// </summary>
	public static Color Create(byte red, byte green, byte blue, byte alpha = 255) {
		return new Color(red / 255.0F, green / 255.0F, blue / 255.0F, alpha / 255.0F);
	}

	/// <summary>
	///     Converts hsv color to rgba color with the given alpha.
	/// </summary>
	/// <param name="hue">Hsv hue.</param>
	/// <param name="saturation">Hsv saturation.</param>
	/// <param name="value">Hsv value.</param>
	/// <param name="alpha">Desired alpha of the result.</param>
	/// <returns>A converted rgba color.</returns>
	public static Color HsvToRgb(float hue, float saturation, float value, float alpha = 1.0F) {
		int i = (int) (hue * 6) % 6;
		float f = hue * 6 - i;
		float f1 = value * (1 - saturation);
		float f2 = value * (1 - f * saturation);
		float f3 = value * (1 - (1 - f) * saturation);
		float f4 = 0;
		float f5 = 0;
		float f6 = 0;
		switch (i) {
			case 0:
				f4 = value;
				f5 = f3;
				f6 = f1;
				break;
			case 1:
				f4 = f2;
				f5 = value;
				f6 = f1;
				break;
			case 2:
				f4 = f1;
				f5 = value;
				f6 = f3;
				break;
			case 3:
				f4 = f1;
				f5 = f2;
				f6 = value;
				break;
			case 4:
				f4 = f3;
				f5 = f1;
				f6 = value;
				break;
			case 5:
				f4 = value;
				f5 = f1;
				f6 = f2;
				break;
		}
		return new Color(f4, f5, f6, alpha);
	}

	public bool Equals(Color other) {
		return Comparison.DoEqual(Red, other.Red)
			&& Comparison.DoEqual(Green, other.Green)
			&& Comparison.DoEqual(Blue, other.Blue)
			&& Comparison.DoEqual(Alpha, other.Alpha);
	}

	public override bool Equals(object? obj) {
		return obj is Color other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Red, Green, Blue, Alpha);
	}

	public static bool operator ==(Color left, Color right) {
		return left.Equals(right);
	}

	public static bool operator !=(Color left, Color right) {
		return !left.Equals(right);
	}

	public static Color operator *(in Color a, in Color b) {
		return a.Multiply(b);
	}

	public static Color operator ~(in Color c) {
		return c.Invert();
	}

	public static Color operator *(in Color c, float v) {
		return c.Multiply(v);
	}
}
