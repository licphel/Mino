namespace Mino.Nio.NBT;

/// <summary>
///     Provides tag map serialization and tag validation.
/// </summary>
public static class TagSystem {
	/*
	 * TYPE CONVERSION RULES:
	 *
	 * I. value type: take int for example,
	 * | Get<int>("key") -> 0 if not exists or null.
	 * | Get<int>("key", fallback) -> fallback if not exists or null.
	 * | TryGet<int>("key", out int val) -> false if not exists or null. Otherwise, val = actual value.
	 *
	 * II. ref type: we only support string.
	 * | Get<string>("key") -> string.Empty if not exists or null.
	 * | Get<string>("key", fallback) -> fallback if not exists or null.
	 * | TryGet<string>("key", out string val) -> false if not exists or null. Otherwise, val = actual value.
	 *
	 * Anyway, NEVER use nullable generic in tag system (like int? v = map.Get<int?>("key"), which is undefined.)
	 */

	// Type IDs.
	private const byte NULL = 1;
	private const byte MAP = 2;
	private const byte LIST = 3;
	private const byte BYTE = 4;
	private const byte SHORT = 5;
	private const byte USHORT = 6;
	private const byte INT = 7;
	private const byte UINT = 8;
	private const byte LONG = 9;
	private const byte ULONG = 10;
	private const byte FLOAT = 11;
	private const byte DOUBLE = 12;
	private const byte BOOL = 13;
	private const byte STRING = 14;
	private const byte BYTES = 15;

	public static bool Validate(object? o) {
		return Tell(o) != 0;
	}

	public static T GetNonnullFallback<T>(T? fallback) {
		if (fallback != null) {
			return fallback;
		}
		if (typeof(T) == typeof(string)) {
			return (T) Convert.ChangeType(string.Empty, typeof(T));
		}
		throw new Error("unsupported type");
	}

	public static T AsWithFallback<T>(object? v, T? fallback) {
		if (v == null) {
			return GetNonnullFallback(fallback);
		}
		return (T) Convert.ChangeType(v, typeof(T));
	}

	public static T AsWithFallback<T>(object? v, Func<T> fallback) {
		if (v == null) {
			return fallback.Invoke();
		}
		return (T) Convert.ChangeType(v, typeof(T));
	}

	public static byte Tell(object? o) {
		if (o is null) {
			return NULL;
		}
		return o switch {
			TagMap => MAP,
			TagList => LIST,
			byte => BYTE,
			short => SHORT,
			ushort => USHORT,
			int => INT,
			uint => UINT,
			long => LONG,
			ulong => ULONG,
			float => FLOAT,
			double => DOUBLE,
			bool => BOOL,
			string => STRING,
			byte[] => BYTES,
			_ => 0
		};
	}

	/// <summary>
	///     Encodes a tag map into a byte buffer.
	/// </summary>
	/// <param name="map">Serialized map.</param>
	/// <param name="output">Output buffer.</param>
	public static void Encode(TagMap map, ByteBuffer output) {
		foreach ((string key, var val) in map) {
			output.Write(Tell(val));
			output.WriteString(key);
			if (val != null) {
				encodePrimitive(val, output);
			}
		}

		output.Write<byte>(0); // Exit.
	}

	/// <summary>
	///     Deserializes a tag map from a byte buffer.
	/// </summary>
	/// <param name="input">Input buffer.</param>
	/// <returns>A new tag map deserialized from the input.</returns>
	public static TagMap Decode(ByteBuffer input) {
		TagMap map = new TagMap();

		while (true) {
			byte id = input.Read<byte>();
			if (id == 0) {
				break;
			}

			string key = input.ReadString();
			object? data;

			if (id == NULL) {
				data = null;
			} else {
				data = decodePrimitive(input, id);
			}
			map.Set(key, data);
		}

		return map;
	}

	private static object? decodePrimitive(ByteBuffer input, byte id) {
		if (id == MAP) {
			return Decode(input);
		}
		if (id == LIST) {
			TagList list = new TagList();
			int size = input.Read<int>();
			for (int i = 0; i < size; i++) {
				byte type = input.Read<byte>();
				object? data = decodePrimitive(input, type);
				list.Add(data);
			}
			return list;
		}
		if (id == BYTE) {
			return input.Read<byte>();
		}
		if (id == SHORT) {
			return input.Read<short>();
		}
		if (id == USHORT) {
			return input.Read<ushort>();
		}
		if (id == INT) {
			return input.Read<int>();
		}
		if (id == UINT) {
			return input.Read<uint>();
		}
		if (id == LONG) {
			return input.Read<long>();
		}
		if (id == ULONG) {
			return input.Read<ulong>();
		}
		if (id == FLOAT) {
			return input.Read<float>();
		}
		if (id == DOUBLE) {
			return input.Read<double>();
		}
		if (id == BOOL) {
			return input.Read<bool>();
		}
		if (id == STRING) {
			return input.ReadString();
		}
		if (id == BYTES) {
			int size = input.Read<int>();
			byte[] array = new byte[size];
			input.ReadBytes(array, size);
			return array;
		}
		return null;
	}

	private static void encodePrimitive(object o, ByteBuffer output) {
		if (o is TagMap map) {
			Encode(map, output);
		} else if (o is TagList list) {
			output.Write(list.Count);
			foreach (object v in list) {
				output.Write(Tell(v));
				encodePrimitive(v, output);
			}
		} else if (o is byte b1) {
			output.Write(b1);
		} else if (o is short s1) {
			output.Write(s1);
		} else if (o is ushort us) {
			output.Write(us);
		} else if (o is int i) {
			output.Write(i);
		} else if (o is uint ui) {
			output.Write(ui);
		} else if (o is long l) {
			output.Write(l);
		} else if (o is ulong ul) {
			output.Write(ul);
		} else if (o is float f) {
			output.Write(f);
		} else if (o is double d) {
			output.Write(d);
		} else if (o is bool b2) {
			output.Write(b2);
		} else if (o is string s2) {
			output.WriteString(s2);
		} else if (o is byte[] bytes) {
			output.WriteBytes(bytes);
		}
	}
}
