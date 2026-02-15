#region
using System.Reflection;
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
	/// <returns>A nullable stream.</returns>
	public Stream? OpenStream() {
		return Scheme.OpenStream(this);
	}

	/// <summary>
	///     Asynchronously opens a stream for I/O.
	/// </summary>
	/// <returns>A nullable stream.</returns>
	public async Task<Stream?> OpenStreamAsync(CancellationToken ct = default) {
		return await Scheme.OpenStreamAsync(this, ct);
	}

	/// <summary>
	///     Converts to a local file path.
	/// </summary>
	/// <returns>A local path name.</returns>
	public string ToFilePath() {
		return Scheme.ToFilePath(this);
	}

	private const int _STREAM_BUFFER_SIZE = 1048576;

	/// <summary>
	///     Synchronously reads the resource of the url.
	/// </summary>
	/// <returns>A resource byte buffer.</returns>
	/// <exception cref="Error">If the url has no resource.</exception>
	public ByteBuffer Read() {
		Stream? stream = OpenStream();
		if (stream == null) {
			throw new Error("URL cannot open a stream");
		}
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream, _STREAM_BUFFER_SIZE);
		return new ByteBuffer(memoryStream.ToArray());
	}

	/// <summary>
	///     Asynchronously reads the resource of the url.
	/// </summary>
	/// <returns>A resource byte buffer.</returns>
	/// <exception cref="Error">If the url has no resource.</exception>
	public async Task<ByteBuffer> ReadAsync() {
		Stream? stream = await OpenStreamAsync();
		if (stream == null) {
			throw new Error("URL cannot open a stream");
		}
		using MemoryStream memoryStream = new MemoryStream();
		await stream.CopyToAsync(memoryStream, _STREAM_BUFFER_SIZE);
		return new ByteBuffer(memoryStream.ToArray());
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
			throw new Error($"cannot find the parent directory of '{url}'");
		}
		return new Url(url.Scheme, url.Path.Substring(0, idx + 1));
	}

	public static Url GetRelativeName(Url basePath, Url path) {
		if (basePath.Scheme != path.Scheme) {
			throw new Error("cannot operate between difference schemes");
		}
		return new Url(basePath.Scheme, path.Path.Replace(basePath.Path + "/", ""));
	}

	/// <summary>
	///     Finds the executable file's parent directory url.
	/// </summary>
	/// <returns>The executable file's parent directory url.</returns>
	/// <exception cref="Error">If cannot find the url.</exception>
	public static Url GetExecUrl() {
		Assembly? entryAssembly = Assembly.GetCallingAssembly();
		if (entryAssembly == null) {
			throw new Error("cannot find current assembly");
		}
		DirectoryInfo? dir = new FileInfo(entryAssembly.Location).Directory;
		if (dir == null) {
			throw new Error("cannot find current assembly location");
		}
		return new Url(UrlScheme.PcFile, dir.FullName);
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
