namespace Mino.Mathematics.Planar;

/// <summary>
///     A 2d transformation matrix stack.
/// </summary>
public class MatrixStackPlanar {
	private Action? _onDirty;
	public int Len = 1;
	public Matrix3x2[] ModelStack = new Matrix3x2[256];

	public MatrixStackPlanar(Action? onDirty) {
		_onDirty = onDirty;
		ModelStack[0] = Matrix3x2.Identity;
	}

	public ref Matrix3x2 Top {
		get => ref ModelStack[Len - 1];
	}

	public bool IsEmpty {
		get => Len == 1;
	}

	public Matrix3x2 GetCombinedMatrix(in Matrix3x2 pv) {
		return pv * Top;
	}

	public void Clear() {
		_onDirty?.Invoke();
		Len = 1;
		ModelStack[0] = Matrix3x2.Identity;
	}

	public void Push() {
		_onDirty?.Invoke();
		Len++;
		doCheck();
		ModelStack[Len - 1] = ModelStack[Len - 2];
	}

	private void doCheck() {
		if (IsEmpty) {
			throw new Error("cannot modify base matrix");
		}
	}

	public void Pop() {
		_onDirty?.Invoke();
		Len--;
	}

	public void Load(in Matrix3x2 Matrix3x2) {
		doCheck();
		Top = Matrix3x2;
	}

	public void Rotate(float f) {
		doCheck();
		Top = Top.Rotate(f);
	}

	public void Rotate(in Vector2 r, float f) {
		doCheck();
		Top.Translate(r.X, r.Y);
		Top.Rotate(f);
		Top.Translate(-r.X, -r.Y);
	}

	public void Translate(float x, float y) {
		doCheck();
		Top.Translate(x, y);
	}

	public void Scale(float x, float y) {
		doCheck();
		Top.Scale(x, y);
	}

	public void Shear(float x, float y) {
		doCheck();
		Top.Shear(x, y);
	}

	public void Translate(in Vector2 v) {
		Translate(v.X, v.Y);
	}

	public void Scale(in Vector2 v) {
		Scale(v.X, v.Y);
	}

	public void Shear(in Vector2 v) {
		Shear(v.X, v.Y);
	}
}
