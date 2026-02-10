using Mino.Nio;

namespace Mino.Network;

/// <summary>
///     Heartbeat packet to check client timeout.
/// </summary>
public class HeartbeatPacket : Packet {
	public override void Read(ByteBuffer buffer) {
		// Nothing.
	}

	public override void Write(ByteBuffer buffer) {
		// Nothing.
	}

	public override void Perform() {
		// Nothing.
	}

	public override void OnReach(PacketHandlerHost.Channel channel) {
		// Update heartbeat time to avoid being kicked.
		channel.LastHeartbeatTime = DateTime.UtcNow;
	}
}
