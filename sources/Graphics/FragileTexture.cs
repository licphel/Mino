namespace Mino.Graphics;

/// <summary>
///     A fragile texture ref.
/// </summary>
public interface FragileTexture {
	/// <summary>
	///     Pins to get the texture object.
	/// </summary>
	/// <returns>The source texture.</returns>
	Texture Pin();

	/// <summary>
	///     Size on x-axis.
	/// </summary>
	int Width {
		get => Pin().Width;
	}

	/// <summary>
	///     Size on y-axis.
	/// </summary>
	int Height {
		get => Pin().Height;
	}

	/// <summary>
	///     Size on z-axis.
	/// </summary>
	int Depth {
		get => Pin().Depth;
	}
}
