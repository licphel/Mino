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
///     Packet handler host (server), able to connect to multiple handlers.
/// </summary>
public class PacketHandlerHost : IDisposable {
	private readonly List<Channel> _channels = new List<Channel>();
	private readonly ConcurrentQueue<Packet> _consumption = new ConcurrentQueue<Packet>();
	private readonly CancellationTokenSource _disposeCts = new CancellationTokenSource();
	private readonly ReaderWriterLockSlim _lock =
		new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
	private Thread? _acceptThread;
	private volatile bool _active;
	private Thread? _broadcastThread;
	private Socket? _socketLs;
	private bool _disposed;

	/// <summary>
	///     Whether the server is active.
	/// </summary>
	public bool IsActive {
		get => _active;
	}

	/// <summary>
	///     Number of connected clients.
	/// </summary>
	public int ClientCount {
		get {
			_lock.EnterReadLock();
			try {
				return _channels.Count;
			} finally {
				_lock.ExitReadLock();
			}
		}
	}

	/// <summary>
	///     Gets all connected client uids.
	/// </summary>
	public IReadOnlyList<Uid16> ConnectedClients {
		get {
			_lock.EnterReadLock();
			try {
				return _channels.Select(c => c.Uid).ToList();
			} finally {
				_lock.ExitReadLock();
			}
		}
	}

