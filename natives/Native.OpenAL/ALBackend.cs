#region
using Mino.Audio.Hardware;
using Mino.Audio.Hardware.Desc;
using Mino.Audio.Hardware.Enum;
using Mino.Framework;
using Mino.Framework.BSP;
using Mino.Native.OpenAL.Object;
using Silk.NET.OpenAL;
#endregion

namespace Mino.Native.OpenAL;

public unsafe class ALBackend : AudioBackend, ServiceProvider {
	private AL _al = AL.GetApi();
	private ALContext _alc = ALContext.GetApi();
	private Context* _context;
	private Device* _device;
	private bool _disposed;
	private bool _ext_EFX;
	private bool _ext_StereoPanning;
	private bool _init;

	public void Init() {
		if (_init) {
			return;
		}
		_init = true;
		_device = _alc.OpenDevice(null);
		if (_device == null) {
			throw new Error("al open device failed");
		}
		_context = _alc.CreateContext(_device, null);
		if (_context == null) {
			throw new Error("al create context failed");
		}
		_alc.MakeContextCurrent(_context);

		// Set initial args for clip pan.
		_al.SetListenerProperty(ListenerVector3.Position, 0.0F, 0.0F, 0.0F);
		_al.SetListenerProperty(ListenerVector3.Velocity, 0.0F, 0.0F, 0.0F);
		float[] orientation = [0.0F, 0.0F, -1.0F, 0.0F, 1.0F, 0.0F];
		fixed (float* oritPtr = orientation) {
			_al.SetListenerProperty(ListenerFloatArray.Orientation, oritPtr);
		}

		// Check extensions.
		_ext_EFX = _al.IsExtensionPresent("ALC_EXT_EFX");
		_ext_StereoPanning =
			_al.IsExtensionPresent("AL_EXT_STEREO_ANGLES") || _alc.IsExtensionPresent("AL_EXT_PANNING");
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		if (_context != null) {
			_alc.DestroyContext(_context);
		}
		if (_device != null) {
			_alc.CloseDevice(_device);
		}
		GC.SuppressFinalize(this);
	}

	public void PollEvents() {
		AudioError err;
		while ((err = _al.GetError()) != AudioError.NoError) {
			throw new Error($"al error raised '{err}'");
		}
	}

	public uint ClipGen() {
		return _clipHeap.Allocate(new ALClip(_al, _al.GenSource()));
	}

	public void ClipDelete(uint clip) {
		uint handle = _clipHeap.GetData(clip)._handle;

		_al.DeleteSource(handle);
		_clipHeap.Free(clip);
	}

	public void ClipPlay(uint clip) {
		uint handle = _clipHeap.GetData(clip)._handle;

		// Set source relative mode
		// for pan simulation.
		_al.SetSourceProperty(handle, SourceBoolean.SourceRelative, true);
		_al.SourcePlay(handle);
	}

	public void ClipStop(uint clip) {
		uint handle = _clipHeap.GetData(clip)._handle;

		// For behavioral correctness,
		// we do not use SourceStop, since it rewinds the clip.
		_al.SourcePause(handle);
	}

	public void ClipData(uint clip, ClipDesc desc) {
		// Get sparse array object.
		ref ALClip _c = ref _clipHeap.GetData(clip);
		uint handle = _c._handle;
		// Set userdata.
		_c._desc = desc;

		_al.SetSourceProperty(handle, SourceInteger.Buffer, desc.Line._handle);
	}

	public void ClipSetProperty<T>(uint clip, ClipProperty property, T value) {
		// Get sparse array object.
		ref ALClip _c = ref _clipHeap.GetData(clip);
		uint handle = _c._handle;

		// We integrate all properties in a single method.
		if (property == ClipProperty.Gain) {
			_al.SetSourceProperty(handle, SourceFloat.Gain, Util.As<float, T>(value));
		} else if (property == ClipProperty.Pitch) {
			_al.SetSourceProperty(handle, SourceFloat.Pitch, Util.As<float, T>(value));
		} else if (property == ClipProperty.Pan) {
			float pan = Util.As<float, T>(value);
			// Try 'Stereo Panning' extension.
			if (_ext_StereoPanning) {
				_al.SetSourceProperty(handle, (SourceFloat) 0x1005, pan);
			} else {
				// Simulate by position.
				// This won't work with stereo sounds.
				float panZ = -MathF.Sqrt(1.0F - pan * pan);
				_al.SetSourceProperty(handle, SourceVector3.Position, pan, 0.0F, panZ);
			}
		} else if (property == ClipProperty.FramePosition) {
			int frames = Util.As<int, T>(value) * _c._desc.Line.Desc.FrameBytes;
			_al.SetSourceProperty(handle, SourceInteger.ByteOffset, frames);
		} else if (property == ClipProperty.Looping) {
			_al.SetSourceProperty(handle, SourceBoolean.Looping, Util.As<bool, T>(value));
		} else {
			throw new Error("invalid arg: " + nameof(property));
		}
	}

	public void ClipGetProperty<T>(uint clip, ClipProperty property, out T value) {
		uint handle = _clipHeap.GetData(clip)._handle;

		// We integrate all properties in a single method.
		if (property == ClipProperty.FramePosition) {
			_al.GetSourceProperty(handle, GetSourceInteger.ByteOffset, out int bytesOff);
			value = Util.As<T, float>(bytesOff);
		} else if (property == ClipProperty.Playback) {
			_al.GetSourceProperty(handle, GetSourceInteger.SourceState, out int st);
			int converted = (int) ALEnumC.Cast((SourceState) st);
			value = Util.As<T, int>(converted);
		} else {
			throw new Error("invalid arg: " + nameof(property));
		}
	}

	public uint LineGen() {
		return _lineHeap.Allocate(new ALLine(_al, _al.GenBuffer()));
	}

	public void LineDelete(uint line) {
		uint handle = _lineHeap.GetData(line)._handle;

		_al.DeleteBuffer(handle);
		_lineHeap.Free(line);
	}

	public void LineData(uint line, LineDesc desc) {
		// Get sparse array object.
		ref ALLine _l = ref _lineHeap.GetData(line);
		uint handle = _l._handle;
		// Set userdata.
		_l._desc = desc;

		int dataSize = desc.Data?.Length ?? 0;
		// Format support check is inside the cast.
		BufferFormat format = ALEnumC.Cast(desc.Format);

		fixed (byte* dataPtr = desc.Data) {
			_al.BufferData(handle, format, dataPtr, dataSize, desc.SampleRate);
		}
	}

	// Finalizer in case.
	~ALBackend() {
		Dispose();
	}

	private Heap<ALClip> _clipHeap = new Heap<ALClip>();
	private Heap<ALLine> _lineHeap = new Heap<ALLine>();
}
