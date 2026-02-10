using Mino.Graphics.RHI.Enum;

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     Baked resource layout of a resource set.
/// </summary>
public class ResourceSetLayout {
	public readonly Slot[] Slots;

	private ResourceSetLayout(params Slot[] slots) {
		Slots = slots;
	}

	/// <summary>
	///     Bakes a resource set layout from a series of bindings.
	/// </summary>
	/// <param name="bindings">Used bindings.</param>
	/// <returns>A baked resource set layout.</returns>
	public static ResourceSetLayout Bake(params Slot[] bindings) {
		for (int i = 0; i < bindings.Length; i++) {
			ref Slot slot = ref bindings[i];
			slot.Binding = i;
		}
		return new ResourceSetLayout(bindings.OrderBy(s => s.Binding).ToArray());
	}

	/// <summary>
	///     A binding slot.
	/// </summary>
	public class Slot {
		public int Count;
		public string Name = string.Empty;
		public ShaderType Stages;
		public ResourceType Type;

		public int Binding { get; internal set; } = -1;
	}
}
