namespace Mino.Framework.BSP;

/// <summary>
///     Cross-platform service manager.
/// </summary>
public static class Service {
	private static readonly Dictionary<Type, BackendEntry> _bestBackends = new Dictionary<Type, BackendEntry>();
	private static readonly Dictionary<Type, List<BackendEntry>> _backends = new Dictionary<Type, List<BackendEntry>>();
	private static readonly Dictionary<string, BackendEntry> _namedBackends = new Dictionary<string, BackendEntry>();
	private static readonly Lock _lock = new Lock();

	static Service() {
		// Load builtin services.
		_ = new BuiltinServices();
	}

	public static void _load<T>(T instance, string name, uint platformFlag, int priority) where T : ServiceProvider {
		lock (_lock) {
			Type type = typeof(T);
			BackendEntry entry = new BackendEntry {
				Instance = instance,
				PlatformFlag = platformFlag,
				Priority = priority
			};

			if (!_backends.TryGetValue(type, out List<BackendEntry>? value)) {
				value = new List<BackendEntry>();
				_backends[type] = value;
			}

			value.Add(entry);
			_namedBackends[name] = entry;

			if ((platformFlag & Platform.Current) != 0) {
				if (!_bestBackends.TryGetValue(type, out BackendEntry? current) || priority < current.Priority) {
					_bestBackends[type] = entry;
				}
			}
		}
	}

	/// <summary>
	///     Gets a best backend object of current platform.
	/// </summary>
	/// <param name="preferredBackend">Preferred backend name, null means auto.</param>
	/// <typeparam name="T">Backend interface type generic.</typeparam>
	/// <returns>A backend interface impl.</returns>
	/// <exception cref="Error"></exception>
	public static T GetBest<T>(string? preferredBackend = null) where T : class {
		Type type = typeof(T);

		lock (_lock) {
			if (!string.IsNullOrEmpty(preferredBackend)) {
				if (_namedBackends.TryGetValue(preferredBackend, out BackendEntry? namedEntry)) {
					if ((namedEntry.PlatformFlag & Platform.Current) != 0) {
						return (T) namedEntry.Instance;
					}
				}
			}

			if (_bestBackends.TryGetValue(type, out BackendEntry? best)) {
				return (T) best.Instance;
			}
		}

		throw new Error("no backend on current platform.");
	}

	/// <summary>
	///     Gets all backends with this type.
	/// </summary>
	/// <typeparam name="T">Backend interface type generic.</typeparam>
	/// <returns>A list containing all possible backends.</returns>
	public static IReadOnlyList<T> GetAll<T>() where T : class {
		Type type = typeof(T);
		var result = new List<T>();

		lock (_lock) {
			if (_backends.TryGetValue(type, out List<BackendEntry>? entries)) {
				foreach (BackendEntry entry in entries) {
					if ((entry.PlatformFlag & Platform.Current) != 0) {
						result.Add((T) entry.Instance);
					}
				}
			}
		}

		return result.AsReadOnly();
	}

	private class BackendEntry {
		public required object Instance { get; init; }
		public required uint PlatformFlag { get; init; }
		public int Priority { get; init; } = 0;
	}
}
