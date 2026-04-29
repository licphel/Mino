using System.Collections.Concurrent;
using Mino.Nio;
using Mino.Nio.NBT;

namespace Mino.Modular.Persistent;

/// <summary>
///		Manages all data values.
/// </summary>
public sealed class PersistentSystem {
	internal NBTCompound _G = new NBTCompound();
	internal bool _init;
	internal ConcurrentQueue<Action> _finalW = new ConcurrentQueue<Action>();
	
	/// <summary>
	///		Initializes the data system.
	/// </summary>
	/// <param name="url">Local data storage file.</param>
	/// <exception cref="RMLException">Thrown if url is not a file url.</exception>
	public void Init(Url url) {
		if (!url.Scheme.IsFileBased) {
			throw new RMLException($"Url {url} is not a file url");
		}
		_init = true;
		
		AppDomain.CurrentDomain.ProcessExit += delegate {
			// Write present datas.
			while (!_finalW.IsEmpty) {
				if (_finalW.TryDequeue(out Action? act)) {
					act.Invoke();
				}
 			}
			
			TextAccess ta = NBTSystem.DumpJson(_G);
			ta.Write(url);
		};
		
		if (Furl.Typeof(url) == Furl.PathType.NotExist) {
			// No local data.
			return;
		}
		_G = NBTSystem.ParseJson(url);
	}
}
