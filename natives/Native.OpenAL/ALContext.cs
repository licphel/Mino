using Mino.Audio;
using Mino.Framework.Resource;
using Mino.Native.OpenAL.Object;
using Mino.Utility;
using Mino.Utility.Logging;
using Silk.NET.OpenAL;
using ALC = Silk.NET.OpenAL.ALContext;

namespace Mino.Native.OpenAL;

public unsafe sealed class ALContext : AbstractThreadContext {
	public AL _al = AL.GetApi();
	public ALC _alc = ALC.GetApi();
	public Context* _context;
	public Device* _device;
	/*
	 * Extension info.
	 * 1. EFX
	 * 2. StereoPanning
	 */
	public bool _ext_EFX;
	public bool _ext_StereoPanning;
	/*
	 * Clip tracking.
	 */
	public LinkedList<ALClip> _trackingList = new LinkedList<ALClip>();

	public override void PollEvents() {
		// Send a pending event.
		Pend(() => {
			AudioError err;
			while ((err = _al.GetError()) != AudioError.NoError) {
				Log.Warn($"OpenAL error raised: '{err}'");
			}

			// Tracking playback states.
			foreach (ALClip alc in _trackingList) {
				_al.GetSourceProperty(alc._handle, GetSourceInteger.SourceState, out int v1);
				_al.GetSourceProperty(alc._handle, SourceFloat.SecOffset, out float v2);
				alc._alRawPlayback = v1;
				alc._alSecOffset = v2;
			}
		});
	}

	protected override void OnInit() {
		Factory.RegisterInterface<Clip, ALClip>(injector);
		Factory.RegisterInterface<DataLine, ALDataLine>(injector);
		return;
		
		void injector(ThreadContextHolder h) {
			h.Listen(this);
		}
	}
	
	protected override void OnContextStart() {
		_device = _alc.OpenDevice(null);
		if (_device == null) {
			throw new Crash("OpenAL open device failed");
		}
		_context = _alc.CreateContext(_device, null);
		if (_context == null) {
			throw new Crash("OpenAL create context failed");
		}
		_alc.MakeContextCurrent(_context);
		Log.Info("OpenAL context was successfully initialized");

		/*
		 * Initial listener position & orientation.
		 * This is for panning.
		 */
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
	
	protected override void OnContextStop() {
		// Do nothing.
	}
	
	protected override void OnDispose() {
		if (_context != null) {
			_alc.DestroyContext(_context);
		}
		if (_device != null) {
			_alc.CloseDevice(_device);
		}
		_al.Dispose();
		_alc.Dispose();
	}
}
