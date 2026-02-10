// #define HIGHP

namespace Mino.Mathematics;

/// <summary>
///     Provides tabled fast trigonometric calculation.
/// </summary>
public static class FastTrigonometric {
	private const int _TABLE_SIZE = 4096;
	private const float _TWO_PI = MathF.PI * 2.0F;
	private const float _INV_TWO_PI = 1.0F / _TWO_PI;

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
		float index = mod(rad) * _INV_TWO_PI * _TABLE_SIZE;
		int indexInt = (int) index & _TABLE_SIZE - 1;
		float fraction = index - indexInt;
		// Find sine.
		int nextIndex = indexInt + 1 & _TABLE_SIZE - 1;
		indexInt &= _TABLE_SIZE - 1;
		sin = lerp(_table[indexInt], _table[nextIndex], fraction);
		// Find cosine.
		int cosIndex = indexInt + _TABLE_SIZE / 4 & _TABLE_SIZE - 1;
		int cosNextIndex = cosIndex + 1 & _TABLE_SIZE - 1;
		cos = lerp(_table[cosIndex], _table[cosNextIndex], fraction);
#endif
	}

	private static float mod(float rad) {
		rad %= _TWO_PI;
		if (rad < 0) {
			rad += _TWO_PI;
		}
		return rad;
	}

	private static float lerp(float a, float b, float t) {
		return a + (b - a) * t;
	}

#if !HIGHP
	private static readonly float[] _table = new float[_TABLE_SIZE];

	static FastTrigonometric() {
		for (int i = 0; i < _TABLE_SIZE; i++) {
			float angle = i * _TWO_PI / _TABLE_SIZE;
			_table[i] = MathF.Sin(angle);
		}
	}
#endif
}
