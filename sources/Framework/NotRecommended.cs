namespace Mino.Framework;

/// <summary>
///		Marks a field or method that is not recommended for users to refer.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class NotRecommended : Attribute {
}
