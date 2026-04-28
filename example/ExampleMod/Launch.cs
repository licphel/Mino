using Mino.RML;

namespace Mino;

public class Launch {
	public static void Main(string[] args) {
		RMLSourceMod.Register(
			new RMLSourceMod(
				// Self-contained launcher
				"~", 
				"Mod Name",
				"mod_id"
			)
		);
		RMLCore.Main(args);
	}
}
