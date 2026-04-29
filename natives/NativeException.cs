namespace Mino;

public class NativeException : Exception {
	public NativeException(string message) : base(message) {
	}

	public NativeException(string message, Exception? innerException) : base(message, innerException) {
	}
}
