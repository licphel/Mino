namespace Mino.Mathematics.ThreeDim;

/// <summary>
///     3D camera-based euler angle.
/// </summary>
public readonly struct Euler : IEquatable<Euler> {
	public readonly float Yaw = 0.0F;
	public readonly float Pitch = 0.0F;
	public readonly float Roll = 0.0F;

	public Euler() {
	}

	public Euler(float yaw, float pitch, float roll) {
		Yaw = yaw;
		Pitch = pitch;
		Roll = roll;
	}

	public override string ToString() {
		return $"(Y:{Yaw:3F} P:{Pitch:3F} R:{Roll:3F})";
	}

	public bool Equals(Euler other) {
		return Comparison.DoEqual(Yaw, other.Yaw)
			&& Comparison.DoEqual(Pitch, other.Pitch)
			&& Comparison.DoEqual(Roll, other.Roll);
	}

	public override bool Equals(object? obj) {
		return obj is Euler other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Yaw, Pitch, Roll);
	}

	public static bool operator ==(Euler left, Euler right) {
		return left.Equals(right);
	}

	public static bool operator !=(Euler left, Euler right) {
		return !left.Equals(right);
	}
}
