namespace Mino.Utility;

public class AccessViolatedException : Exception {
	public AccessViolatedException(string message) : base(message) {
	}

	public AccessViolatedException(string message, Exception? innerException) : base(message, innerException) {
	}
}
