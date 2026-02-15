namespace Mino.Algorithm.Noise;

/// <summary>
///     Generates fluid pattern like noise.
/// </summary>
public class NoiseGeneratorOctaveTurbulence : NoiseGenerator {
	private readonly NoiseGenerator _noise;
	private readonly int _octaves;
	private readonly double _persistence;
	private readonly double _lacunarity;
	private readonly double _amplitudeScale;
	private readonly double _frequencyScale;

	public NoiseGeneratorOctaveTurbulence(
		NoiseGenerator noise,
		int octaves,
		double persistence = 0.5,
		double lacunarity = 2.0,
		double amplitudeScale = 1.0,
		double frequencyScale = 1.0) {

		_noise = noise;
		_octaves = octaves;
		_persistence = persistence;
		_lacunarity = lacunarity;
		_amplitudeScale = amplitudeScale;
		_frequencyScale = frequencyScale;
	}

	public double Generate(double x, double y, double z) {
		double amplitude = 1.0;
		double frequency = 1.0;
		double noiseValue = 0.0;
		double maxAmplitude = 0.0;

		for (int i = 0; i < _octaves; i++) {
			double nx = x * frequency * _frequencyScale;
			double ny = y * frequency * _frequencyScale;
			double nz = z * frequency * _frequencyScale;

			// Absolute value creates turbulence.
			double n = Math.Abs(_noise.Generate(nx, ny, nz) * 2.0 - 1.0);

			noiseValue += n * amplitude;
			maxAmplitude += amplitude;

			amplitude *= _persistence;
			frequency *= _lacunarity;
		}

		return noiseValue / maxAmplitude * _amplitudeScale;
	}
}
