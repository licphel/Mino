using Mino.Nio.NBT;
using Mino.Utility;

namespace Mino.Modular;

/// <summary>
///		A language translation group.
/// </summary>
public sealed class Language {
	private string _key;
	private NBTCompound _i18nMap = new NBTCompound();
	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	
	public Language(string key) {
		_key = key;
	}

	/// <summary>
	///		Appends a translation map to the language object.
	/// </summary>
	/// <param name="map"></param>
	public void Append(NBTCompound map) {
		_lock.EnterWriteLock();
		foreach (var kv in map) {
			if (kv.Value is not string) {
				continue; // Jump through non-string values.
			}
			_i18nMap.Set(kv.Key, kv.Value);
		}
		_lock.ExitWriteLock();
	}

	///  <summary>
	/// 		Gets a translation by a specific key.
	///  </summary>
	///  <param name="key">The translation key.</param>
	///  <param name="fallback">The fallback value.</param>
	///  <returns>The translation value with a fallback.</returns>
	public string Get(string key, in Maybe<string> fallback = default) {
		_lock.EnterReadLock();
		string result = _i18nMap.Get(key, fallback);
		_lock.ExitReadLock();
		return result;
	}
}
