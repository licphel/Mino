namespace Mino.Graphics.Sprite;

/// <summary>
///     Drawing alignment.
/// </summary>
public struct Alignment {
	/*
	 * -1: left
	 * 0: central
	 * 1: right
	 */
	public int Horizontal;
	/*
	 * -1: up
	 * 0: central
	 * 1: down
	 */
	public int Vertical;

	public static readonly Alignment LeftUp = new Alignment {
		Horizontal = -1,
		Vertical = -1
	};

	public static readonly Alignment RightUp = new Alignment {
		Horizontal = 1,
		Vertical = -1
	};

	public static readonly Alignment CentralUp = new Alignment {
		Horizontal = 0,
		Vertical = -1
	};

	public static readonly Alignment Central = new Alignment {
		Horizontal = 0,
		Vertical = 0
	};
}
