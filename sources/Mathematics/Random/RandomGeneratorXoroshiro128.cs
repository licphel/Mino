namespace Mino.Mathematics.Random;

/// <summary>
///     Xoroshiro128+ random generator.
/// </summary>
public class RandomGeneratorXoroshiro128 : RandomGenerator {
	public const ulong XOR128_HEADER = 128UL;
	private const double _DOUBLE_NORM = 1.0 / (1UL << 53);

	private ulong _stateA;
	private ulong _stateB;
	private ulong _stateIA;
	private ulong _stateIB;

	public RandomGeneratorXoroshiro128(ulong seed) {
		ulong x = seed;
		_stateIA = _stateA = splitMix64(ref x);
		_stateIB = _stateB = splitMix64(ref x);
	}

	public RandomGeneratorXoroshiro128(ulong stateA, ulong stateB) {
		if (stateA == 0 && stateB == 0) {
			throw new Error("state cannot be all zeros");
		}
		_stateIA = _stateA = stateA;
		_stateIB = _stateB = stateB;
	}

	public RandomGeneratorXoroshiro128() : this((ulong) DateTime.UtcNow.Ticks) {
	}

	public double NextDouble() {
		return (nextULong() >> 11) * _DOUBLE_NORM;
	}

	public int NextInt(int bound) {
		if (bound <= 0) {
			throw new Error(nameof(bound), "Bound must be positive.");
		}

		ulong mask = ~0UL;
		mask >>= leadingZerosCount((ulong) bound - 1);

		ulong r;
		do {
			r = nextULong() & mask;
		} while (r >= (ulong) bound);

		return (int) r;
	}

	public ulong[] InitialState {
		get => [XOR128_HEADER, _stateIA, _stateIB];
	}

	public ulong[] State {
		get => [XOR128_HEADER, _stateA, _stateB];
	}

	public RandomGenerator Drift(ulong x) {
		ulong newSeedA = _stateA ^ splitMix64(ref x);
		ulong newSeedB = _stateB ^ splitMix64(ref x);
		return new RandomGeneratorXoroshiro128(newSeedA, newSeedB);
	}

	public RandomGenerator Jump() {
		return new RandomGeneratorXoroshiro128(_stateA, _stateB).jump(
			[0x900294d8f554a5, 0x170865df4b3201fc]);
	}

	public void Recover(ulong[] state) {
		if (state.Length != 3 || state[0] != XOR128_HEADER) {
			throw new Error("unknown state format");
		}
		_stateIA = _stateA = state[1];
		_stateIB = _stateB = state[2];
	}

	public RandomGenerator CopyOriginally() {
		return new RandomGeneratorXoroshiro128(_stateIA, _stateIB);
	}

	public RandomGenerator CopyCurrently() {
		return new RandomGeneratorXoroshiro128(_stateA, _stateB);
	}

	private ulong nextULong() {
		ulong s0 = _stateA;
		ulong s1 = _stateB;
		ulong result = s0 + s1;
		s1 ^= s0;
		_stateA = rotateLeft(s0, 24) ^ s1 ^ s1 << 16;
		_stateB = rotateLeft(s1, 37);
		return result;
	}

	private static ulong splitMix64(ref ulong x) {
		ulong z = x += 0x9e3779b97f4a7c15;
		z = (z ^ z >> 30) * 0xbf58476d1ce4e5b9;
		z = (z ^ z >> 27) * 0x94d049bb133111eb;
		return z ^ z >> 31;
	}

	private static ulong rotateLeft(ulong x, int k) {
		return x << k | x >> 64 - k;
	}

	private static int leadingZerosCount(ulong x) {
		if (x == 0) {
			return 64;
		}
		int n = 0;
		if ((x & 0xFFFFFFFF00000000) == 0) {
			n += 32;
			x <<= 32;
		}
		if ((x & 0xFFFF000000000000) == 0) {
			n += 16;
			x <<= 16;
		}
		if ((x & 0xFF00000000000000) == 0) {
			n += 8;
			x <<= 8;
		}
		if ((x & 0xF000000000000000) == 0) {
			n += 4;
			x <<= 4;
		}
		if ((x & 0xC000000000000000) == 0) {
			n += 2;
			x <<= 2;
		}
		if ((x & 0x8000000000000000) == 0) {
			n += 1;
		}
		return n;
	}

	public RandomGenerator LongJump() {
		return new RandomGeneratorXoroshiro128(_stateA, _stateB).jump(
			[0xd2a98b26625eee7b, 0xdddf9b1090aa7ac1]);
	}

	private RandomGeneratorXoroshiro128 jump(ulong[] jumpConstants) {
		ulong s0 = 0;
		ulong s1 = 0;
		for (int i = 0; i < jumpConstants.Length; i++) {
			for (int b = 0; b < 64; b++) {
				if ((jumpConstants[i] & 1UL << b) != 0) {
					s0 ^= _stateA;
					s1 ^= _stateB;
				}
				nextULong();
			}
		}
		_stateA = s0;
		_stateB = s1;
		return this;
	}
}
