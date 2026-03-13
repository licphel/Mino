using System.Collections.Concurrent;
using Mino.Nio;
using Mino.Nio.NBT;

namespace Mino.Utility.Configuration;

/// <summary>
///		Manages all config values.
/// </summary>
public static class ConfigSystem {
	internal static TagMap _G = new TagMap();
	internal static bool _init;
	internal static ConcurrentQueue<Action> _finalW = new ConcurrentQueue<Action>();
	
	/// <summary>
	///		Initializes the config system.
	/// </summary>
	/// <param name="url">Local config storage file.</param>
	/// <exception cref="Crash">Thrown if url is not a file url.</exception>
	public static void Init(Url url) {
		if (!url.Scheme.IsFileBased) {
			throw new Crash($"Url {url} is not a file url");
		}
		_init = true;
		
		AppDomain.CurrentDomain.ProcessExit += delegate {
			// Write present configs.
			while (!_finalW.IsEmpty) {
				if (_finalW.TryDequeue(out Action? act)) {
					act.Invoke();
				}
 			}
			
			TextAccess ta = TagSystem.DumpJson(_G);
			ta.Write(url);
		};
		
		if (FileUtil.GetTypeOf(url) == FileUtil.PathType.NotExist) {
			// No local data.
			return;
		}
		_G = TagSystem.ParseJson(url);
	}
}
