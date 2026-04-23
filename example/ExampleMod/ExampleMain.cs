using Mino.Audio;
using Mino.Desktop;
using Mino.Framework;
using Mino.Graphics;
using Mino.Mathematics;
using Mino.Modular;
using Mino.Modular.Resource;
using Mino.Native.GLFW;
using Mino.Native.OpenAL;
using Mino.Native.OpenGL;
using Mino.Utility.Logging;

namespace Mino;

public sealed class ExampleMain : Mod {
	public static Executor Executor = null!;
	private static ManualResetEvent initCall = new ManualResetEvent(false);
	
	public override void OnPreLoad() {
		base.OnPreLoad();
		
		Thread thread = new Thread(_Init) {
			Name = "Exec"
		};
		thread.Start();
		// Blocks the mod loading thread,
		// since the asset loading requires that the game starts.
		initCall.WaitOne();
	}

	protected override void OnQueueAssetLoading(AssetLoader loader, OverrideRecord rec) {
		base.OnQueueAssetLoading(loader, rec);
		
		AssetLoader child = loader.CopyWithProcessors(Domain);
		child.Scan(Directory);
		loader.Enqueue(child);
	}

	public override void OnPostLoading() {
		base.OnPostLoading();

		AssetLoader? loader = DominantLoader;
		if (loader != null) {
			while (!loader.Done) {
				loader.Next();
			}
		}
	}

	private static void _Init() {
		Window window = new GLFWWindow();
		// Init window from config.json
		window.Init(new WindowHints {
			Size = new Vector2(800, 600),
			CursorHotspot = default,
			CursorImage = null,
			Icon = null,
			Title = "Test Window",
			DebugContext = false,
			AutoIconify = false,
			Decorated = true,
			Floating = false,
			FocusOnShow = true,
			Maximized = false,
			Resizable = true,
			Visible = true,
			Vsync = true
		});
		
		RenderSystem.LoadContext(window, new GLContext());
		AudioSystem.LoadContext(new ALContext());
		
		Executor = new ExecutorSync();
		
		Executor.OnDispose += delegate {
			// Dispose assets.
			Assets.Foreach((key, value) => {
				if (value.Obj is IDisposable disposable) {
					disposable.Dispose();
					Log.Debug($"Disposed: {key}");
				}
			});
			
			// Ensure logger writes safely.
			Log.Instance.Flush();
		};
		
		// Allow the mod loading thread to continue.
		initCall.Set();
		Executor.Start(window, 60, 120);
	}
}
