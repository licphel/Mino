using System.Collections.Concurrent;
using Mino.Nio;
using Mino.Nio.NBT;

namespace Mino.Framework;

/// <summary>
///		Manages all option values.
/// </summary>
public static class OptionSystem {
	internal static TagMap _G = new TagMap();
	internal static bool _init;
	internal static ConcurrentQueue<Action> _finalW = new ConcurrentQueue<Action>();
	
	/// <summary>
	///		Initializes the option system.
	/// </summary>
	/// <param name="url">Local option storage file.</param>
	/// <exception cref="Error">Thrown if url is not a file url.</exception>
	public static void Init(Url url) {
		if (!url.Scheme.IsFileBased) {
			throw new Error("not a file url");
		}
		_init = true;
		
		AppDomain.CurrentDomain.ProcessExit += delegate {
			// Write present options.
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
