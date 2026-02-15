// #define HIGHP

namespace Mino.Mathematics;

/// <summary>
///     Provides tabled fast trigonometric calculation.
/// </summary>
public static class FastTrigonometric {
	private const int TableSize = 4096;
	private const float TwoPi = MathF.PI * 2.0F;
	private const float InvTwoPi = 1.0F / TwoPi;

	/// <summary>
	///     Gets the sine and cosine value of a radian in a pre-calculated table.
	/// </summary>
	/// <param name="rad">The target radian.</param>
	/// <param name="sin">Output sine.</param>
	/// <param name="cos">Output cosine.</param>
	public static void Get(float rad, out float sin, out float cos) {
#if HIGHP
		(sin, cos) = MathF.SinCos(rad);
#else
		float index = mod(rad) * InvTwoPi * TableSize;
		int indexInt = (int) index & TableSize - 1;
		float fraction = index - indexInt;
		// Find sine.
		int nextIndex = indexInt + 1 & TableSize - 1;
		indexInt &= TableSize - 1;
		sin = lerp(_table[indexInt], _table[nextIndex], fraction);
		// Find cosine.
		int cosIndex = indexInt + TableSize / 4 & TableSize - 1;
		int cosNextIndex = cosIndex + 1 & TableSize - 1;
		cos = lerp(_table[cosIndex], _table[cosNextIndex], fraction);
#endif
	}

	private static float mod(float rad) {
		rad %= TwoPi;
		if (rad < 0) {
			rad += TwoPi;
		}
		return rad;
	}

	private static float lerp(float a, float b, float t) {
		return a + (b - a) * t;
	}

#if !HIGHP
	private static readonly float[] _table = new float[TableSize];

	static FastTrigonometric() {
		for (int i = 0; i < TableSize; i++) {
			float angle = i * TwoPi / TableSize;
			_table[i] = MathF.Sin(angle);
		}
	}
#endif
}
