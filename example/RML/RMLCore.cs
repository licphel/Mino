using Mino.Audio;
using Mino.Audio.Desc;
using Mino.Framework;
using Mino.Graphics;
using Mino.Graphics.Desc;
using Mino.Graphics.Text;
using Mino.Modular;
using Mino.Modular.Resource;
using Mino.Nio;
using Mino.Utility.Logging;

namespace Mino.RML;

public static class RMLCore {
	public static Action<AssetLoader> OnSetProcessors = loader => {
		// Textures
		loader.AddProcessor(
			url => FileUtil.GetExtension(url).ToLowerInvariant() is "png" or "jpg" or "jpeg", (id, url) => {
				Image image = Image.Parse(url.Read());
				Texture tex = RenderSystem.Create<Texture>(TextureDesc.CreateByImage(image));
				Assets.Set(id, tex);
			});
		
		// Font types
		loader.AddProcessor(
			url => FileUtil.GetExtension(url).ToLowerInvariant() is "ttf" or "otf", (id, url) => {
				Font font = Font.Load(url);
				font.SetResolution(64);
				Assets.Set(id, font); 
			});
		
		// Texts
		loader.AddProcessor(
			url => FileUtil.GetExtension(url).ToLowerInvariant() is "txt",
			(id, url) => {
				TextAccess str = url;
				Assets.Set(id, (string) str);
			}
		);
		
		// Wave data lines.
		loader.AddProcessor(
			url => FileUtil.GetExtension(url).ToLowerInvariant() is "wav",
			(id, url) => {
				DataLine dataLine = AudioSystem.Create<DataLine>(DataLineDesc.Parse(url.Read()));
				Assets.Set(id, dataLine);
			}
		);
	};
	
	public static void Main(string[] args) {
		// Firstly, we handle args.
		FrameworkSetup.Start(args);

		// Build source mods.
		foreach (RMLSourceMod sm in RMLSourceMod._smRegistry) {
			sm.Build();
		}
		
		// Add log output dest.
		Url logUrl = Url.Local("log/latest.log");
		FileUtil.CreateFile(logUrl);
		Log.Instance.OutputTo(logUrl);
		
		// Create dominant loader.
		AssetLoader loader = new AssetLoader(new Domain("ml"));
		OnSetProcessors?.Invoke(loader);
		// ...
		// Other processors can be added, up to you.
	
		// Load mods.
		Url modUrl = Url.Local("mod");
		FileUtil.CreateDirectory(modUrl);
		Mod.LoadDirectory(modUrl);
		
		// Load source mods.
		foreach (RMLSourceMod sm in RMLSourceMod._smRegistry) {
			Mod.Load(new Url(sm.ProjectPath) / "mod");
		}
		
		List<Mod> mods = Mod.Freeze();

		// Mod init.
		OverrideRecord rec = new OverrideRecord();
		foreach (Mod mod in mods) {
			mod.QueueAssetLoading(loader, rec);
		}
		foreach (Mod mod in mods) {
			mod.OnPostLoading();
		}
	}
}
