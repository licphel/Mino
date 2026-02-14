namespace Mino.Mathematics;

/// <summary>
///		Abstract matrix interface.
/// </summary>
public interface Matrix<T> where T : Matrix<T> {
	static abstract T operator *(in T left, in T right);
}
