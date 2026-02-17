#region
using System.Runtime.InteropServices;
using Mino.Graphics.Hardware.Desc;
using Mino.Graphics.Hardware.Enum;
using Silk.NET.OpenGL;
#endregion

namespace Mino.Native.OpenGL.Object;

public unsafe class GLTexture {
	public GL _gl;
	public uint _handle;
	public GLEnum _iFormat;
	public GLEnum _pixFormat;
	public GLEnum _pixType;
	public GLEnum _target;
	public TextureDesc _desc;

	public GLTexture(GL gl, uint handle) {
		_gl = gl;
		_handle = handle;
	}

	public void OnTextureData(in TextureDesc desc) {
		// Set userdata.
		_desc = desc;

		// Cache enums.
		_target = GLEnumC.Cast(desc.Type);
		(_iFormat, _pixFormat, _pixType) = GLEnumC.Cast(desc.Format);

		int width = desc.Width;
		int height = desc.Height;
		int depth = desc.Depth;
		byte[]? data = desc.InitialBytes;

		_target = GLEnumC.Cast(desc.Type);
		(_iFormat, _pixFormat, _pixType) = GLEnumC.Cast(desc.Format);

		_gl.ActiveTexture(TextureUnit.Texture0);
		_gl.BindTexture(_target, _handle);

		// Flip-y for gl usage.
		if (desc.Type == TextureType.Texture2D) {
			// Buf fixed: Somehow the texture is flipped,
			// I don't know which part has done it,
			// But it does. Maybe silk.NET?
		}

		byte* dataPtr = null;
		GCHandle? gch = null;
		if (data != null) {
			gch = GCHandle.Alloc(data, GCHandleType.Pinned);
			dataPtr = (byte*) gch.Value.AddrOfPinnedObject();
		}

		switch (desc.Type) {
			case TextureType.Texture1D:
				_gl.TexImage1D(_target, 0, (int) _iFormat, (uint) width, 0, _pixFormat, _pixType, dataPtr);
				break;
			case TextureType.Texture2D:
				_gl.TexImage2D(
					_target, 0, (int) _iFormat, (uint) width, (uint) height, 0, _pixFormat, _pixType, dataPtr);
				break;
			case TextureType.Texture3D:
				_gl.TexImage3D(
					_target, 0, (int) _iFormat, (uint) width, (uint) height, (uint) depth, 0, _pixFormat, _pixType,
					dataPtr);
				break;
			default:
				// TODO: More texture types support.
				gch?.Free();
				throw new Error("no support");
		}

		gch?.Free();

		if (desc.MipLevels > 0) {
			int maxLevel = desc.MipLevels - 1;
			_gl.TexParameterI(_target, TextureParameterName.TextureMaxLevel, in maxLevel);
			_gl.GenerateMipmap(_target);
		}

		_gl.Finish();
		_gl.BindTexture(_target, 0);
	}

	public void OnTextureSubmit(in TextureSubmission submission) {
		int x = (int) submission.Region.MinX;
		int y = (int) submission.Region.MinY;
		int z = (int) submission.Region.MinZ;
		int width = (int) submission.Region.Width;
		int height = (int) submission.Region.Height;
		int depth = (int) submission.Region.Depth;
		byte[]? rawData = submission.Bytes;

		_gl.ActiveTexture(TextureUnit.Texture0);
		_gl.BindTexture(_target, _handle);

		// Flip-y for gl usage.
		if (_desc.Type == TextureType.Texture2D) {
			// Buf fixed: Somehow the texture is flipped,
			// I don't know which part has done it,
			// But it does. Maybe silk.NET?
		}

		byte* dataPtr = null;
		GCHandle? gch = null;
		if (rawData != null) {
			gch = GCHandle.Alloc(rawData, GCHandleType.Pinned);
			dataPtr = (byte*) gch.Value.AddrOfPinnedObject();
		}

		switch (_desc.Type) {
			case TextureType.Texture1D:
				_gl.TexSubImage1D(_target, 0, x, (uint) width, _pixFormat, _pixType, dataPtr);
				break;
			case TextureType.Texture2D:
				_gl.TexSubImage2D(
					_target, 0, x, y, (uint) width, (uint) height, _pixFormat, _pixType, dataPtr);
				break;
			case TextureType.Texture3D:
				_gl.TexSubImage3D(
					_target, 0, x, y, z, (uint) width, (uint) height, (uint) depth, _pixFormat, _pixType,
					dataPtr);
				break;
			default:
				// TODO: More texture types support.
				gch?.Free();
				throw new Error("no support");
		}

		gch?.Free();

		// Regen mipmap.
		if (_desc.MipLevels > 0) {
			int maxLevel = _desc.MipLevels - 1;
			_gl.TexParameterI(_target, TextureParameterName.TextureMaxLevel, in maxLevel);
			_gl.GenerateMipmap(_target);
		}

		_gl.Finish();
		_gl.BindTexture(_target, 0);
	}
}
