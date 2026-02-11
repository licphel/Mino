using System.Runtime.InteropServices;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Silk.NET.OpenGL;

namespace Mino.Native.OpenGL.Object;

public unsafe class GLTexture {
	public TextureDesc _desc;
	public GL _gl;
	public uint _handle;
	public GLEnum _iFormat;
	public GLEnum _pixFormat;
	public GLEnum _pixType;
	public GLEnum _target;

	public GLTexture(GL gl, uint handle) {
		_gl = gl;
		_handle = handle;
	}

	public void OnTextureData(in TextureDesc desc) {
		// Set userdata.
		_desc = desc;

		int width = desc.Width;
		int height = desc.Height;
		int depth = desc.Depth;
		byte[]? data = desc.Data;

		_target = GLEnumC.Cast(desc.Type);
		(_iFormat, _pixFormat, _pixType) = GLEnumC.Cast(desc.Format);

		_gl.ActiveTexture(TextureUnit.Texture0);
		_gl.BindTexture(_target, _handle);

		// Flip-y for gl usage.
		if (desc.Type == TextureType.Texture2D) {
			// Buf fixed: Somehow the texture is flipped,
			// I don't know which part has done it,
			// But it does. Maybe silk.NET?

			// data = Image.FlipRgbaImage2D(data, width, height);
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
}
