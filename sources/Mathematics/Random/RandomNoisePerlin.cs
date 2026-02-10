namespace Mino.Mathematics.Random;

/// <summary>
///     Perlin noise, according to Ken Perlin.
/// </summary>
public class RandomNoisePerlin : RandomNoise {
	private readonly int[] _perm = new int[512];
	private readonly int[] _permHalf = new int[256];

	public RandomNoisePerlin(RandomGenerator seed) {
		for (int i = 0; i < 256; i++) {
			_permHalf[i] = seed.NextInt(256);
		}
		for (int i = 0; i < 256; i++) {
			_perm[i + 256] = _perm[i] = _permHalf[i];
		}
	}

	public double Generate(double x, double y, double z) {
		int x0 = floor(x) & 255, y0 = floor(y) & 255, z0 = floor(z) & 255;
		x -= floor(x);
		y -= floor(y);
		z -= floor(z);
		double u = fade(x), v = fade(y), w = fade(z);
		int a = _perm[x0] + y0,
			aa = _perm[a] + z0,
			ab = _perm[a + 1] + z0,
			b = _perm[x0 + 1] + y0,
			ba = _perm[b] + z0,
			bb = _perm[b + 1] + z0;
		return lerp(
			w,
			lerp(
				v, lerp(u, grad(_perm[aa], x, y, z), grad(_perm[ba], x - 1, y, z)),
				lerp(u, grad(_perm[ab], x, y - 1, z), grad(_perm[bb], x - 1, y - 1, z))),
			lerp(
				v,
				lerp(u, grad(_perm[aa + 1], x, y, z - 1), grad(_perm[ba + 1], x - 1, y, z - 1)),
				lerp(
					u, grad(_perm[ab + 1], x, y - 1, z - 1),
					grad(_perm[bb + 1], x - 1, y - 1, z - 1)))) / 2.0 + 0.5;
	}

	private static int floor(double v) {
		int i = (int) v;
		return v >= i ? i : i - 1;
	}

	private static double fade(double t) {
		return t * t * t * (t * (t * 6 - 15) + 10);
	}

	private static double lerp(double t, double a, double b) {
		return a + t * (b - a);
	}

	private static double grad(int hash, double x, double y, double z) {
		int h = hash & 15;
		double u = h < 8 ? x : y, v = h < 4 ? y : h == 12 || h == 14 ? x : z;
		return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
	}
}
