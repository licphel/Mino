#region
using Mino.Audio.Hardware.Desc;
using Mino.Audio.Hardware.Enum;
#endregion

namespace Mino.Audio.Hardware;

/// <summary>
///     Native audio backend interface, provides low-level audio operations.
/// </summary>
public interface AudioBackend : IDisposable {
	void Init();
	void PollEvents();

	// Clip
	uint ClipGen();
	void ClipDelete(uint clip);
	void ClipPlay(uint clip);
	void ClipStop(uint clip);
	void ClipData(uint clip, ClipDesc desc);
	void ClipSetProperty<T>(uint clip, ClipProperty property, T value);
	void ClipGetProperty<T>(uint clip, ClipProperty property, out T value);

	// Line
	uint LineGen();
	void LineDelete(uint line);
	void LineData(uint line, LineDesc desc);
}
