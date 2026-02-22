#region
using Mino.Graphics.Enum;
#endregion

namespace Mino.Graphics.Desc;

/// <summary>
///     Describes a gpu-side buffer object.
/// </summary>
public record struct BufferObjectDesc {
	public BufferFrequency Frequency;
	public BufferType Type;
	public BufferUsage Usage;

	public BufferObjectDesc() {
		Type = default;
		Usage = BufferUsage.GpuRead;
		Frequency = BufferFrequency.Static;
	}
}
