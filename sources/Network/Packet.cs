using System.IO.Compression;
using Mino.Nio;

namespace Mino.Network;

/// <summary>
///     Represents a net packet. A packet should have a constructor without any parameters.
/// </summary>
public abstract class Packet {
	// Packet registry.
	private static readonly List<Type> _ID_2_PACKET = new List<Type>();
	private static readonly Dictionary<Type, int> _PACKET_2_ID = new Dictionary<Type, int>();

	/// <summary>
	///     Empty means the packet is client-bound.
	///     Otherwise, it identifies which client the packet comes from.
	/// </summary>
	public Uid16 ClientId = Uid16.Empty;

	static Packet() {
		// Builtin packets.
		Register<HeartbeatPacket>();
		Register<DummyPacket>();
	}

	/// <summary>
	///     Read the buffered data to this packet. This function may be called multiple times.
	/// </summary>
	/// <param name="buffer">The src buffer.</param>
	public abstract void Read(ByteBuffer buffer);

	/// <summary>
	///     Write from this packet to the buffer. This function may be called multiple times.
	/// </summary>
	/// <param name="buffer">The dst buffer.</param>
	public abstract void Write(ByteBuffer buffer);

	/// <summary>
	///     Apply the packet. This function is ensured to be called after read.
	/// </summary>
	public abstract void Perform();

	/// <summary>
	///     Called on the packet reaches a server channel.
	/// </summary>
	/// <param name="channel">The specific channel.</param>
	public virtual void OnReach(PacketHandlerHost.Channel channel) { }

	/// <summary>
	///     Register a packet.
	/// </summary>
	/// <typeparam name="T">Packet type.</typeparam>
	public static void Register<T>() {
		int id = _ID_2_PACKET.Count;
		_ID_2_PACKET.Add(typeof(T));
		_PACKET_2_ID[typeof(T)] = id;
	}

	internal static void encode(byte[] cmpBuffer, Packet packet, out Span<byte> bytes) {
		int pid = _PACKET_2_ID.GetValueOrDefault(packet.GetType(), -1);
		if (pid == -1) {
			bytes = Array.Empty<byte>();
			return;
		}

		ByteBuffer buffer = new ByteBuffer(8, Endianness.Big);
		buffer.Write(pid);
		packet.Write(buffer);

		int byteCount = buffer.ReadableBytes;
		int compressedLen = compress(cmpBuffer, buffer.BufferArray, 0, byteCount);
		buffer.WriteIndex = 0;
		buffer.Write(compressedLen);
		buffer.WriteBytes(cmpBuffer.AsSpan(0, compressedLen));

		bytes = buffer.AsSpan();
	}

	internal static Packet? decode(byte[] decBuffer, ByteBuffer buffer, int len) {
		int begin = buffer.ReadIndex;
		decompress(decBuffer, buffer.BufferArray, begin, len);

		ByteBuffer dataBuffer = new ByteBuffer(decBuffer, Endianness.Big);
		int pid = dataBuffer.Read<int>();

		if (pid < 0 || pid >= _ID_2_PACKET.Count) {
			return null;
		}

		Type type = _ID_2_PACKET[pid];
		if (Activator.CreateInstance(type) is not Packet packet) {
			return null;
		}

		packet.Read(dataBuffer);
		buffer.ReadIndex += len;
		return packet;
	}

	private static int compress(byte[] buffer, byte[] data, int begin, int len) {
		using MemoryStream memoryStream = new MemoryStream(buffer);
		CompressionLevel compLevel = len switch {
			<= 16 => CompressionLevel.NoCompression,
			<= 128 => CompressionLevel.Fastest,
			<= 1024 => CompressionLevel.Optimal,
			_ => CompressionLevel.SmallestSize
		};
		using (BrotliStream brotliStream = new BrotliStream(memoryStream, compLevel, true)) {
			brotliStream.Write(data.AsSpan(begin, len));
			brotliStream.Flush();
		}

		return (int) memoryStream.Position;
	}

	private static int decompress(byte[] buffer, byte[] compressedData, int begin, int len) {
		using MemoryStream inputStream = new MemoryStream(compressedData, begin, len);
		using MemoryStream outputStream = new MemoryStream(buffer);
		using BrotliStream brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress);
		brotliStream.CopyTo(outputStream);
		brotliStream.Flush();
		return (int) outputStream.Position;
	}
}
