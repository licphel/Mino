namespace Mino.Input;

public class SnapshotFailedException : Exception {
	public SnapshotFailedException(string message) : base(message) {
	}

	public SnapshotFailedException(string message, Exception? innerException) : base(message, innerException) {
	}
}
