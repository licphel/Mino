using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Mino.Network;

/// <summary>
///     Provides network utility functions.
/// </summary>
public static class Net {
	public const int BUFFER_SIZE = 1024 * 1024;
	public const int COMPRESSION_BUFFER_SIZE = 1024 * 1024;
	public const int DECOMPRESSION_BUFFER_SIZE = 1024 * 1024;
	public const string BROADCAST_HEADER = "79062786-685F-45F9-A6F8-94CAE5143CF3";

	/// <summary>
	///     Used in integrated client-server connection.
	/// </summary>
	public static int SharedPort { get; private set; }

	/// <summary>
	///     Finds the first free port greater than the given port.
	/// </summary>
	/// <param name="port">The minimum port.</param>
	/// <returns>A free port.</returns>
	/// <exception cref="Error">Thrown if no port is available.</exception>
	public static int FindFreePort(int port = 8080) {
		var nowPorts = new HashSet<int>();
		IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

		foreach (IPEndPoint ep in properties.GetActiveTcpListeners()) {
			nowPorts.Add(ep.Port);
		}
		foreach (IPEndPoint ep in properties.GetActiveUdpListeners()) {
			nowPorts.Add(ep.Port);
		}

		for (int i = port; i < 65535; i++) {
			if (!nowPorts.Contains(i)) {
				return SharedPort = i;
			}
		}

		throw new Error("no port available");
	}

	/// <summary>
	///     Finds the IP of this machine.
	/// </summary>
	/// <returns>The IP address of this machine.</returns>
	/// <exception cref="Error">Thrown if IP cannot be found.</exception>
	public static string FindLocalIP() {
		IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (IPAddress ip in host.AddressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork) {
				return ip.ToString();
			}
		}
		throw new Error("IP not found");
	}
}
