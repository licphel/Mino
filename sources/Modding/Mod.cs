using System.Collections.Concurrent;
using System.Reflection;
using Mino.Nio;
using Mino.Nio.NBT;

namespace Mino.Modding;

/*
 * A mod is constructed as:
 * mod/
 * |
 * - {MOD_ID}
 *	|
 *	- {...} // contents
 *  - config.json
 *  - A.dll
 *  - bootstrap.json
 *
 * Where:
 * config.json storages your custom mod configs.
 * bootstrap.json must follow the pattern as:
 * {
 *   "program": "A.dll" // program dll name.
 *   "entrypoint": "Namespace.MainClass.cs" // full name of the :Mod class.
 *   "mod_id": MOD_ID // your mod id, will be soon loaded into Mod object.
 * }
 */
/// <summary>
///		Represents a mod instance.
/// </summary>
public class Mod {
	/// <summary>
	///		All loaded mods.
	/// </summary>
	public static readonly ConcurrentDictionary<string, Mod> Mods = new ConcurrentDictionary<string, Mod>();
	
	/// <summary>
	///		The 'mod/{MOD_ID}' directory.
	/// </summary>
	public readonly Url Directory;
	/// <summary>
	///		Mod id.
	/// </summary>
	public readonly string ModId;
	/// <summary>
	///		Whether the mod is enabled.
	/// </summary>
	public bool IsEnabled { get; set; } = true;
	
	private Mod(string modId, Url directory) {
		ModId = modId;
		Directory = directory;
	}

	/// <summary>
	///		Creates a mod from a mod dir.
	/// </summary>
	/// <param name="root">Mod root dir.</param>
	/// <returns>A mod instance.</returns>
	public static Mod Create(in Url root) {
		// Load bootstrap.json.
		TagMap bootstrap = TagSystem.ParseJson(root / "bootstrap.json");
		string modId = bootstrap.Get<string>("mod_id");
		Url programUrl = root / bootstrap.Get<string>("program");
		string entry = bootstrap.Get<string>("entrypoint");
		Assembly asm = Assembly.LoadFile(programUrl.ToFilePath());

		Type? type = asm.GetType(entry);
		if (type != null && type.IsAssignableTo(typeof(Mod))) {
			Mod mod = (Mod) Activator.CreateInstance(asm.GetType(entry)!, root, modId)!;

			if (Mods.ContainsKey(modId)) {
				throw new Error($"modId conflict: {modId}");
			}
			Mods[modId] = mod;

			string depErr = mod.CheckDependencies();
			if (!string.IsNullOrEmpty(depErr)) {
				throw new Error($"dep error: {depErr}");
			}
			
			return mod;
		}
		throw new Error($"entrypoint '{entry}' not found when loading {modId}");
	}

	/// <summary>
	///		Checks dependencies and returns a optional error message.
	/// </summary>
	/// <returns>Empty means no error. Otherwise the game will stop.</returns>
	public virtual string CheckDependencies() {
		return string.Empty;
	}
}
