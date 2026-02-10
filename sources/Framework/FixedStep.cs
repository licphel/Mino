namespace Mino.Framework;

/// <summary>
///     A packed Update(FixedStep step) arg.
/// </summary>
public readonly ref struct FixedStep {
	public readonly double Delta;
	public readonly TimeSpan Timestamp;

	public FixedStep(Executor executor) {
		Delta = executor.Delta;
		Timestamp = executor.Timestamp;
	}
}
