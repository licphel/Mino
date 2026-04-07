using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mino.Modular.Eventing;
using Mino.Modular.Eventing.Events;
using Mino.Modular.Persistent;
using Mino.Modular.Resource;
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
public class Mod {
	/// <summary>
	///		All loaded mods.
	/// </summary>
	public static readonly ConcurrentDictionary<string, Mod> Mods = new ConcurrentDictionary<string, Mod>();
	public static readonly ConcurrentDictionary<Assembly, Mod> ModsByAsm = new ConcurrentDictionary<Assembly, Mod>();
	
	/// <summary>
	///		Bottom core mod, normally the game itself.
	/// </summary>
	public static Mod BottomCore { get; private set; } = null!;
	
	/// <summary>
	///		Dominant loader of all mods.
	/// </summary>
	public static AssetLoader? DominantLoader { get; private set; }

	private static Lock _lock = new Lock();
	private static bool _frozen;
	
	// Mod asm. Used to subscribe events.
	public Assembly? Asm = null;
	public PersistentSystem PersistentSystem = new PersistentSystem();
	public Domain Domain = Domain.Unknown;
	
	private void injectValues(in Url directory, in ModInfo info, Assembly? asm) {
		Directory = directory;
		Info = info;
		Asm = asm;
		Domain = new Domain(Info.ModId);
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
	
	/*
	 * Mod loading stages:
	 * 
	 * ASM_LOAD
	 *	|
	 * PRE_LOAD
	 *	|
	 * SORT_BY_DEPENDENCY
	 *	|
	 * QUEUE_ASSETS
	 *	|
	 * POST_LOAD
	 *	|
	 * LIFECYCLE
	 */

	/// <summary>
	///		On mod pre-load.
	/// </summary>
	public virtual void OnPreLoad() {
		// Override...
	}

	///  <summary>
	/// 		Enqueues loading assets or overriding other mod's assets.
	///  </summary>
	///  <param name="loader">Dominant loader.</param>
	///  <param name="rec">Record of all overriders.</param>
	public void QueueAssetLoading(AssetLoader loader, OverrideRecord rec) {
		OnQueueAssetLoading(DominantLoader = loader, rec);
	}

	///  <summary>
	/// 		Enqueues loading assets or overriding other mod's assets.
	///  </summary>
	///  <param name="loader">Dominant loader.</param>
	///  <param name="rec">Record of all overriders.</param>
	protected virtual void OnQueueAssetLoading(AssetLoader loader, OverrideRecord rec) {
		// Override default behavior
		Url baseOverride = Directory / "override";
		
		foreach (Url url in FileUtil.SubDirectories(baseOverride)) {
			string modId = FileUtil.GetNameNoExtension(url);

			AssetLoader subLoader = loader.CopyWithProcessors(Domain.TryFind(modId));
			subLoader.IsOverriding = true;
			subLoader.Scan(url);
			loader.Enqueue(subLoader);
			
			rec.Record(ModId, modId);
		}
	}

	/// <summary>
	///		On mod post-load.
	/// </summary>
	public virtual void OnPostLoading() {
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
				if (di.MinVersion != null && mod.Version < di.MinVersion) {
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
			
			if (Mods.ContainsKey(modId)) {
				throw new Crash($"Mod id conflict: {modId}");
			}

			if (info.HasProgram) {
				// Has program mod.
				string entry = info.Entrypoint;
				Url programUrl = root / info.ProgramLocation;
				Assembly asm = Assembly.LoadFrom(programUrl.ToFilePath());

				Type? type = asm.GetType(entry);
			
				// Init mod instance.
				if (type != null && type.IsAssignableTo(typeof(Mod))) {
					Mod mod = (Mod) Activator.CreateInstance(asm.GetType(entry)!)!;
					mod.injectValues(root, info, asm);
					Mods[modId] = mod;
					ModsByAsm[asm] = mod;
					mod.OnPreLoad();
					
					Log.Info($"Mod '{modId}' successfully loaded. All subscribed");
					
					EventBus.Instance.ScanSubscribers(asm);

					// Set the first core mod as bottom core.
					if (mod.IsCoreMod) {
						if (BottomCore != null) {
							throw new Crash($"Already has a root mod. old={BottomCore.ModId}, new is from {root}");
						}
						BottomCore = mod;
						Log.Info($"Mod '{modId}' works as bottom core");
					}
				} else {
					throw new Crash($"Entrypoint '{entry}' not found when loading mod '{modId}'");
				}
			} else {
				// No program mod.
				Mod mod = new Mod();
				mod.injectValues(root, info, null);
				Mods[modId] = mod;
				mod.OnPreLoad();
				
				Log.Info($"Mod '{modId}' successfully loaded. Default mod instance created");
			}
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
	///		Loads a class only if given mod is present.
	/// </summary>
	/// <param name="dep">Required dependency.</param>
	/// <typeparam name="T">Type to load.</typeparam>
	/// <returns>True if the class is loaded.</returns>
	public bool LoadClassIfPresent<T>(in DependencyInfo dep) {
		if (Mods.TryGetValue(dep.ModId, out Mod? mod)) {
			if ((dep.MinVersion == null || mod.Version >= dep.MinVersion) 
			&& (dep.MaxVersion == null || mod.Version <= dep.MaxVersion)) {
				RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
				Log.Debug($"Integration loaded: '{ModId}' offered integration with '{dep.ModId}'");
				return true;
			}
		}
		Log.Debug($"Integration failed to load: '{ModId}' offered integration with '{dep.ModId}'");
		return false;
	}

	/// <summary>
	///		Freezes mod loading.
	/// </summary>
	/// <returns>Sorted mods.</returns>
	public static List<Mod> Freeze() {
		lock (_lock) {
			_frozen = true;

			foreach (Mod mod in Mods.Values) {
				mod.CheckDependencies();
			}

			List<Mod> sortedMods = topologicalSort(Mods.Values.ToArray());
			foreach (Mod mod in sortedMods) {
				Log.Debug($"Lazy initializing mod '{mod.ModId}'...");
			}

			return sortedMods;
		}
	}
	
	/// <summary>
	///		Converts an identifier to a mod-based url resource location.
	/// </summary>
	/// <param name="id">Identifier of a mod resource.</param>
	/// <returns>A url of domain - mod id, key - resource finder.</returns>
	/// <exception cref="Crash">Thrown if there's no matching mod.</exception>
	public static Url GetResourceLocation(in Identifier id) {
		Mod? mod = Mods!.GetValueOrDefault(id.Domain.Name, null);
		if (mod == null) {
			throw new Crash($"Domain '{id.Domain}' is not a mod");
		}
		
		return mod.Directory / id.Path;
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
