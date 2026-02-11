using System.Collections.Concurrent;
using System.Reflection;

namespace Mino.Framework;

/// <summary>
///     Marks a class is an implementation of an interface.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class Backend : Attribute {
	public const string WINDOWS = "Windows";
	public const string MACOS = "MacOS";
	public const string LINUX = "Linux";
	public const string ANDROID = "Android";
	public const string IOS = "iOS";
	/// <summary>
	///     Windows, Linux, MacOS.
	/// </summary>
	public const string DESKTOP = WINDOWS + LINUX + MACOS;
	/// <summary>
	///     Windows, Linux, Macos, Android, IOS.
	/// </summary>
	public const string ANY = DESKTOP + ANDROID + IOS;

	private static readonly ConcurrentDictionary<Type, object> _implCache =
		new ConcurrentDictionary<Type, object>();
	private static string currentOs;

	static Backend() {
		if (OperatingSystem.IsWindows()) {
			currentOs = "windows";
		} else if (OperatingSystem.IsLinux()) {
			currentOs = "linux";
		} else if (OperatingSystem.IsMacOS()) {
			currentOs = "macos";
		} else if (OperatingSystem.IsAndroid()) {
			currentOs = "android";
		} else if (OperatingSystem.IsIOS()) {
			currentOs = "ios";
		} else {
			throw new Error("unknown operating system");
		}
	}

	public Backend(string name, string? requirement = null) {
		Name = name;
		Requirement = requirement ?? ANY;
	}

	public string Name { get; }
	public string Requirement { get; }

	/// <summary>
	///     Finds an implementation of local platform with the preference.
	/// </summary>
	/// <param name="pref">Preferred implementation name.</param>
	/// <typeparam name="T">Interface type.</typeparam>
	/// <returns>An implementation of the given interface.</returns>
	/// <exception cref="Error">Thrown if there is no workable implementation.</exception>
	public static T Find<T>(string? pref = null) where T : class {
		Type type = typeof(T);
		if (_implCache.TryGetValue(type, out object? cached)) {
			return (cached as T)!;
		}

		// Find in all assemblies.
		Assembly?[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		T? found = null;
		foreach (Assembly? assembly in assemblies) {
			if (assembly == null) {
				continue;
			}
			Type[] founds = assembly.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && type.IsAssignableFrom(t)).ToArray();
			var osReqMet = new List<Type>();
			foreach (Type t in founds) {
				Backend? attr = t.GetCustomAttribute<Backend>();
				if (attr == null) {
					continue;
				}
				bool osReq = attr.Requirement.Contains(currentOs);
				if (osReq) {
					osReqMet.Add(t);
					// Firstly, if we can find preferred backend, do it.
					if (attr.Name == pref) {
						found = Activator.CreateInstance(t) as T;
						break;
					}
				}
			}
			// Secondly, try types that meet os-requirement.
			if (osReqMet.Count > 0) {
				found ??= Activator.CreateInstance(osReqMet[0]) as T;
			}
			// Finally, randomly find one as fallback.
			if (founds.Length > 0) {
				found ??= Activator.CreateInstance(founds[0]) as T;
			}
		}
		_implCache[type] =
			found ?? throw new Error($"{type} has no backend impl");
		return found;
	}
}
