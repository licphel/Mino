using System.Text;

namespace Mino.Nio;

/// <summary>
///		Unified text access.
/// </summary>
public ref struct TextAccess {
	private string _text;
	
	private TextAccess(string text) {
		_text = text;
	}

	///  <summary>
	/// 		Writes all text to a url dest.
	///  </summary>
	///  <param name="url">Writing destination.</param>
	///  <param name="mode">Writing mode, 'a' or 'w'.</param>
	public void Write(in Url url, string? mode = null) {
		Stream? stream = url.OpenStream(mode ?? "w");
		if (stream == null || !stream.CanWrite) {
			return;
		}
		
		// Handle different modes.
		if (mode == "a") {
			stream.Position = stream.Length;
		} else {
			stream.SetLength(0);
			stream.Position = 0;
		}

		using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
		writer.Write(_text);
	}

	// TA -> string
	public static implicit operator string(in TextAccess access) {
		return access._text;
	}

	// string -> TA
	public static implicit operator TextAccess(string str) {
		return new TextAccess(str);
	}

	// URL src -> TA
	public static implicit operator TextAccess(in Url url) {
		Stream? stream = url.OpenStream("r");
		if (stream == null || !stream.CanRead) {
			return string.Empty;
		}
		
		if (stream.CanSeek) {
			stream.Position = 0;
		}
		using StreamReader reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
		return reader.ReadToEnd();
	}
}
