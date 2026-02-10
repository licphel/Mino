using Mino.Framework;
using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;
using Mino.Graphics.RHI.Enum;
using Mino.Mathematics;

namespace Mino.Graphics;

/// <summary>
///     Gpu-side texture.
/// </summary>
public class Texture : IDisposable {
	private RenderBackend _backend;
	private bool _disposed;
	private uint _handle;

	public Texture(in TextureDesc desc) {
		// Do not validate the description,
		// since users may create an empty texture.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = _backend.TextureGen();

		Submit(Desc);
	}

	/// <summary>
	///     The texture desc.
	/// </summary>
	public TextureDesc Desc { get; private set; }

	/// <summary>
	///     Size on x-axis.
	/// </summary>
	public int Width {
		get => Desc.Width;
	}

	/// <summary>
	///     Size on y-axis.
	/// </summary>
	public int Height {
		get => Desc.Height;
	}

	/// <summary>
	///     Size on z-axis.
	/// </summary>
	public int Depth {
		get => Desc.Depth;
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;

		_backend.TextureDelete(_handle);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Submits texture data to gpu.
	/// </summary>
	/// <param name="desc">Texture desc.</param>
	public void Submit(in TextureDesc desc) {
		if (_disposed) {
			throw new Error("disposed");
		}
		// Validation.
		if (desc.Width <= 0) {
			throw new Error("invalid size");
		}
		if (desc.Type == TextureType.Texture2D && desc.Height <= 0) {
			throw new Error("invalid size");
		}
		if (desc.Type == TextureType.Texture3D && (desc.Height <= 0 || desc.Depth <= 0)) {
			throw new Error("invalid size");
		}

		// Reset userdata.
		Desc = desc;
		_backend.TextureData(_handle, desc);
	}

	[NotRecommended]
	public uint GetBackendHandle() {
		return _handle;
	}

	internal void blit(Texture canvas, in Box2 dst, in Box2 src, TextureFilter filter) {
		// Call backend blitter.
		_backend.TextureBlit(
			_handle, (int) src.MinX, (int) src.MinY, (int) src.Width, (int) src.Height, canvas._handle, (int) dst.MinX,
			(int) dst.MinY, (int) dst.Width, (int) dst.Height, filter);
	}
}
