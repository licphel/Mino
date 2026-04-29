#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace Mino.Nio.NBT;

// Json conversion tool class.
internal class NBTJsonConverter : JsonConverter<NBTCompound> {
	public override NBTCompound Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.StartObject) {
			throw new JsonException("Expected start of object");
		}

		NBTCompound map = new NBTCompound();

		while (reader.Read()) {
			if (reader.TokenType == JsonTokenType.EndObject) {
				return map;
			}
			if (reader.TokenType != JsonTokenType.PropertyName) {
				throw new JsonException("Expected property name");
			}

			string key = reader.GetString()!;
			reader.Read();
			object? value = _read(ref reader, options);
			map.Set(key, value);
		}

		throw new JsonException("Unexpected end of JSON");
	}

	private object? _read(ref Utf8JsonReader reader, JsonSerializerOptions options) {
		switch (reader.TokenType) {
			case JsonTokenType.Null:
				return null;
			case JsonTokenType.True:
				return true;
			case JsonTokenType.False:
				return false;
			case JsonTokenType.String:
				return reader.GetString();
			case JsonTokenType.Number:
				if (reader.TryGetByte(out byte byteValue)) {
					return byteValue;
				}
				if (reader.TryGetSByte(out sbyte sbyteValue)) {
					return sbyteValue;
				}
				if (reader.TryGetInt16(out short shortValue)) {
					return shortValue;
				}
				if (reader.TryGetUInt16(out ushort ushortValue)) {
					return ushortValue;
				}
				if (reader.TryGetInt32(out int intValue)) {
					return intValue;
				}
				if (reader.TryGetUInt32(out uint uintValue)) {
					return uintValue;
				}
				if (reader.TryGetInt64(out long longValue)) {
					return longValue;
				}
				if (reader.TryGetUInt64(out ulong ulongValue)) {
					return ulongValue;
				}
				if (reader.TryGetSingle(out float floatValue)) {
					return floatValue;
				}
				if (reader.TryGetDouble(out double doubleValue)) {
					return doubleValue;
				}
				return (double) reader.GetDecimal();
			case JsonTokenType.StartObject:
				return Read(ref reader, typeof(NBTCompound), options);
			case JsonTokenType.StartArray:
				NBTList list = new NBTList();
				while (reader.Read()) {
					if (reader.TokenType == JsonTokenType.EndArray) {
						return list;
					}
					object? value = _read(ref reader, options);
					list.Add(value);
				}
				throw new JsonException("Unexpected end of array");
			case JsonTokenType.None:
			case JsonTokenType.EndObject:
			case JsonTokenType.EndArray:
			case JsonTokenType.PropertyName:
			case JsonTokenType.Comment:
			default:
				throw new JsonException($"Unsupported token type: {reader.TokenType}");
		}
	}

	public override void Write(Utf8JsonWriter writer, NBTCompound value, JsonSerializerOptions options) {
		writer.WriteStartObject();
		
		foreach (KeyValuePair<string, object> kv in value) {
			writer.WritePropertyName(kv.Key);
			_write(writer, kv.Value, options);
		}

		writer.WriteEndObject();
	}

	private void _write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options) {
		switch (value) {
			case null:
				writer.WriteNullValue();
				break;
			case NBTCompound map:
				Write(writer, map, options);
				break;
			case NBTList list:
				writer.WriteStartArray();
				foreach (object item in list) {
					_write(writer, item, options);
				}
				writer.WriteEndArray();
				break;
			case string str:
				writer.WriteStringValue(str);
				break;
			case bool b:
				writer.WriteBooleanValue(b);
				break;
			case byte[] bytes:
				writer.WriteBase64StringValue(bytes);
				break;
			case byte b:
				writer.WriteNumberValue(b);
				break;
			case short s:
				writer.WriteNumberValue(s);
				break;
			case ushort us:
				writer.WriteNumberValue(us);
				break;
			case int i:
				writer.WriteNumberValue(i);
				break;
			case uint ui:
				writer.WriteNumberValue(ui);
				break;
			case long l:
				writer.WriteNumberValue(l);
				break;
			case ulong ul:
				writer.WriteNumberValue(ul);
				break;
			case float f:
				writer.WriteNumberValue(f);
				break;
			case double d:
				writer.WriteNumberValue(d);
				break;
			default:
				writer.WriteStringValue(value.ToString());
				break;
		}
	}
}
