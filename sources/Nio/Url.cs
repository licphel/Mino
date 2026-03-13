#region
using System.Reflection;
using Mino.Utility;
#endregion

namespace Mino.Nio;

/// <summary>
///     Universal resource locator.
/// </summary>
public readonly struct Url : IEquatable<Url> {
	public readonly string Path;
	public readonly UrlScheme Scheme;

	public Url(UrlScheme scheme, string url) {
		(Path, Scheme) = standardize(url, scheme);
	}

	public Url(string url) {
		(Path, Scheme) = parseUrl(url);
	}

	/// <summary>
	///     Synchronously opens a stream for I/O.
	/// </summary>
	/// <param name="op">Operations, 'r', 'w' or 'a' (Append).</param>
	/// <returns>A nullable stream.</returns>
	public Stream? OpenStream(string op) {
		return Scheme.OpenStream(this, op);
	}

	/// <summary>
	///     Asynchronously opens a stream for I/O.
	/// </summary>
	/// <param name="op">Operations, 'r', 'w' or 'a' (Append).</param>
	/// <param name="ct">The token.</param>
	/// <returns>A nullable stream.</returns>
	public async Task<Stream?> OpenStreamAsync(string op, CancellationToken ct = default) {
		return await Scheme.OpenStreamAsync(this, op, ct);
	}

	/// <summary>
	///     Converts to a local file path.
	/// </summary>
	/// <returns>A local path name.</returns>
	public string ToFilePath() {
		return Scheme.ToFilePath(this);
	}

	private const int StreamBufferSize = 1048576;

	/// <summary>
	///     Synchronously reads the resource of the url.
	/// </summary>
	/// <returns>A resource byte buffer.</returns>
	/// <exception cref="Crash">If the url has no resource.</exception>
	public ByteBuffer Read() {
		using Stream? stream = OpenStream("r");
		if (stream == null) {
			throw new Crash("URL cannot open a stream");
		}
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream, StreamBufferSize);
		return new ByteBuffer(memoryStream.ToArray());
	}

	/// <summary>
	///     Asynchronously reads the resource of the url.
	/// </summary>
	/// <returns>A resource byte buffer.</returns>
	/// <exception cref="Crash">If the url has no resource.</exception>
	public async Task<ByteBuffer> ReadAsync() {
		Stream? stream = await OpenStreamAsync("r");
		if (stream == null) {
			throw new Crash("URL cannot open a stream");
		}
		using MemoryStream memoryStream = new MemoryStream();
		await stream.CopyToAsync(memoryStream, StreamBufferSize);
		return new ByteBuffer(memoryStream.ToArray());
	}
	
	/// <summary>
	///     Synchronously writes the buffer.
	/// </summary>
	/// <param name="buffer">Buffer to write.</param>
	/// <exception cref="Crash">If the url has no resource.</exception>
	public void Write(ByteBuffer buffer) {
		using Stream? stream = OpenStream("w");
		if (stream == null) {
			throw new Crash("URL cannot open a stream");
		}
		stream.Write(buffer.AsSpan());
		stream.Flush();
	}

	/// <summary>
	///     Asynchronously writes the buffer.
	/// </summary>
	/// <param name="buffer">Buffer to write.</param>
	/// <returns>An async task.</returns>
	/// <exception cref="Crash">If the url has no resource.</exception>
	public async Task WriteAsync(ByteBuffer buffer) {
		Stream? stream = await OpenStreamAsync("w");
		if (stream == null) {
			throw new Crash("URL cannot open a stream");
		}
		await stream.WriteAsync(buffer.AsMemory());
		await stream.FlushAsync();
	}

	public override string ToString() {
		return $"{Scheme}://{Path}";
	}

	public static Url operator /(Url url, string name) {
		if (url.Path.EndsWith('/')) {
			return new Url(url.Scheme, url.Path + name);
		}
		return new Url(url.Scheme, url.Path + "/" + name);
	}

	public static Url operator ~(Url url) {
		int idx = url.Path.LastIndexOf('/');
		if (idx < 0) {
			throw new Crash($"Cannot find the parent directory of '{url}'");
		}
		return new Url(url.Scheme, url.Path.Substring(0, idx + 1));
	}
	
	public static Url GetRelativeName(in Url basePath, in Url path) {
		if (!basePath.Scheme.IsFileBased || !path.Scheme.IsFileBased) {
			throw new Crash($"Url {basePath} is not a file url");
		}
		return new Url(basePath.Scheme, path.ToFilePath().Replace(basePath.ToFilePath() + "/", ""));
	}

	/// <summary>
	///     Finds the executable file's parent directory url.
	/// </summary>
	/// <exception cref="Crash">If cannot find the url.</exception>
	public static Url Local(string sub) {
		Assembly? entryAssembly = Assembly.GetCallingAssembly();
		if (entryAssembly == null) {
			throw new Crash("Cannot find current assembly");
		}
		DirectoryInfo? dir = new FileInfo(entryAssembly.Location).Directory;
		if (dir == null) {
			throw new Crash("Cannot find current assembly location");
		}
		return new Url(UrlScheme.PcFile, dir.FullName + "/run") / sub;
	}

	public bool Equals(Url other) {
		return Path == other.Path && Scheme.Equals(other.Scheme);
	}

	public override bool Equals(object? obj) {
		return obj is Url other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Path, Scheme);
	}

	public static bool operator ==(Url left, Url right) {
		return left.Equals(right);
	}

	public static bool operator !=(Url left, Url right) {
		return !left.Equals(right);
	}

	public static implicit operator string(in Url url) {
		return url.Path;
	}
	
	public static implicit operator Url(string str) {
		return new Url(str);
	}

	private static (string, UrlScheme) parseUrl(string url) {
		int end = url.IndexOf("://", StringComparison.Ordinal);
		if (end > 0) {
			string scheme = url.Substring(0, end).ToLowerInvariant();
			UrlScheme? tryPc = UrlScheme.ByName(scheme);
			if (tryPc != null) {
				return standardize(url.Substring(end + 3), tryPc);
			}
		}
		return standardize(url, UrlScheme.PcFile);
	}

	private static (string, UrlScheme) standardize(string path, UrlScheme scheme) {
		path = path.Replace('\\', '/');
		if (path.EndsWith('/')) {
			path = path.Remove(path.Length - 1);
		}
		return (path, scheme);
	}
}
