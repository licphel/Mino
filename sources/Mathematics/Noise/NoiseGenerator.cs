namespace Mino.Mathematics.Noise;

/// <summary>
///     Represents a 3D random noise generator.
/// </summary>
public interface NoiseGenerator {
	/// <summary>
	///     Generate a 3D noise value.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <param name="z">Z coordinate.</param>
	/// <returns>A noise value in [0.0, 1.0].</returns>
	double Generate(double x, double y, double z);

	/// <summary>
	///     Generate a 2D noise value.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <param name="y">Y coordinate.</param>
	/// <returns>A noise value in [0.0, 1.0].</returns>
	double Generate(double x, double y) {
		return Generate(x, y, 0.0);
	}

	/// <summary>
	///     Generate a 1D noise value.
	/// </summary>
	/// <param name="x">X coordinate.</param>
	/// <returns>A noise value in [0.0, 1.0].</returns>
	double Generate(double x) {
		return Generate(x, 0.0, 0.0);
	}
}
