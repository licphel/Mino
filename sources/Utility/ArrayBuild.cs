namespace Mino.Utility;

/// <summary>
///		Utilities of making arrays.
/// </summary>
public static class ArrayBuild {
	public static T[] OfFilled<T>(int x, Func<T> supplier) {
		T[] arr = new T[x];
		while (x-- > 0) {
			arr[x] = supplier();
		}
		return arr;
	}
	
	public static T[][] OfFilled<T>(int x, int y, Func<T> supplier) {
		T[][] arr = new T[][y];
		while (y-- > 0) {
			arr[y] = OfFilled(x, supplier);
		}
		return arr;
	}  
	
	public static T[][][] OfFilled<T>(int x, int y, int z, Func<T> supplier) {
		T[][][] arr = new T[][][z];
		while (z-- > 0) {
			arr[z] = OfFilled(x, y, supplier);
		}
		return arr;
	}  
}
