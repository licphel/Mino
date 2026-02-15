namespace Mino.Framework;

/// <summary>
///     A packed Update(TimeStep step) arg.
/// </summary>
public readonly struct TimeStep {
	public readonly double Delta;
	public readonly TimeSpan Timestamp;
	
	public float Milliseconds {
		get => (float) Timestamp.TotalMilliseconds;
	}
	
	public float Seconds {
		get => (float) Timestamp.TotalSeconds;
	}
	
	public float Minutes {
		get => (float) Timestamp.TotalMinutes;
	}

	public TimeStep(TimeSpan timestamp, double delta) {
		Delta = delta;
		Timestamp = timestamp;
	}
}
