namespace Mino.Framework.Resource;

/// <summary>
///     Marks a constructor is used for resource factory reflex.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class ResourceCreation : Attribute {
}
