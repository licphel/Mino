namespace Mino.Nio;

/// <summary>
///     Utility functions for urls using 'file://' scheme.
/// </summary>
public static class FileUtil {
	/// <summary>
	///     Identifies the type of a file url.
	/// </summary>
	public enum PathType {
		Directory,
		File,
		NotExist
	}

	/// <summary>
	///     Gets the name of a url.
	/// </summary>
	/// <param name="url">Target file url.</param>
	/// <returns>Name with extension.</returns>
	public static string GetName(in Url url) {
		ensureFile(url);
		int idx = url.Path.LastIndexOf('/');
		return idx >= 0 ? url.Path.Substring(idx + 1) : url.Path;
	}

	/// <summary>
	///     Gets the name of a url without extension name.
	/// </summary>
	/// <param name="url">Target file url.</param>
	/// <returns>Name without extension.</returns>
	public static string GetNameNoExtension(in Url url) {
		ensureFile(url);
		string name = GetName(url);
		int idx = name.LastIndexOf('.');
		return idx >= 0 ? name.Substring(0, idx) : name;
	}

	/// <summary>
	///     Gets the extension name of a url without '.' (dot).
	/// </summary>
	/// <param name="url">Target file url.</param>
	/// <returns>Extension name without dot.</returns>
	public static string GetExtension(in Url url) {
		ensureFile(url);
		string name = GetName(url);
		int idx = name.LastIndexOf('.');
		return idx >= 0 ? name.Substring(idx + 1) : string.Empty;
	}

	/// <summary>
	///     Gets the type of a file url.
	/// </summary>
	/// <param name="url">Target file url.</param>
	/// <returns>The path type of the url.</returns>
	public static PathType GetTypeOf(in Url url) {
		ensureFile(url);
		try {
			FileAttributes attributes = File.GetAttributes(url.Path);
			if (attributes.HasFlag(FileAttributes.Directory)) {
				return PathType.Directory;
			}
			return PathType.File;
		} catch {
			return PathType.NotExist;
		}
	}

	/// <summary>
	///     Lists all subordinate directories of a file url.
	/// </summary>
	/// <param name="url">Target file url.</param>
	/// <returns>The collection of subordinate directories.</returns>
	public static IEnumerable<Url> SubDirectories(in Url url) {
		ensureFile(url);
		return Array.ConvertAll(Directory.GetDirectories(url.Path), path => new Url(path));
	}

	/// <summary>
	///     Lists all subordinate files of a file url.
	/// </summary>
	/// <param name="url">Target file url.</param>
	/// <returns>The collection of subordinate files.</returns>
	public static IEnumerable<Url> SubFiles(in Url url) {
		ensureFile(url);
		return Array.ConvertAll(Directory.GetFiles(url.Path), path => new Url(path));
	}

	/// <summary>
	///     Deletes a file or directory.
	/// </summary>
	/// <param name="url">Target file url.</param>
	public static void Delete(in Url url) {
		ensureFile(url);
		PathType urlType = GetTypeOf(url);
		if (urlType == PathType.File) {
			File.Delete(url.Path);
		} else if (urlType == PathType.Directory) {
			Directory.Delete(url.Path, true);
		}
	}

	/// <summary>
	///     Moves the file or directory at src url to the dst url.
	/// </summary>
	/// <param name="src">Src file url.</param>
	/// <param name="dst">Dst file url.</param>
	/// <exception cref="Error">Thrown if src url does not exist.</exception>
	public static void Move(in Url src, in Url dst) {
		ensureFile(src);
		PathType urlType = GetTypeOf(src);
		if (urlType == PathType.NotExist) {
			throw new Error($"cannot find '{src}'");
		}

		// Ensure destination's parent.
		string? destDir = Path.GetDirectoryName(dst.Path);
		if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir)) {
			Directory.CreateDirectory(destDir);
		}

		if (urlType == PathType.File) {
			File.Move(src.Path, dst.Path);
		} else if (urlType == PathType.Directory) {
			Directory.Move(src.Path, dst.Path);
		}
	}

	/// <summary>
	///     Creates the specified file and all its parent directories.
	/// </summary>
	/// <param name="url">Target file url.</param>
	public static void CreateFile(in Url url) {
		ensureFile(url);
		try {
			Url parent = ~url;
			Directory.CreateDirectory(parent.Path);
		} catch {
			// ignored.
		}
		File.Create(url.Path).Close();
	}

	/// <summary>
	///     Creates the specified directory and all its parent directories.
	/// </summary>
	/// <param name="url">Target file url.</param>
	public static void CreateDirectory(in Url url) {
		ensureFile(url);
		Directory.CreateDirectory(url.Path);
	}

	private static void ensureFile(in Url url) {
		if (url.Scheme != UrlScheme.PcFile) {
			throw new Error("URL is not 'file://' scheme");
		}
	}
}
