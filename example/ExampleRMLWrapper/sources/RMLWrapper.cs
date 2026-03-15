#region
using Mino.RML;
#endregion

namespace Mino;

public static class RMLWrapper {
	public static void Main(string[] args) {
		/*
		 * Register a SourceMod.
		 * This allows you to develop in your own mod project
		 * and this wrapper will automatically build your project
		 * and regard ExampleMod/mod/ as a valid mod to process.
		 */
		RMLSourceMod.Register(
			new RMLSourceMod(
				// Path to your project directory
				"<Your solution path>/ExampleMod",
				// Project name, equal to the .csproj file name
				"ExampleMod",
				// Mod id
				"example"
			)
		);
		
		// Official RML loading process
		RMLCore.Main(args);
	}
}
