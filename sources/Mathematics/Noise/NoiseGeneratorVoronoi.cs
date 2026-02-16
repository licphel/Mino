#region
using Mino.Mathematics.Random;
#endregion

namespace Mino.Mathematics.Noise;

/// <summary>
///     Quick voronoi noise.
/// </summary>
public class NoiseGeneratorVoronoi : NoiseGenerator {
	private readonly long _seed;

	public NoiseGeneratorVoronoi(RandomGenerator seed) {
		_seed = seed.NextInt();
	}

	public double Generate(double x, double y, double z) {
		int x0 = floor(x);
		int y0 = floor(y);
		int z0 = floor(z);
		double xc = 0;
		double yc = 0;
		double zc = 0;
		double md = int.MaxValue;
		for (int k = z0 - 2; k <= z0 + 2; k++) {
			for (int j = y0 - 2; j <= y0 + 2; j++) {
				for (int i = x0 - 2; i <= x0 + 2; i++) {
					double xp = i + hash(i, j, k, _seed);
					double yp = j + hash(i, j, k, _seed + 1);
					double zp = k + hash(i, j, k, _seed + 2);
					double xd = xp - x;
					double yd = yp - y;
					double zd = zp - z;
					double d = xd * xd + yd * yd + zd * zd;

					if (d < md) {
						md = d;
						xc = xp;
						yc = yp;
						zc = zp;
					}
				}
			}
		}
		return hash(floor(xc), floor(yc), floor(zc), 0);
	}

	private static int floor(double v) {
		int i = (int) v;
		return v >= i ? i : i - 1;
	}

	private static double hash(int x, int y, int z, long seed) {
		ulong hash = 14695981039346656037UL;
		hash ^= (ulong) x;
		hash *= 1099511628211UL;
		hash ^= (ulong) y;
		hash *= 1099511628211UL;
		hash ^= (ulong) z;
		hash *= 1099511628211UL;
		hash ^= (ulong) seed;
		hash *= 1099511628211UL;
		hash ^= hash >> 33;
		hash *= 0xff51afd7ed558ccdUL;
		hash ^= hash >> 33;
		hash *= 0xc4ceb9fe1a85ec53UL;
		hash ^= hash >> 33;
		return (hash & 0x1FFFFFFFFFFFFFUL) / (double) 0x1FFFFFFFFFFFFFUL;
	}
}
