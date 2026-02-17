#region
using Mino.Graphics.Hardware.Enum;
#endregion

namespace Mino.Graphics.Hardware.Desc;

/// <summary>
///     Describes a gpu-side buffer object.
/// </summary>
public record struct BufferDesc {
	public BufferFrequency Frequency;
	public BufferType Type;
	public BufferUsage Usage;

	public BufferDesc() {
		Type = default;
		Usage = BufferUsage.GpuRead;
		Frequency = BufferFrequency.Static;
	}
}
