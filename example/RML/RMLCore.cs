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
		AssetLoader loader = new AssetLoader("ml");
		loader.AddProcessor(
			url => url.Path.EndsWith(".png"), (id, url) => {
				Image image = Image.Parse(url.Read());
				Texture tex = RenderSystem.Create<Texture>(TextureDesc.CreateByImage(image));
				Assets.Set(id, tex);
			});
		loader.AddProcessor(
			url => url.Path.EndsWith(".ttf") || url.Path.EndsWith(".otf"), (id, url) => {
				Font font = Font.Load(url);
				font.SetResolution(64);
				Assets.Set(id, font); 
			});
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
