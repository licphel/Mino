using Mino.Mathematics;

namespace Mino.Scene;

/// <summary>
///     A 2d transformation matrix stack.
/// </summary>
public class MatrixStack<T> where T : Matrix<T>, new() {
	private Action? _onDirty;
	public int Len = 1;
	public T[] ModelStack = new T[256];

	public MatrixStack(Action? onDirty = null) {
		_onDirty = onDirty;
		ModelStack[0] = new T();
	}

	public ref T Top {
		get => ref ModelStack[Len - 1];
	}

	public bool IsEmpty {
		get => Len == 1;
	}

	public void Clear() {
		_onDirty?.Invoke();
		Len = 1;
		ModelStack[0] = new T();
	}

	public void Push() {
		_onDirty?.Invoke();
		Len++;
		assertNonempty();
		ModelStack[Len - 1] = ModelStack[Len - 2];
	}

	public void Push(in T mat) {
		Push();
		Top *= mat;
	}

	public void Pop() {
		_onDirty?.Invoke();
		Len--;
	}

	public void Load(in T mat) {
		assertNonempty();
		Top = mat;
	}
	
	private void assertNonempty() {
		if (IsEmpty) {
			throw new Error("cannot modify base matrix");
		}
	}
}
