namespace Mino.Graphics.Sprite;

public struct BrushState : IEquatable<BrushState> {
	internal RenderPipe? _pipe = null;
	internal BrushPrimitive? _primitive = null;
	internal ResourceSet? _set = null;
	internal Texture? _tex = null;
	
	public BrushState() {
	}
	
	public bool Equals(BrushState other) {
		return _pipe == other._pipe && _primitive == other._primitive && _set == other._set && _tex == other._tex;
	}
	
	public override bool Equals(object? obj) {
		return obj is BrushState other && Equals(other);
	}
	
	public override int GetHashCode() {
		return HashCode.Combine(_pipe, _primitive, _set, _tex);
	}
	
	public static bool operator ==(BrushState left, BrushState right) {
		return left.Equals(right);
	}
	
	public static bool operator !=(BrushState left, BrushState right) {
		return !left.Equals(right);
	}
}
