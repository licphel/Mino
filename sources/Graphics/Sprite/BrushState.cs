namespace Mino.Graphics.Sprite;

public struct BrushState {
	internal RenderPipe? _pipe = null;
	internal BrushPrimitive? _primitive = null;
	internal ResourceSet? _set = null;
	internal Texture? _tex = null;
	
	public BrushState() {
	}
}
