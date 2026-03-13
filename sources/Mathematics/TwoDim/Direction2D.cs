namespace Mino.Mathematics.TwoDim;

/// <summary>
///		2D directions.
/// </summary>
public sealed class Direction2D {
	public static readonly Direction2D North = new Direction2D(new PosI(0, -1), 90, 0);
	public static readonly Direction2D NorthEast = new Direction2D(new PosI(1, -1), 45, 1);
	public static readonly Direction2D East = new Direction2D(new PosI(1, 0), 0, 2);
	public static readonly Direction2D SouthEast = new Direction2D(new PosI(1, 1), -45, 3);
	public static readonly Direction2D South = new Direction2D(new PosI(0, 1), -90, 4);
	public static readonly Direction2D SouthWest = new Direction2D(new PosI(-1, 1), -135, 5);
	public static readonly Direction2D West = new Direction2D(new PosI(-1, 0), 180, 6);
	public static readonly Direction2D NorthWest = new Direction2D(new PosI(-1, -1), 135, 7);
	
	/// <summary>
	///		N E S W
	/// </summary>
	public static readonly Direction2D[] Face4 = [North, East, South, West];
	/// <summary>
	///		N NE E SE S SW W NW
	/// </summary>
	public static readonly Direction2D[] Face8 = [North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest];

	public readonly int Index;
	public readonly PosI Offset;
	public readonly float Angle;
	
	private Direction2D(PosI offset, float angle, int index) {
		Offset = offset;
		Angle = angle;
		Index = index;
	}

	/// <summary>
	///		The clockwise next Pi/2-based direction.
	/// </summary>
	public Direction2D Cw4 {
		get => Face8[(Index + 2) % 8];
	}
	
	/// <summary>
	///		The counterclockwise next Pi/2-based direction.
	/// </summary>
	public Direction2D Ccw4 {
		get => Face8[(Index - 2) % 8];
	}
	
	/// <summary>
	///		The clockwise next Pi/4-based direction.
	/// </summary>
	public Direction2D Cw8 {
		get => Face8[(Index + 1) % 8];
	}
	
	/// <summary>
	///		The counterclockwise next Pi/4-based direction.
	/// </summary>
	public Direction2D Ccw8 {
		get => Face8[(Index - 1) % 8];
	}

	/// <summary>
	///		Gets the nearest direction of an angle.
	/// </summary>
	/// <param name="angle">Angle.</param>
	/// <returns>A direction near to the angle.</returns>
	public static Direction2D GetNearest(float angle) {
		float minDelta = MathF.PI;
		int idx = 0;
		
		foreach (Direction2D d in Face8) {
			float delta = Math.Abs(angle - d.Angle);
			if (minDelta >= delta) {
				minDelta = delta;
				idx = d.Index;
			}
		}

		return Face8[idx];
	}
	
	public static PosI operator +(in PosI pos, Direction2D dir) {
		return pos + dir.Offset;
	}
	
	public static Pos operator +(in Pos pos, Direction2D dir) {
		return pos + (Pos) dir.Offset;
	}
	
	public static Vector2 operator +(in Vector2 vec, Direction2D dir) {
		return vec + dir.Offset;
	}
}
