using Mino.Framework;
using Mino.Graphics.RHI;
using Mino.Graphics.RHI.Desc;

namespace Mino.Graphics;

/// <summary>
///     Gpu-side texture.
/// </summary>
public class Texture : IDisposable {
	private RenderBackend _backend;
	public readonly HandleRef _handle;
	private bool _disposed;

	public Texture(in TextureDesc desc) {
		// Do not validate the description,
		// since users may create an empty texture.
		Desc = desc;

		_backend = RenderSystem.GetBackend();
		_handle = new HandleRef(_backend.TextureGen());

		// Validation.
		if (desc.Width < 0 || desc.Height < 0 || desc.Depth < 0) {
			throw new Error("invalid size");
		}

		// Set userdata.
		Desc = desc;
		_backend.TextureData(_handle, desc);
	}

	/// <summary>
	///     The texture desc.
	/// </summary>
	public TextureDesc Desc { get; set; }

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
	/// <param name="submission">Texture submission data.</param>
	public void Submit(in TextureSubmission submission) {
		if (_disposed) {
			throw new Error("disposed");
		}
		// Validation.
		if (submission.Region.Width < 0 || submission.Region.Height < 0 || submission.Region.Depth < 0) {
			throw new Error("invalid size");
		}
		
		_backend.TextureSubmit(_handle, submission);
	}

	// Implicit cast to native handle.
	public static implicit operator uint(Texture obj) {
		return obj._handle;
	}
}
