namespace Mino.Mathematics.Random;

/// <summary>
///     Recoverable random generator.
/// </summary>
public interface RandomGenerator {
	private static RandomGenerator? _default;
	private static readonly Lock _lock = new Lock();

	/// <summary>
	///     A default global random generator.
	/// </summary>
	static RandomGenerator Default {
		get {
			if (_default == null) {
				lock (_lock) {
					_default ??= new RandomGeneratorXoroshiro128();
				}
			}
			return _default;
		}
	}

	/// <summary>
	///     The initial state of the random generator.
	/// </summary>
	ulong[] InitialState { get; }

	/// <summary>
	///     The current state of the random generator.
	/// </summary>
	ulong[] State { get; }

	/// <summary>
	///     Generates a bool value.
	/// </summary>
	/// <returns>True for 50%, false for 50%.</returns>
	public bool NextBool() {
		return NextDouble() <= 0.5;
	}

	/// <summary>
	///     Generates a double value.
	/// </summary>
	/// <returns>A double in [0.0, 1.0].</returns>
	double NextDouble();

	/// <summary>
	///     Generates an integer value.
	/// </summary>
	/// <param name="bound">The boundary of the generated value.</param>
	/// <returns>An integer in [0, bound).</returns>
	int NextInt(int bound = int.MaxValue);

	/// <summary>
	///     Generates a double value.
	/// </summary>
	/// <returns>A double in [min, max].</returns>
	public double NextDouble(double min, double max) {
		return NextDouble() * (max - min) + min;
	}

	/// <summary>
	///     Generates an integer value.
	/// </summary>
	/// <returns>An integer in [min, max].</returns>
	public int NextInt(int min, int max) {
		max++;
		return NextInt(max - min) + min;
	}

	/// <summary>
	///     Randomly selects an element in a non-empty list.
	/// </summary>
	/// <param name="list">The target non-empty list.</param>
	/// <returns>A random element in the given list.</returns>
	/// <exception cref="Error">Thrown if the list is empty.</exception>
	public T Select<T>(List<T> list) {
		if (list.Count == 0) {
			throw new Error("empty list");
		}
		return list[NextInt(list.Count)];
	}

	/// <summary>
	///     Randomly selects an element in a non-empty array.
	/// </summary>
	/// <param name="arr">The target non-empty array.</param>
	/// <returns>A random element in the given array.</returns>
	/// <exception cref="Error">Thrown if the array is empty.</exception>
	public T Select<T>(params T[] arr) {
		if (arr.Length == 0) {
			throw new Error("empty array");
		}
		return arr[NextInt(arr.Length)];
	}

	/// <summary>
	///     Generates a double value which follows gaussian distribution,
	/// </summary>
	/// <returns>A double in [0.0, 1.0].</returns>
	public double NextGaussianDouble() {
		double x, y, w;
		do {
			x = NextDouble() * 2 - 1;
			y = NextDouble() * 2 - 1;
			w = x * x + y * y;
		} while (w is >= 1 or 0);
		double c = Math.Sqrt(-2 * Math.Log(w) / w);
		double gaussian = y * c;
		return Math.Tanh(gaussian / 3.0) * 0.5 + 0.5;
	}

	/// <summary>
	///     Generates a double value which follows gaussian distribution,
	/// </summary>
	/// <returns>A double in [min, max].</returns>
	public double NextGaussianDouble(double min, double max) {
		return NextGaussianDouble() * (max - min) + min;
	}

	/// <summary>
	///     Gets a new, different random generator according to x.
	/// </summary>
	/// <param name="x">The drift param.</param>
	/// <returns>The drifted new random generator.</returns>
	RandomGenerator Drift(ulong x);

	/// <summary>
	///     Jumps through a long sequence.
	/// </summary>
	/// <returns>The jumped new random generator.</returns>
	RandomGenerator Jump();

	/// <summary>
	///     Recovers the state from an ulong array.
	/// </summary>
	/// <param name="state">The state obtained by a random generator of the same type.</param>
	void Recover(ulong[] state);

	/// <summary>
	///     Copies a random generator with the original state.
	/// </summary>
	/// <returns>A new random generator.</returns>
	RandomGenerator CopyOriginally();

	/// <summary>
	///     Copies a random generator with the current state.
	/// </summary>
	/// <returns>A new random generator.</returns>
	RandomGenerator CopyCurrently();
}
