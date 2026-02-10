namespace Mino.Mathematics;

/// <summary>
///     Provides float comparisons.
/// </summary>
public class Comparison {
	/// <summary>
	///     Checks if the two floats are approximately equal.
	/// </summary>
	/// <param name="a">Float A.</param>
	/// <param name="b">Float B.</param>
	/// <returns>True if a is approximately equal to b, otherwise, false.</returns>
	public static bool DoEqual(float a, float b) {
		return Math.Abs(a - b) < float.Epsilon;
	}

	/// <summary>
	///     Checks if the two doubles are approximately equal.
	/// </summary>
	/// <param name="a">Double A.</param>
	/// <param name="b">Double B.</param>
	/// <returns>True if a is approximately equal to b, otherwise, false.</returns>
	public static bool DoEqual(double a, double b) {
		return Math.Abs(a - b) < double.Epsilon;
	}
}
