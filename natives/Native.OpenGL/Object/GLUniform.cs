using System.Runtime.InteropServices;
using Mino.Framework;
using Mino.Mathematics;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public static unsafe class GLUniform {
	public static void OnUniformData<T>(this GL gl, int location, in T data) where T : unmanaged {
		int size = Marshal.SizeOf<T>();
		if (size == sizeof(float) * 1) {
			if (typeof(T) == typeof(float)) {
				float val = Util.As<float, T>(data);
				gl.Uniform1(location, val);
			} else if (typeof(T) == typeof(int)) {
				int val = Util.As<int, T>(data);
				gl.Uniform1(location, val);
			}
		} else if (size == sizeof(float) * 2) {
			if (typeof(T) == typeof(Vector2)) {
				Vector2 vec = Util.As<Vector2, T>(data);
				gl.Uniform2(location, vec.X, vec.Y);
			}
		} else if (size == sizeof(float) * 3) {
			if (typeof(T) == typeof(Vector3)) {
				Vector3 vec = Util.As<Vector3, T>(data);
				gl.Uniform3(location, vec.X, vec.Y, vec.Z);
			}
		} else if (size == sizeof(float) * 4) {
			if (typeof(T) == typeof(Vector4)) {
				Vector4 vec = Util.As<Vector4, T>(data);
				gl.Uniform4(location, vec.X, vec.Y, vec.Z, vec.W);
			}
		} else if (size == sizeof(float) * 6) {
			if (typeof(T) == typeof(Matrix3x2)) {
				Matrix3x2 mat = Util.As<Matrix3x2, T>(data);
				gl.UniformMatrix3x2(location, 1, false, (float*) &mat);
			}
		} else if (size == sizeof(float) * 16) {
			if (typeof(T) == typeof(Matrix4x4)) {
				Matrix4x4 mat = Util.As<Matrix4x4, T>(data);
				gl.UniformMatrix4(location, 1, false, (float*) &mat);
			}
		}
	}

	public static void OnUniformData<T>(this GL gl, int location, ReadOnlySpan<T> data) where T : unmanaged {
		// TODO
	}
}
