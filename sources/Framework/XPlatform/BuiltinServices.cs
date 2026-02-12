using Mino.Native.GLFW;
using Mino.Native.OpenAL;
using Mino.Native.OpenGL;

namespace Mino.Framework.XPlatform;

// Builtin service loader.
internal class BuiltinServices {
	// Create a local object to init.
	public BuiltinServices() {
		load(new ALBackend(), "OpenAL", Platform.DESKTOP, 0);
		load(new GLFWWindow(), "GLFW", Platform.DESKTOP, 0);
		load(new GLBackend(), "OpenGL", Platform.DESKTOP, 0);
	}
	
	private static void load<T>(T obj, string name, uint os, int priority) where T : ServiceProvider {
		try {
			Service._load(obj, name, os, priority);
		} catch (Exception ex) {
			// Do not interrupt.
			Logger.Global.Warn(ex);
		}
	}
}
