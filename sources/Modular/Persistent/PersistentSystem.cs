using System.Collections.Concurrent;
using Mino.Nio;
using Mino.Nio.NBT;
using Mino.Utility;

namespace Mino.Modular.Persistent;

/// <summary>
///		Manages all data values.
/// </summary>
public sealed class PersistentSystem {
	internal TagMap _G = new TagMap();
	internal bool _init;
	internal ConcurrentQueue<Action> _finalW = new ConcurrentQueue<Action>();
	
	/// <summary>
	///		Initializes the data system.
	/// </summary>
	/// <param name="url">Local data storage file.</param>
	/// <exception cref="Crash">Thrown if url is not a file url.</exception>
	public void Init(Url url) {
		if (!url.Scheme.IsFileBased) {
			throw new Crash($"Url {url} is not a file url");
		}
		_init = true;
		
		AppDomain.CurrentDomain.ProcessExit += delegate {
			// Write present datas.
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
