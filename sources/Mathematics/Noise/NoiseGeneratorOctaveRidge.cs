namespace Mino.Mathematics.Noise;

/// <summary>
///     Ridge octave noise - produces sharp, vein-like patterns.
/// </summary>
public class NoiseGeneratorOctaveRidge : NoiseGenerator {
	private readonly NoiseGenerator _noise;
	private readonly int _octaves;
	private readonly double _persistence;
	private readonly double _lacunarity;
	private readonly double _amplitudeScale;
	private readonly double _frequencyScale;
	private readonly double _ridgeOffset;

	public NoiseGeneratorOctaveRidge(
		NoiseGenerator noise,
		int octaves,
		double persistence = 0.5,
		double lacunarity = 2.0,
		double amplitudeScale = 1.0,
		double frequencyScale = 1.0,
		double ridgeOffset = 1.0) {

		_noise = noise;
		_octaves = octaves;
		_persistence = persistence;
		_lacunarity = lacunarity;
		_amplitudeScale = amplitudeScale;
		_frequencyScale = frequencyScale;
		_ridgeOffset = ridgeOffset;
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

			// Get noise and apply ridge transform.
			double n = _noise.Generate(nx, ny, nz);
			// Convert to ridge.
			n = _ridgeOffset - Math.Abs(n * 2.0 - 1.0);

			noiseValue += n * amplitude;
			maxAmplitude += amplitude * _ridgeOffset;

			amplitude *= _persistence;
			frequency *= _lacunarity;
		}

		return noiseValue / maxAmplitude * _amplitudeScale;
	}
}
