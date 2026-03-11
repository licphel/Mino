namespace Mino.Nio.NBT;

/// <summary>
///		Key sequence.
/// </summary>
public struct Seq {
	public Seq(string str, bool split = false) {
		Semantic = str;
		ShouldSplit = split;
	}

	/// <summary>
	///		The semantic key.
	/// </summary>
	public string Semantic { get; }

	/// <summary>
	///		Whether the key should be reinterpreted as a key sequence.
	/// </summary>
	public bool ShouldSplit { get; }

	// Seq -> string
	public static implicit operator string(in Seq seq) {
		return seq.Semantic;
	}
	
	// string -> Seq
	public static implicit operator Seq(string str) {
		return new Seq(str);
	}
}
