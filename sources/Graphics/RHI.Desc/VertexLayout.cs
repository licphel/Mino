using Mino.Graphics.RHI.Enum;

namespace Mino.Graphics.RHI.Desc;

/// <summary>
///     Baked vertex layout.
/// </summary>
public class VertexLayout {
	public Attr[] Attrs;
	public int Stride;

	private VertexLayout(Attr[] attrs, int stride) {
		Attrs = attrs;
		Stride = stride;
	}

	/// <summary>
	///     Bakes a vertex layout from a series of consecutive attributes.
	/// </summary>
	/// <param name="attributes">Attributes ordered locationally.</param>
	/// <returns>A backed vertex layout.</returns>
	public static VertexLayout Bake(params Attr[] attributes) {
		int offset = 0;
		for (int i = 0; i < attributes.Length; i++) {
			ref Attr attr = ref attributes[i];
			attr.Offset = offset;
			attr.Location = i;
			attr.Size = getDataTypeInBytes(attr.Type) * attr.Components;
			offset += attr.Size;
		}
		return new VertexLayout(attributes.OrderBy(attr => attr.Location).ToArray(), offset);
	}

	private static int getDataTypeInBytes(VertexAttributeType type) {
		return type switch {
			VertexAttributeType.uint8 => 1,
			VertexAttributeType.uint16 => 2,
			VertexAttributeType.uint32 => 4,
			VertexAttributeType.Int8 => 1,
			VertexAttributeType.Int16 => 2,
			VertexAttributeType.Int32 => 4,
			VertexAttributeType.Float16 => 2,
			VertexAttributeType.Float32 => 4,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	/// <summary>
	///     Vertex attribute info.
	/// </summary>
	public class Attr {
		public int Components = 1;
		public bool Normalized = false;
		public VertexAttributeType Type = default;

		public int Location { get; internal set; } = -1;
		public int Offset { get; internal set; } = -1;
		public int Size { get; internal set; } = -1;
	}
}