	/// <summary>
	///     Starts the server.
	/// </summary>
	/// <exception cref="ObjectDisposedException">Thrown if server is disposed.</exception>
	/// <exception cref="NetworkException">Thrown if server is already started.</exception>
	/// <exception cref="SocketException">Thrown if socket initialization fails.</exception>
	public void Start() {
		if (_active) {
			Log.Warn("Server socket has already started");
			return;
		}

		try {
			_active = true;

			int port = Net.FindFreePort();

			// Initialize listener socket.
			_socketLs = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
				NoDelay = true,
				LingerState = new LingerOption(false, 0),
				ReceiveTimeout = 5000,
				SendTimeout = 5000
			};
			_socketLs.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

			_socketLs.Bind(new IPEndPoint(IPAddress.Any, port));
			_socketLs.Listen(64);

			// Start accept thread.
			_acceptThread = new Thread(() => acceptLoop(port)) {
				Name = "NetServer-Accept",
				IsBackground = true
			};
			_acceptThread.Start();

			// Start broadcast thread for LAN discovery.
			_broadcastThread = new Thread(() => broadcastLoop(port)) {
				Name = "NetServer-Broadcast",
				IsBackground = true
			};
			_broadcastThread.Start();
		} catch (Exception ex) {
			_active = false;
			cleanup();
			throw new NetworkException("Failed to start server.", ex);
		}
	}

	/// <summary>
	///     Sends a packet to all connected clients.
	/// </summary>
	/// <param name="packet">The packet to send.</param>
	/// <exception cref="NetworkException">Thrown if server is not active.</exception>
	public void Send(Packet packet) {
		if (!IsActive) {
			Log.Warn("Server is not active");
			return;
		}

		_lock.EnterReadLock();
		try {
			foreach (Channel ch in _channels) {
				if (ch.IsConnected) {
					try {
						ch._packets.Add(packet, _disposeCts.Token);
					} catch (OperationCanceledException) {
						// Server stopping, ignore.
					} catch (ObjectDisposedException) {
						// Channel disposed, remove it.
						ch.MarkForRemoval();
					}
				}
			}
		} finally {
			_lock.ExitReadLock();
		}
	}

	/// <summary>
	///     Sends a packet to a specific client.
	/// </summary>
	/// <param name="uid">The uid of the target client.</param>
	/// <param name="packet">The packet to send.</param>
	/// <exception cref="NetworkException">Thrown if server is not active.</exception>
	public void Send(in Uid16 uid, Packet packet) {
		if (!IsActive) {
			Log.Warn("Server is not active");
			return;
		}

		_lock.EnterReadLock();
		try {
			foreach (Channel ch in _channels) {
				if (ch.Uid == uid && ch.IsConnected) {
					try {
						ch._packets.Add(packet, _disposeCts.Token);
						return;
					} catch (OperationCanceledException) {
						// Server stopping.
					} catch (ObjectDisposedException) {
						ch.MarkForRemoval();
					}
				}
			}
		} finally {
			_lock.ExitReadLock();
		}
	}

	/// <summary>
	///     Sends a packet to multiple clients.
	/// </summary>
	/// <param name="uids">The uids of the target clients.</param>
	/// <param name="packet">The packet to send.</param>
	/// <exception cref="NetworkException">Thrown if server is not active.</exception>
	public void Send(ICollection<Uid16> uids, Packet packet) {
		if (!IsActive) {
			Log.Warn("Server is not active");
			return;
		}

		_lock.EnterReadLock();
		try {
			foreach (Channel ch in _channels) {
				if (uids.Contains(ch.Uid) && ch.IsConnected) {
					try {
						ch._packets.Add(packet, _disposeCts.Token);
					} catch (OperationCanceledException) {
						// Server stopping
					} catch (ObjectDisposedException) {
						ch.MarkForRemoval();
					}
				}
			}
		} finally {
			_lock.ExitReadLock();
		}
	}

	/// <summary>
	///     Processes received packets and performs maintenance.
	/// </summary>
	public void Process() {
		if (!_active) {
			return;
		}

		processIncomingPackets();
		checkChannels();
		pollActiveConnections();
	}

	/// <summary>
	///     Finds a channel by its uid.
	/// </summary>
	/// <param name="uid">The channel uid.</param>
	/// <returns>The channel, or null if not found.</returns>
	public Channel? Find(in Uid16 uid) {
		_lock.EnterReadLock();
		try {
			for (int i = _channels.Count - 1; i >= 0; i--) {
				Channel remote = _channels[i];
				if (remote.Uid == uid) {
					return remote;
				}
			}
			return null;
		} finally {
			_lock.ExitReadLock();
		}
	}

	/// <summary>
	///     Kicks a client from the server.
	/// </summary>
	/// <param name="uid">The uid of the client to kick.</param>
	/// <returns>True if client was found and kicked, false otherwise.</returns>
	public bool Kick(in Uid16 uid) {
		_lock.EnterReadLock();
		try {
			for (int i = _channels.Count - 1; i >= 0; i--) {
				Channel ch = _channels[i];
				if (ch.Uid == uid) {
					ch.Disconnect();
					return true;
				}
			}
			return false;
		} finally {
			_lock.ExitReadLock();
		}
	}

	/// <summary>
	///     Gracefully stops the server.
	/// </summary>
	public void Stop() {
		if (!_active) {
			return;
		}

		_active = false;
		_disposeCts.Cancel();

		// Notify all clients.
		_lock.EnterReadLock();
		try {
			foreach (Channel ch in _channels) {
				ch.Disconnect();
			}
		} finally {
			_lock.ExitReadLock();
		}

		cleanup();
	}

	private void acceptLoop(int port) {
		while (_active && !_disposeCts.Token.IsCancellationRequested) {
			try {
				if (_socketLs == null || !_socketLs.IsBound) {
					break;
				}

				if (_socketLs.Poll(1_000_000, SelectMode.SelectRead)) {
					Socket? clientSocket = _socketLs.Accept();

					if (clientSocket != null) {
						Channel channel = new Channel(clientSocket, this);

						_lock.EnterWriteLock();
						try {
							_channels.Add(channel);
						} finally {
							_lock.ExitWriteLock();
						}

						channel.start();
					}
				}
			} catch (SocketException ex) when (
			ex.SocketErrorCode is SocketError.Interrupted or SocketError.OperationAborted) {
				// Cancellation requested.
				break;
			} catch (Exception ex) {
				Log.Debug(ex);

				if (!isSocketErrorRecoverable(ex)) {
					break;
				}
			}
		}
	}

	private void broadcastLoop(int port) {
		using UdpClient broadcaster = new UdpClient();
		broadcaster.EnableBroadcast = true;
		broadcaster.Client.SendTimeout = 1000;

		IPEndPoint broadcastEp = new IPEndPoint(IPAddress.Broadcast, 15000);
		string message = $"{Net.BroadcastHeader}[{port}]";
		byte[] bytes = Encoding.UTF8.GetBytes(message);

		while (_active && !_disposeCts.Token.IsCancellationRequested) {
			try {
				broadcaster.Send(bytes, bytes.Length, broadcastEp);
			} catch (SocketException ex) {
				Log.Debug(ex);

				if (!isSocketErrorRecoverable(ex)) {
					break;
				}
			}

			try {
				Thread.Sleep(1000);
			} catch (ThreadInterruptedException) {
				break;
			}
		}
	}

	private void processIncomingPackets() {
		while (!_consumption.IsEmpty) {
			if (_consumption.TryDequeue(out Packet? packet)) {
				try {
					packet.Perform();
				} catch (Exception ex) {
					Log.Debug(ex);
				}
			}
		}
	}

	private void checkChannels() {
		var toRemove = new List<Channel>();

		_lock.EnterReadLock();
		try {
			foreach (Channel ch in _channels) {
				if (!ch.IsConnected || ch._pollError) {
					toRemove.Add(ch);
				}
			}
		} finally {
			_lock.ExitReadLock();
		}

		if (toRemove.Count > 0) {
			_lock.EnterWriteLock();
			try {
				foreach (Channel ch in toRemove) {
					if (_channels.Remove(ch)) {
						ch.Dispose();
					}
				}
			} finally {
				_lock.ExitWriteLock();
			}
		}
	}

	private void pollActiveConnections() {
		DateTime now = DateTime.UtcNow;

		_lock.EnterReadLock();
		try {
			foreach (Channel ch in _channels) {
				// Check for timeout (10s).
				if ((now - ch.LastHeartbeatTime).TotalMilliseconds > 10_000) {
					ch.MarkForRemoval();
					Log.Info($"Channel {ch.Uid} kicked for timeout.");
					continue;
				}

				// Poll socket for errors.
				ch.poll();
			}
		} finally {
			_lock.ExitReadLock();
		}
	}

	private void cleanup() {
		// Close listener socket.
		try {
			if (_socketLs != null) {
				if (_socketLs.Connected) {
					_socketLs.Shutdown(SocketShutdown.Both);
				}
				_socketLs.Close();
				_socketLs = null;
			}
		} catch (SocketException ex) {
			Log.Debug(ex);
		}

		// Stop threads.
		try {
			_broadcastThread?.Interrupt();
			_broadcastThread = null;

			_acceptThread?.Interrupt();
			_acceptThread = null;
		} catch {
			// ignored
		}

		// Clear channels.
		_lock.EnterWriteLock();
		try {
			foreach (Channel ch in _channels) {
				ch.Dispose();
			}
			_channels.Clear();
		} finally {
			_lock.ExitWriteLock();
		}

		// Clear packet queue.
		while (_consumption.TryDequeue(out _)) { }
	}

	private static bool isSocketErrorRecoverable(Exception ex) {
		return ex is SocketException socketEx &&
			socketEx.SocketErrorCode != SocketError.AccessDenied &&
			socketEx.SocketErrorCode != SocketError.AddressAlreadyInUse &&
			socketEx.SocketErrorCode != SocketError.AddressNotAvailable;
	}

	internal void enqueuePacket(Packet packet) {
		if (packet != null) {
			_consumption.Enqueue(packet);
		}
	}

	internal void removeChannel(Channel channel) {
		_lock.EnterWriteLock();
		try {
			_channels.Remove(channel);
		} finally {
			_lock.ExitWriteLock();
		}
	}

	public void Dispose() {
		if (_disposed) {
			return;
		}
		_disposed = true;
		GC.SuppressFinalize(this);

		Stop();
		_lock.Dispose();
		_disposeCts.Dispose();
	}

	/// <summary>
	///     Packet handler host channel.
	/// </summary>
	public class Channel : IDisposable {
		private readonly CancellationTokenSource _channelCts = new CancellationTokenSource();
		private readonly byte[] _compressionBuffer = new byte[Net.CompressionBufferSize];
		private readonly byte[] _decompressionBuffer = new byte[Net.DecompressionBufferSize];
		internal readonly BlockingCollection<Packet> _packets = new BlockingCollection<Packet>();
		private readonly Socket _socket;
		private volatile bool _connected = true;
		private DateTime _lastPoll = DateTime.UtcNow;
		internal bool _pollError;
		private ByteBuffer _rcvBuf = new ByteBuffer(Net.BufferSize, Endianness.Big);
		private Thread? _receiveThread;
		private Thread? _sendThread;
		private bool _disposed;

		internal Channel(Socket socket, PacketHandlerHost server) {
			_socket = socket;
			Server = server;
			_socket.NoDelay = true;
			_socket.ReceiveTimeout = 5_000;
			_socket.SendTimeout = 5_000;

			EndpointName = socket.RemoteEndPoint?.ToString();
			LastHeartbeatTime = DateTime.UtcNow;

			// We initialize the uid, but it might be soon overrode.
			Uid = Uid16.Create();
		}

		/// <summary>
		///     The endpoint name.
		/// </summary>
		public string? EndpointName { get; }

		/// <summary>
		///     The uid of the channel.
		/// </summary>
		public Uid16 Uid { get; set; }

		/// <summary>
		///     Whether the channel is connected.
		/// </summary>
		public bool IsConnected {
			get => _connected && !_disposed && _socket.Connected;
		}

		/// <summary>
		///     Time of last heartbeat packet.
		/// </summary>
		public DateTime LastHeartbeatTime { get; set; }

		/// <summary>
		///     Parent server of the channel.
		/// </summary>
		public PacketHandlerHost Server { get; }

		/// <summary>
		///     Disconnects the channel.
		/// </summary>
		public void Disconnect() {
			if (!_connected) {
				return;
			}
			_connected = false;
			_channelCts.Cancel();

			try {
				if (_socket.Connected) {
					_socket.Shutdown(SocketShutdown.Both);
					_socket.Close(100);
				}
			} catch (SocketException ex) {
				Log.Debug(ex);
			} finally {
				Server.removeChannel(this);
			}
		}

		/// <summary>
		///     Marks the channel for removal.
		/// </summary>
		public void MarkForRemoval() {
			_pollError = true;
		}

		internal void start() {
			_sendThread = new Thread(sendLoop) {
				Name = $"ServerChannel-Send-{Uid}",
				IsBackground = true
			};
			_sendThread.Start();

			_receiveThread = new Thread(receiveLoop) {
				Name = $"ServerChannel-Receive-{Uid}",
				IsBackground = true
			};
			_receiveThread.Start();
		}

		internal void poll() {
			if (!_connected) {
				return;
			}

			DateTime now = DateTime.UtcNow;
			if ((now - _lastPoll).TotalMilliseconds >= 1_000) {
				_lastPoll = now;

				try {
					_pollError = _socket.Poll(1000, SelectMode.SelectError);
				} catch (SocketException) {
					_pollError = true;
				}
			}
		}

		private void sendLoop() {
			try {
				while (_connected && !_channelCts.Token.IsCancellationRequested) {
					try {
						if (_packets.TryTake(out Packet? packet, 100, _channelCts.Token)) {
							if (packet == null) {
								continue;
							}
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
					} catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut) {
						// Timeout, continue.
					} catch (Exception ex) {
						Log.Debug(ex);
						Disconnect();
						break;
					}
				}
			} catch {
				Disconnect();
			}
		}

		private void receiveLoop() {
			byte[] tempBuffer = new byte[4096];

			try {
				while (_connected && !_channelCts.Token.IsCancellationRequested) {
					try {
						if (_socket.Poll(1_000, SelectMode.SelectRead)) {
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
						// Timeout, continue.
					} catch (SocketException ex) when (
					ex.SocketErrorCode is SocketError.ConnectionReset
						or SocketError.ConnectionAborted) {
						Disconnect();
						break;
					} catch (Exception ex) {
						Log.Debug(ex);
						Disconnect();
						break;
					}
				}
			} catch {
				Disconnect();
			}
		}

		private void processReceivedData() {
			while (_rcvBuf.ReadableBytes >= sizeof(int)) {
				int readMark = _rcvBuf.ReadIndex;
				int len = _rcvBuf.Read<int>();

				if (len is < 0 or > Net.CompressionBufferSize) {
					Log.Debug($"Invalid packet length from {EndpointName}: {len}");
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
					if (packet == null) {
						continue;
					}
					packet.ClientId = Uid;
					packet.OnReach(this);

					Server.enqueuePacket(packet);
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

			_channelCts.Dispose();
			_packets.Dispose();
			_rcvBuf = null!;
		}
	}
}
