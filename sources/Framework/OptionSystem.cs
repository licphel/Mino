using Mino.Nio;
using Mino.Nio.NBT;

namespace Mino.Framework;

/// <summary>
///		Manages all option values.
/// </summary>
public static class OptionSystem {
	internal static TagMap _serialization = new TagMap();
	internal static bool _init;
	
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
			ByteBuffer buf = new ByteBuffer().With(Endianness.Little);
			TagSystem.Encode(_serialization, buf);
			url.Write(buf);
		};
		
		if (FileUtil.GetTypeOf(url) == FileUtil.PathType.NotExist) {
			// No local data.
			return;
		}
		_serialization = TagSystem.Decode(url.Read().With(Endianness.Little));
	}
}
