namespace Mino.Nio.NBT;

/// <summary>
///		A tag interpreter interface.
/// </summary>
public interface TagInterpreter {
	TagMap Read(string source);
}
