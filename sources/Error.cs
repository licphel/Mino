using Mino.Framework;

namespace Mino;

/// <summary>
///     An error thrown from the framework.
/// </summary>
public class Error : Exception {
	/// <summary>
	///		Output logger when an error was thrown.
	/// </summary>
	public static Logger LoggerUsed { get; set; } = Logger.Global;

	public Error(string? message) : base(message) {
		LoggerUsed.Fatal(message ?? "unknown error");
	}

	public Error(string? message, Exception? innerException) : base(message, innerException) {
		LoggerUsed.Fatal(innerException ?? this);
	}

	public Error(string arg, string? message) : base($"{arg}: {message}") {
		LoggerUsed.Fatal($"{arg}: {message ?? "unknown error"}");
	}
}
