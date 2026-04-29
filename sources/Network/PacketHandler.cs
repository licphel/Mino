#region
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Mino.Nio;
using Mino.Utility.Logging;
#endregion

namespace Mino.Network;

/// <summary>
///     Robust packet handler (client).
/// </summary>
public class PacketHandler : IDisposable {
	private readonly Lock _connectionLock = new Lock();
	private readonly ConcurrentQueue<Packet> _consumption = new ConcurrentQueue<Packet>();
	private readonly CancellationTokenSource _disposeCts = new CancellationTokenSource();
	private readonly BlockingCollection<Packet> _packets = new BlockingCollection<Packet>();
	private byte[] _compressionBuffer = new byte[Net.CompressionBufferSize];
	private volatile bool _connected;
	private byte[] _decompressionBuffer = new byte[Net.DecompressionBufferSize];
	private DateTime _lastHeartbeat = DateTime.UtcNow;
	private ByteBuffer _rcvBuf = new ByteBuffer(Net.BufferSize, Endianness.Big);
	private volatile bool _rcvSt;
	private volatile bool _sendSt;
	private Socket? _socket;
	private bool _disposed;

	/// <summary>
	///     The connection type to the server.
	/// </summary>
	public NetConnectionType ConnectionType { get; private set; } = NetConnectionType.NotConnected;

	/// <summary>
	///     The endpoint.
	/// </summary>
	public IPEndPoint? Endpoint { get; private set; }

	/// <summary>
	///     Whether the client is connected.
	/// </summary>
	public bool IsConnected {
		get => _connected;
	}

	/// <summary>
	///     Searches for a server and try to connect to it.
	/// </summary>
	/// <param name="connectionType">The connection type.</param>
	/// <param name="endpoint">The endpoint used in LAN mode.</param>
	/// <returns>Whether the connection is built.</returns>
	/// <exception cref="NetworkException">Thrown if already connected or disposed.</exception>
	/// <exception cref="NetworkException">Thrown if LAN mode is used but endpoint is null.</exception>
	public bool Search(NetConnectionType connectionType, IPEndPoint? endpoint = null) {
		lock (_connectionLock) {
			if (_connected) {
				Log.Warn("Socket is already connected");
				return false;
			}
		}

		string serverIP;
		int serverPort;
		ConnectionType = connectionType;

		switch (connectionType) {
			case NetConnectionType.NotConnected:
				Log.Warn("Unexpected connection type: NotConnected");
				return false;
			case NetConnectionType.LocalArea: {
				serverIP = discover(out serverPort);
				if (string.IsNullOrEmpty(serverIP)) {
					return false;
				}
				break;
			}
			case NetConnectionType.Remote:
				if (endpoint == null) {
					Log.Warn("Null endpoint");
					return false;
				}
				serverIP = endpoint.Address.ToString();
				serverPort = endpoint.Port;
				break;
			case NetConnectionType.Integrated:
			default:
				serverIP = "127.0.0.1";
				serverPort = Net.SharedPort;
				break;
		}

		if (!connect(serverIP, serverPort)) {
			return false;
		}

		try {
			Thread sendThread = new Thread(send);
			sendThread.Name = $"NetClient-Send-{serverIP}:{serverPort}";
			sendThread.IsBackground = true;
			sendThread.Start();

			Thread rcvThread = new Thread(receive);
			rcvThread.Name = $"NetClient-Receive-{serverIP}:{serverPort}";
			rcvThread.IsBackground = true;
			rcvThread.Start();

			// Yield till the threads start.
			DateTime start = DateTime.UtcNow;
			while (!_rcvSt || !_sendSt) {
				if ((DateTime.UtcNow - start).TotalMilliseconds > 5000) {
					Disconnect();
					return false;
				}
				Thread.Sleep(10);
			}

			return true;
		} catch {
			Disconnect();
			throw;
		}
	}

	/// <summary>
	///     Sends a packet to server.
	/// </summary>
	/// <param name="packet">A server-bound packet.</param>
	/// <exception cref="NetworkException">Thrown if not connected.</exception>
	public void Send(Packet packet) {
		if (!_connected) {
			Log.Warn("Socket is not connected");
			return;
		}

		try {
			_packets.Add(packet, _disposeCts.Token);
		} catch (OperationCanceledException) {
			Log.Warn("Socket is disconnecting or disposed");
		}
	}

	/// <summary>
	///     Disconnects from the server.
	/// </summary>
	public void Disconnect() {
		if (!_connected) {
			return;
		}

		lock (_connectionLock) {
			if (!_connected) {
				return;
			}

			_connected = false;
			_disposeCts.Cancel();

			try {
				if (_socket != null && _socket.Connected) {
					_socket.Shutdown(SocketShutdown.Both);
					_socket.Close(100);
				}
			} catch (SocketException ex) {
				// Log but don't throw during cleanup.
				Log.Debug(ex);
			} finally {
				_socket = null;
				_packets.Dispose();
			}
		}
	}

	/// <summary>
	///     Calls to process deserialized packets.
	/// </summary>
	public void Process() {
		while (!_consumption.IsEmpty) {
			if (_consumption.TryDequeue(out Packet? packet)) {
				try {
					packet.Perform();
				} catch (Exception ex) {
					Log.Debug(ex);
				}
			}
		}

		// Send heartbeat packet every 5 second.
		if ((DateTime.UtcNow - _lastHeartbeat).TotalMilliseconds >= 5_000) {
			Send(new HeartbeatPacket());
			_lastHeartbeat = DateTime.UtcNow;
		}
	}

