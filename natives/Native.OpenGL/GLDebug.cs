#region
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Mino.Utility.Logging;
using Silk.NET.OpenGL;
#endregion

namespace Mino.Native.OpenGL;

public static unsafe class GLDebug {
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
				Log.Fatal("[OpenGL detailed debugger output]");
				Log.Fatal($"  source: {source}");
				Log.Fatal($"  type: {type}");
				Log.Fatal($"  id: {id}");
				Log.Fatal($"  severity: {severity}");
				Log.Fatal($"  msg: {msg}");
				Log.Fatal($"  location: {method?.DeclaringType?.Name}.{method?.Name}");
				Log.Fatal($"  file: {fileName}:{lineNumber}");
				Log.Fatal("\n");
				break;
			}
		}
	}
}
