namespace Mino.Modular;

public class RMLException : Exception {
	public RMLException(string message) : base(message) {
	}

	public RMLException(string message, Exception? innerException) : base(message, innerException) {
	}
}
