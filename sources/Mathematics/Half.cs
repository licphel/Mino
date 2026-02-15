#region
using System.Runtime.CompilerServices;
#endregion

namespace Mino.Mathematics;

/// <summary>
///     Fast float-half converter.
/// </summary>
public static class Half {
	/// <summary>
	///     Converts float value to half (f16) value.
	/// </summary>
	/// <param name="f">The conversion target.</param>
	/// <returns>A half float in an ushort.</returns>
	public static ushort Cast(float f) {
		uint floatBits = Unsafe.As<float, uint>(ref f);
		uint sign = (floatBits & 0x80000000U) >> 16;
		uint exponent = (floatBits & 0x7F800000U) >> 23;
		uint mantissa = floatBits & 0x007FFFFFU;

		// Considering that f is not expected to be NaN or infinity,
		// we do not handle them, which makes it faster.
		/*
		if (exponent == 0xFF) {
			return (ushort)(sign | 0x7C00 | (uint)(mantissa != 0 ? 1 : 0));
		}
		if (exponent == 0) {
			return (ushort)(sign | mantissa >> 13);
		}
		*/

		int e = (int) exponent - 127 + 15;
		e = Math.Clamp(e, 0, 31);
		return (ushort) (sign | (uint) e << 10 | mantissa >> 13);
	}
}
