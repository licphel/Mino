#region
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Mino.Framework;
using Silk.NET.OpenGL;
#endregion

namespace Mino.Native.OpenGL;

public static unsafe class GLDbg {
	public static void Enable(GL gl) {
		gl.Enable(EnableCap.DebugOutput);
		gl.Enable(EnableCap.DebugOutputSynchronous);
		gl.DebugMessageCallback(callback, in IntPtr.Zero);

		gl.DebugMessageControl(
			DebugSource.DontCare,
			DebugType.DebugTypeError,
			DebugSeverity.DontCare,
			0,
			null,
			true
		);
	}

	private static void callback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr message,
		IntPtr param) {
		string msg = Marshal.PtrToStringAnsi(message, length);

		StackTrace stackTrace = new StackTrace(true);
		StackFrame[] frames = stackTrace.GetFrames();

		foreach (StackFrame frame in frames.Skip(3)) {
			MethodBase? method = frame.GetMethod();
			string? fileName = frame.GetFileName();
			int lineNumber = frame.GetFileLineNumber();

			if (!string.IsNullOrEmpty(fileName) && lineNumber > 0) {
				Logger.Global.Fatal("[GL DBG output]");
				Logger.Global.Fatal($"  source: {source}");
				Logger.Global.Fatal($"  type: {type}");
				Logger.Global.Fatal($"  id: {id}");
				Logger.Global.Fatal($"  severity: {severity}");
				Logger.Global.Fatal($"  msg: {msg}");
				Logger.Global.Fatal($"  location: {method?.DeclaringType?.Name}.{method?.Name}");
				Logger.Global.Fatal($"  file: {fileName}:{lineNumber}");
				Logger.Global.Fatal("\n");
				break;
			}
		}

		throw new Error($"error raised '{msg}'");
	}
}
