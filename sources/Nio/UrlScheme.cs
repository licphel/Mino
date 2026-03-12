#region
using System.Collections.Concurrent;
#endregion

namespace Mino.Nio;

/// <summary>
///     Url scheme.
/// </summary>
public interface UrlScheme {
	// Name-scheme mapping.
	private static readonly ConcurrentDictionary<string, UrlScheme> _schemeMapping =
		new ConcurrentDictionary<string, UrlScheme>();

	// Some builtin schemes.
	public static readonly UrlScheme PcFile = RegisterScheme("file", new FileImpl());
	public static readonly UrlScheme PcHttp = RegisterScheme("http", new HttpImpl("http"));
	public static readonly UrlScheme PcHttps = RegisterScheme("https", new HttpImpl("https"));
	public static readonly UrlScheme PcRf = RegisterScheme("rf", new RfImpl());
	public static readonly UrlScheme PcConsole = RegisterScheme("console", new ConsoleImpl());

	public Url this[string name] {
		get => new Url(this, name);
	}

	/// <summary>
	///     Whether the scheme is file-based.
	/// </summary>
	bool IsFileBased { get; }

	Stream? OpenStream(Url url, string op);

	Task<Stream?> OpenStreamAsync(Url url, string op, CancellationToken ct = default);

	string ToFilePath(Url url);

	public static UrlScheme RegisterScheme(string name, UrlScheme scheme) {
		return _schemeMapping[name] = scheme;
	}

	public static UrlScheme ByName(string name) {
		if (_schemeMapping.TryGetValue(name, out UrlScheme? p)) {
			return p;
		}
		throw new Error($"URL scheme {name} not supported");
	}

	// 'file' scheme implementation.
	private sealed class FileImpl : UrlScheme {
		public bool IsFileBased {
			get => true;
		}

		public Stream OpenStream(Url url, string op) {
			if (op == "r") {
				return new FileStream(url.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			if (op == "w") {
				return new FileStream(url.Path, FileMode.Create, FileAccess.Write, FileShare.Write);
			}
			if (op == "a") {
				return new FileStream(url.Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
			}
			throw new Error($"unknown op: {op}");
		}

		public async Task<Stream?> OpenStreamAsync(Url url, string op, CancellationToken ct) {
			if (op == "r") {
				return await Task.Run(
					() => new FileStream(url.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true), ct);
			}
			if (op == "w") {
				return await Task.Run(
					() => new FileStream(url.Path, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, true), ct);
			}
			if (op == "a") {
				return await Task.Run(
					() => new FileStream(
						url.Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 4096, true), ct);
			}
			throw new Error($"unknown op: {op}");
		}

		public string ToFilePath(Url url) {
			return url.Path;
		}

		public override string ToString() {
			return "file";
		}
	}

	// 'http'/'https' scheme implementation.
	private sealed class HttpImpl : UrlScheme {
		private readonly HttpClient _httpClient = new HttpClient();
		private readonly string _scheme;

		public HttpImpl(string scheme) {
			_scheme = scheme;

			// Automatically dispose http client.
			AppDomain.CurrentDomain.ProcessExit += (_, _) => {
				_httpClient.Dispose();
			};
		}

		public bool IsFileBased {
			get => false;
		}

		public Stream? OpenStream(Url url, string op) {
			if (op != "r") {
				throw new Error("http cannot write");
			}
			try {
				HttpResponseMessage response =
					_httpClient.GetAsync(url.ToString()).GetAwaiter().GetResult();
				response.EnsureSuccessStatusCode();
				return response.Content.ReadAsStream();
			} catch {
				return null;
			}
		}

		public async Task<Stream?> OpenStreamAsync(Url url, string op, CancellationToken ct) {
			if (op != "r") {
				throw new Error("http cannot write");
			}
			try {
				HttpResponseMessage response = await _httpClient.GetAsync(
					url.ToString(), HttpCompletionOption.ResponseHeadersRead, ct);
				response.EnsureSuccessStatusCode();
				return await response.Content.ReadAsStreamAsync(ct);
			} catch {
				return null;
			}
		}

		public string ToFilePath(Url url) {
			throw new Error("http url to file");
		}

		public override string ToString() {
			return _scheme;
		}
	}

	// 'rf' (Resource Finding) scheme implementation,
	// like 'rf://example/sound/test.wav'.
	private sealed class RfImpl : UrlScheme {
		private static readonly Url _runtimeModUrl = Url.Runtime / "mod";

		public bool IsFileBased {
			get => true;
		}

		public Stream? OpenStream(Url url, string op) {
			return (_runtimeModUrl / url.Path).OpenStream(op);
		}

		public Task<Stream?> OpenStreamAsync(Url url, string op, CancellationToken ct) {
			return (_runtimeModUrl / url.Path).OpenStreamAsync(op, ct);
		}

		public string ToFilePath(Url url) {
			return (_runtimeModUrl / url.Path).Path;
		}

		public override string ToString() {
			return "rf";
		}
	}

	// 'console' scheme implementation.
	// Supports: 'console://in', 'console://out', 'console://err'.
	private sealed class ConsoleImpl : UrlScheme {
		public bool IsFileBased {
			get => false;
		}

		public Stream? OpenStream(Url url, string op) {
			if (op != "w") {
				throw new Error("console cannot read");
			}
			return url.Path.ToLowerInvariant() switch {
				"stdin" or "in" => Console.OpenStandardInput(),
				"stdout" or "out" => Console.OpenStandardOutput(),
				"stderr" or "err" => Console.OpenStandardError(),
				_ => null
			};
		}

		public Task<Stream?> OpenStreamAsync(Url url, string op, CancellationToken ct = default) {
			return Task.FromResult(OpenStream(url, op));
		}

		public string ToFilePath(Url url) {
			throw new Error("console url to file");
		}

		public override string ToString() {
			return "console";
		}
	}
}
