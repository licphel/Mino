using System.Collections.Concurrent;
using System.Reflection;
using Mino.Modular.Eventing;
using Mino.Modular.Eventing.Events;
using Mino.Modular.Persistent;
using Mino.Nio;
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
 * }
 */
/// <summary>
///		Represents a mod instance.
/// </summary>
public abstract class Mod {
	/// <summary>
	///		All loaded mods.
	/// </summary>
	public static readonly ConcurrentDictionary<string, Mod> Mods = new ConcurrentDictionary<string, Mod>();
	/// <summary>
	///		Bottom core mod, normally the game itself.
	/// </summary>
	public static Mod BottomCore { get; private set; } = null!;

	private static Lock _lock = new Lock();
	private static bool _frozen;
	
	// Mod asm. Used to subscribe events.
	public Assembly Asm = null!;
	/// <summary>
	///		The persistent system. 'Modly' singleton.
	/// </summary>
	public PersistentSystem PersistentSystem = new PersistentSystem();
	
	private void injectValues(in Url directory, in ModInfo info, Assembly asm) {
		Directory = directory;
		Info = info;
		Asm = asm;
	}
	
	/// <summary>
	///		The 'mod/{MOD_ID}' directory.
	/// </summary>
	public Url Directory { get; private set; }

	/// <summary>
	///		Mod info.
	/// </summary>
	public ModInfo Info { get; private set; } = new ModInfo();
	
	/// <summary>
	///		Whether the mod is enabled.
	/// </summary>
	public bool IsEnabled { get; set; } = true;

	/// <summary>
	///		Whether the mod is a core mod.
	///		If a mod is a core mod, it will ignore vanilla mod.
	/// </summary>
	public bool IsCoreMod {
		get => Info.IsCoreMod;
	}

	/// <summary>
	///		Mod id.
	/// </summary>
	public string ModId {
		get => Info.ModId;
	}
	
	/// <summary>
	///		Mod version.
	/// </summary>
	public Version Version {
		get => Info.Version;
	}
	
	/// <summary>
	///		Mod dependencies.
	/// </summary>
	public DependencyInfo[] Dependencies {
		get => Info.Dependencies;
	}

	/// <summary>
	///		Initializes the mod.
	/// </summary>
	public virtual void Initialize() {
		// Override...
	}

	/// <summary>
	///		Checks dependencies that are not satisfied..
	/// </summary>
	public virtual void CheckDependencies() {
		IEnumerable<DependencyInfo> dep = Dependencies;

		if (!IsCoreMod) {
			dep = dep.Append(new DependencyInfo(BottomCore.ModId, Version));
		}
		
		foreach (DependencyInfo di in dep) {
			if (Mods.TryGetValue(di.ModId, out Mod? mod)) {
				if (mod.Version < di.MinVersion) {
					throw new Crash($"Version too old: '{ModId}' requires {di}");
				}
				if (di.MaxVersion != null && mod.Version > di.MaxVersion) {
					throw new Crash($"Version too new: '{ModId}' requires {di}");
				}
			} else {
				throw new Crash($"Missing dependency: '{ModId}' requires {di}");
			}
		}

		if (IsCoreMod && Dependencies.Length > 0) {
			Log.Warn($"Core mod '{ModId}' shouldn't have dependencies");
		}
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

	/// <summary>
	///		Creates a mod from a mod dir.
	/// </summary>
	/// <param name="root">Mod root dir.</param>
	public static void Load(in Url root) {
		lock (_lock) {
			if (_frozen) {
				throw new Crash("Mod loading is frozen");
			}
		}
		
		try {
			Log.Info($"Possible mod detected: {root}");
			
			// Load mod.json.
			Url infoUrl = root / "mod.json";
			ModInfo info = ModInfo.FromJson(infoUrl);
			
			string modId = info.ModId;
			string entry = info.Entrypoint;
			Url programUrl = root / info.ProgramLocation;
			Assembly asm = Assembly.LoadFrom(programUrl.ToFilePath());

			Type? type = asm.GetType(entry);
			
			// Init mod instance.
			if (type != null && type.IsAssignableTo(typeof(Mod))) {
				Mod mod = (Mod) Activator.CreateInstance(asm.GetType(entry)!)!;
				mod.injectValues(root, info, asm);

				if (Mods.ContainsKey(modId)) {
					throw new Crash($"Mod id conflict: {modId}");
				}
				Mods[modId] = mod;
				EventBus.Instance.ScanSubscribers(asm);
				
				Log.Info($"Mod '{modId}' successfully loaded. All subscribed");

				// Set the first core mod as bottom core.
				if (mod.IsCoreMod) {
					if (BottomCore != null) {
						throw new Crash($"Already has a root mod. old={BottomCore.ModId}, new is from {root}");
					}
					BottomCore = mod;
					Log.Info($"Mod '{modId}' works as bottom core.");
				}
				return;
			}
			
			throw new Crash($"Entrypoint '{entry}' not found when loading mod '{modId}'");
		} catch(Exception ex) {
			Log.Warn($"Failed to load mod from {root}: {ex.Message}");
		}
	}

	/// <summary>
	///		Loads all mods in a directory.
	/// </summary>
	/// <param name="modDir">Directory to scan from.</param>
	public static void LoadDirectory(in Url modDir) {
		foreach (Url file in FileUtil.SubDirectories(modDir)) {
			Load(file);
		}
	}

	/// <summary>
	///		Freezes mod loading.
	/// </summary>
	public static void Freeze() {
		lock (_lock) {
			_frozen = true;

			foreach (Mod mod in Mods.Values) {
				mod.CheckDependencies();
			}

			List<Mod> sortedMods = topologicalSort(Mods.Values.ToArray());
			foreach (Mod mod in sortedMods) {
				Log.Debug($"Lazy initializing mod '{mod.ModId}'...");
				mod.Initialize();
			}
		}
	}

	private static List<Mod> topologicalSort(Mod[] mods) {
		Dictionary<string, HashSet<string>> graph = new Dictionary<string, HashSet<string>>();
		Dictionary<string, int> inDegree = new Dictionary<string, int>();
		Dictionary<string, Mod> modDict = mods.ToDictionary(m => m.ModId);
		
		foreach (Mod mod in mods) {
			graph[mod.ModId] = new HashSet<string>();
			inDegree[mod.ModId] = 0;
		}
		
		foreach (Mod mod in mods) {
			foreach (DependencyInfo dep in mod.Dependencies) {
				if (modDict.ContainsKey(dep.ModId)) {
					if (graph[dep.ModId].Add(mod.ModId)) {
						inDegree[mod.ModId]++;
					}
				}
			}
		}
		
		Queue<string> queue = new Queue<string>(
			inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key)
		);
    
		List<Mod> result = new List<Mod>();
    
		while (queue.Count > 0) {
			string currentId = queue.Dequeue();
			Mod currentMod = modDict[currentId];
			result.Add(currentMod);
			
			foreach (string dependentId in graph[currentId]) {
				inDegree[dependentId]--;
				if (inDegree[dependentId] == 0) {
					queue.Enqueue(dependentId);
				}
			}
		}
    
		// Check circuit dep.
		if (result.Count != mods.Length) {
			IEnumerable<string> remaining = mods.Select(m => m.ModId).Except(result.Select(m => m.ModId));
			throw new Crash($"Circuit dependency detected: {string.Join(", ", remaining)}");
		}
    
		return result;
	}
}
