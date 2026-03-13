using System.Collections.Concurrent;
using System.Reflection;
using Mino.Modular.Eventing;
using Mino.Modular.Eventing.Events;
using Mino.Nio;
using Mino.Nio.NBT;
using Mino.Utility;
using Mino.Utility.Logging;

namespace Mino.Modular;

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
	///		Global parent mod.
	/// </summary>
	public static Mod Parent { get; private set; } = null!;
	
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
	///		Checks dependencies and returns a optional error message.
	/// </summary>
	/// <returns>Empty means no error. Otherwise the game will stop.</returns>
	public virtual string CheckDependencies() {
		return string.Empty;
	}

	/// <summary>
	///		Enables or disables this mod.
	/// </summary>
	/// <param name="enabled">Enable state.</param>
	public void SetEnabled(bool enabled) {
		if (enabled) {
			Log.Info($"Mod {ModId} is enabled");
		} else {
			Log.Info($"Mod {ModId} is disabled");
		}
		IsEnabled = enabled;
		EventBus.Instance.Post(new ModEvent(ModId, enabled ? "e" : "d"));
	}

	///  <summary>
	/// 		Creates a root mod, i.e. the main application.
	///  </summary>
	///  <param name="modId">Game id.</param>
	///  <param name="root">Game content url.</param>
	///  <returns>A mod instance.</returns>
	public static Mod CreateParent(string modId, in Url root) {
		Log.Info($"Parent mod {modId} created");
		return Parent = Mods[modId] = new Mod(modId, root);
	}

	/// <summary>
	///		Creates a mod from a mod dir.
	/// </summary>
	/// <param name="root">Mod root dir.</param>
	/// <returns>A mod instance.</returns>
	public static Mod? Load(in Url root) {
		try {
			Log.Info($"Possible mod detected: {root}");
			
			// Load bootstrap.json.
			Url bootstrapUrl = root / "bootstrap.json";
			TagMap bootstrap = TagSystem.ParseJson(bootstrapUrl);
			string modId = bootstrap.Get<string>("mod_id");
			string entry = bootstrap.Get<string>("entrypoint");
			Url programUrl = root / bootstrap.Get<string>("program");
			Assembly asm = Assembly.LoadFile(programUrl.ToFilePath());

			Type? type = asm.GetType(entry);
			if (type != null && type.IsAssignableTo(typeof(Mod))) {
				Mod mod = (Mod) Activator.CreateInstance(asm.GetType(entry)!, root, modId)!;

				if (Mods.ContainsKey(modId)) {
					throw new Crash($"Mod id conflict: {modId}");
				}
				Mods[modId] = mod;

				string depErr = mod.CheckDependencies();
				if (!string.IsNullOrEmpty(depErr)) {
					throw new Crash($"Dependency not satisfied: {depErr}");
				}

				Log.Info($"Mod '{modId}' successfully loaded");
				return mod;
			}
			throw new Crash($"Entrypoint '{entry}' not found when loading mod '{modId}'");
		} catch {
			// Ignored
		}
		
		return null;
	}

	public static void LoadDirectory(in Url modDir) {
		foreach (Url file in FileUtil.SubDirectories(modDir)) {
			Load(file);
		}
	}
}
