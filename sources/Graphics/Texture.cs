#region
using Mino.Framework.Resource;
using Mino.Graphics.Desc;
using Mino.Graphics.Enum;
#endregion

namespace Mino.Graphics;

/// <summary>
///     Gpu-side texture.
/// </summary>
public interface Texture : FragileTexture, ThreadContextHolder, IDisposable {
	/// <summary>
	///     The texture desc.
	/// </summary>
	TextureDesc Desc { get; }

	/// <summary>
	///     Size on x-axis.
	/// </summary>
	new int Width {
		get => Desc.Width;
	}

	/// <summary>
	///     Size on y-axis.
	/// </summary>
	new int Height {
		get => Desc.Height;
	}

	/// <summary>
	///     Size on z-axis.
	/// </summary>
	new int Depth {
		get => Desc.Depth;
	}

	/// <summary>
	///     Submits texture data to gpu.
	/// </summary>
	/// <param name="submission">Texture submission data.</param>
	void Submit(in TextureSubmission submission);

	/// <summary>
	///     Blits the texture.
	/// </summary>
	/// <param name="to">Dst texture.</param>
	/// <param name="x">Dst x.</param>
	/// <param name="y">Dst y.</param>
	/// <param name="w">Dst width.</param>
	/// <param name="h">Dst height.</param>
	/// <param name="srcX">Src x.</param>
	/// <param name="srcY">Src y.</param>
	/// <param name="srcW">Src width.</param>
	/// <param name="srcH">Src height.</param>
	/// <param name="filter">Blit filter.</param>
	void Blit(Texture to, int x, int y, int w, int h, int srcX, int srcY, int srcW, int srcH,
		TextureFilter filter = TextureFilter.Nearest);
}
