using Mino.Modular.Resource;

namespace Mino.Modular.Eventing.Events;

/// <summary>
///		Event: on loading.
/// </summary>
public sealed class LoadEvent : Event {
	public readonly AssetLoader DominantLoader;
	
	public LoadEvent(AssetLoader dominantLoader) {
		DominantLoader = dominantLoader;
	}
}
