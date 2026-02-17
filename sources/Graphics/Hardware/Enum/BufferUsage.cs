namespace Mino.Graphics.Hardware.Enum;

/// <summary>
///     Identifies gpu buffer usage hints.
/// </summary>
[Flags]
public enum BufferUsage {
	GpuRead = 1 << 0,
	GpuWrite = 1 << 1,
	CpuRead = 1 << 2,
	CpuWrite = 1 << 3
}
