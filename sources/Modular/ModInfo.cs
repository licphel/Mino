using Mino.Nio;
using Mino.Nio.NBT;
using Mino.Utility;

namespace Mino.Modular;

/// <summary>
///		Mod info object.
/// </summary>
public class ModInfo {
	public string ModId = string.Empty;
	public Version Version = new Version();
	public DependencyInfo[] Dependencies = [];
	public bool IsCoreMod = false;
	public string Authors = string.Empty;
	public string Description = string.Empty;
	public string DisplayedName = string.Empty;
	public string Entrypoint = string.Empty;
	public string ProgramLocation = string.Empty;

	/// <summary>
	///     Creates ModInfo from a JSON string or URL.
	/// </summary>
	/// <param name="json">JSON string or URL pointing to mod info.</param>
	/// <returns>A new ModInfo instance.</returns>
	/// <exception cref="Crash">Thrown if required fields are missing or invalid.</exception>
	public static ModInfo FromJson(TextAccess json) {
		TagMap map = TagSystem.ParseJson(json);
		
		// Info configs
		string modId = map.Get<string>("$info.mod_id");
		if (string.IsNullOrWhiteSpace(modId)) {
			throw new Crash("ModInfo missing required field: $info.mod_id");
		}

		string versionStr = map.Get<string>("$info.version");
		if (string.IsNullOrWhiteSpace(versionStr)) {
			throw new Crash($"Mod '{modId}' missing required field: $info.version");
		}
		
		if (!Version.TryParse(versionStr, out Version? version)) {
			throw new Crash($"Mod '{modId}' has invalid version format: {versionStr}");
		}
		
		string authors = map.Get<string>("$info.authors", string.Empty);
		string displayedName = map.Get<string>("$info.displayed_name", modId);
		string description = map.Get<string>("$info.description", string.Empty);
		
		// Program configs
		bool isCoreMod = map.Get<bool>("$program.is_core_mod", false);
		
		string programLocation = map.Get<string>("$program.location");
		if (!isCoreMod && string.IsNullOrWhiteSpace(programLocation)) {
			throw new Crash("ModInfo missing required field: $program.location");
		}

		string entrypoint = map.Get<string>("$program.entrypoint");
		if (!isCoreMod && string.IsNullOrWhiteSpace(entrypoint)) {
			throw new Crash($"Mod '{modId}' missing required field: $program.entrypoint");
		}
		
		// Dep configs
		DependencyInfo[] dependencies = parseDepArr(map, modId);

		return new ModInfo {
			ModId = modId,
			Version = version,
			Authors = authors,
			DisplayedName = displayedName,
			Description = description,

			IsCoreMod = isCoreMod,
			ProgramLocation = programLocation,
			Entrypoint = entrypoint,

			Dependencies = dependencies
		};
	}
	
	private static DependencyInfo[] parseDepArr(TagMap map, string modId) {
		List<DependencyInfo> result = new List<DependencyInfo>();

		// Try to get dependencies list
		if (!map.TryGet("dependencies", out TagList depList)) {
			return Array.Empty<DependencyInfo>();
		}

		for (int i = 0; i < depList.Count; i++) {
			try {
				DependencyInfo? dep = parseDep(depList, i, modId);
				if (dep != null) {
					result.Add(dep.Value);
				}
			} catch (Exception ex) {
				throw new Crash($"Mod '{modId}' has invalid dependency at index {i}: {ex.Message}");
			}
		}

		return result.ToArray();
	}
	
	private static DependencyInfo? parseDep(TagList depList, int index, string modId) {
		TagMap depMap = depList.Get<TagMap>(index);
		
		string targetModId = depMap.Get<string>("mod_id");
		if (string.IsNullOrWhiteSpace(targetModId)) {
			throw new Crash($"Dependency at index {index} missing mod_id");
		}

		// Parse version range
		string minVersionStr = depMap.Get<string>("min_version", "0.0.0");
		if (!Version.TryParse(minVersionStr, out Version? minVersion)) {
			throw new Crash($"Dependency {targetModId} has invalid min_version: {minVersionStr}");
		}

		Version? maxVersion = null;
		if (depMap.TryGet("max_version", out string maxVersionStr)) {
			if (!Version.TryParse(maxVersionStr, out maxVersion)) {
				throw new Crash($"Dependency {targetModId} has invalid max_version: {maxVersionStr}");
			}
		}
			
		return new DependencyInfo(targetModId, minVersion, maxVersion);
	}
}