	private bool connect(string ip, int port) {
		try {
			lock (_connectionLock) {
				_socket = new Socket(
					AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
					NoDelay = true,
					ReceiveTimeout = 5_000,
					SendTimeout = 5_000
				};
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

				Task connectTask = _socket.ConnectAsync(ip, port);
				if (!connectTask.Wait(TimeSpan.FromSeconds(5))) {
					return false;
				}

				_connected = true;
				Endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
				return true;
			}
		} catch (Exception ex) {
			Log.Debug(ex);
			return false;
		}
	}

	private static string discover(out int port) {
		using UdpClient listener = new UdpClient(15000);
		IPEndPoint groupEp = new IPEndPoint(IPAddress.Any, 15000);
		DateTime startTime = DateTime.UtcNow;
		listener.Client.ReceiveTimeout = 100;

		while ((DateTime.UtcNow - startTime).TotalSeconds < 10.0) {
			try {
				if (listener.Available > 0) {
					byte[] bytes = listener.Receive(ref groupEp);
					string msg = Encoding.UTF8.GetString(bytes);

					if (msg.StartsWith(Net.BroadcastHeader)) {
						int idx1 = msg.IndexOf('[');
						int idx2 = msg.LastIndexOf(']');
						if (idx1 < 0 || idx2 < 0 || idx2 < idx1) {
							continue;
						}
						string portFound = msg.Substring(idx1 + 1, idx2 - idx1 - 1);
						if (int.TryParse(portFound, out port)) {
							return groupEp.Address.ToString();
						}
					}
				}
			} catch (SocketException) {
				// Timeout, continue.
			}
			Thread.Sleep(100);
		}

		port = 0;
		return string.Empty;
	}

	private void send() {
		if (_socket == null) {
			return;
		}

		try {
			_sendSt = true;

			while (_connected && !_disposeCts.Token.IsCancellationRequested) {
				try {
					// Use timeout to prevent permanent blocking.
					if (_packets.TryTake(out Packet? packet, 100, _disposeCts.Token)) {
						Packet.encode(_compressionBuffer, packet, out Span<byte> span);

						if (span.IsEmpty) {
							continue;
						}

						int sent = 0;
						while (sent < span.Length && _connected) {
							int sentThisTime = _socket.Send(span[sent..], SocketFlags.None);
							if (sentThisTime == 0) {
								throw new SocketException((int) SocketError.ConnectionReset);
							}
							sent += sentThisTime;
						}
					}
				} catch (OperationCanceledException) {
					break;
				} catch (SocketException ex) {
					if (ex.SocketErrorCode != SocketError.TimedOut) {
						throw;
					}
					// Timeout, continue.
				}
			}
		} catch (Exception ex) {
			Log.Debug(ex);
		} finally {
			if (_connected) {
				Disconnect();
			}
		}
	}

	private void receive() {
		if (_socket == null) {
			return;
		}

		try {
			_rcvSt = true;
			byte[] tempBuffer = new byte[Net.BufferSize];

			while (_connected && !_disposeCts.Token.IsCancellationRequested) {
				try {
					if (_socket.Poll(1000, SelectMode.SelectRead)) {
						int bytesRead = _socket.Receive(tempBuffer, SocketFlags.None);

						if (bytesRead == 0) {
							Disconnect();
							break;
						}

						// Copy to receive buffer.
						if (_rcvBuf.FreeBytes < bytesRead) {
							_rcvBuf.Compact();
							if (_rcvBuf.FreeBytes < bytesRead) {
								_rcvBuf.Ensure(bytesRead - _rcvBuf.FreeBytes);
							}
						}

						Array.Copy(
							tempBuffer, 0, _rcvBuf.BufferArray, _rcvBuf.WriteIndex, bytesRead);
						_rcvBuf.WriteIndex += bytesRead;

						processReceivedData();
					}
				} catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut) {
					// Timeout is expected, continue.
				} catch (SocketException ex) when (
				ex.SocketErrorCode is SocketError.ConnectionReset
					or SocketError.ConnectionAborted) {
					Disconnect();
					break;
				}
			}
		} catch (Exception ex) {
			Log.Debug(ex);
		} finally {
			if (_connected) {
				Disconnect();
			}
		}
	}

	private void processReceivedData() {
		while (_rcvBuf.ReadableBytes >= sizeof(int)) {
			int readMark = _rcvBuf.ReadIndex;
			int len = _rcvBuf.Read<int>();

			if (len is < 0 or > Net.CompressionBufferSize) {
				// Invalid length, disconnect.
				Log.Debug($"Invalid packet length: {len}");
				Disconnect();
				return;
			}

			if (_rcvBuf.ReadableBytes < len) {
				_rcvBuf.ReadIndex = readMark;

				if (_rcvBuf.FreeBytes <= len + sizeof(int)) {
					_rcvBuf.Compact();
				}
				break;
			}

			try {
				Packet? packet = Packet.decode(_decompressionBuffer, _rcvBuf, len);
				if (packet != null) {
					_consumption.Enqueue(packet);
				}
			} catch (Exception ex) {
				Log.Debug(ex);
				// Skip corrupted packet but stay connected.
				_rcvBuf.ReadIndex += len;
			}

			if (_rcvBuf.ReadableBytes <= 0) {
				_rcvBuf.Clear();
			} else if (_rcvBuf.ReadIndex >= _rcvBuf.Capacity / 2) {
				_rcvBuf.Compact();
			}
		}
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		GC.SuppressFinalize(this);

		Disconnect();
		_disposeCts.Dispose();
		_rcvBuf = null!;
		_compressionBuffer = null!;
		_decompressionBuffer = null!;
	}
}
