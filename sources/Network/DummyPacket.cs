#region
using Mino.Framework;
using Mino.Nio;
#endregion

namespace Mino.Network;

/// <summary>
///     A tester dummy packet carrying a piece of text.
/// </summary>
public class DummyPacket : Packet {
	private string _text;

	public DummyPacket() {
		_text = string.Empty;
	}

	public DummyPacket(string text) {
		_text = text;
	}

	public override void Read(ByteBuffer buffer) {
		_text = buffer.ReadString();
	}

	public override void Write(ByteBuffer buffer) {
		buffer.WriteString(_text);
	}

	public override void Perform() {
		Logger.Global.Info(_text);
	}
}
