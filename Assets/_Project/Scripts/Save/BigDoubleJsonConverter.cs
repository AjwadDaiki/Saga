using System;
using BreakInfinity;
using Newtonsoft.Json;

namespace Saga.Save
{
    /// <summary>
    /// Newtonsoft.Json converter for <see cref="BigDouble"/>.
    /// Serializes as a compact object { "m": mantissa, "e": exponent } to
    /// preserve precision exactly (string round-trip can lose lsb on edge cases).
    /// Also tolerates string input ("1.5e308") for hand-edited save files.
    /// </summary>
    public sealed class BigDoubleJsonConverter : JsonConverter<BigDouble>
    {
        public override void WriteJson(JsonWriter writer, BigDouble value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("m");
            writer.WriteValue(value.Mantissa);
            writer.WritePropertyName("e");
            writer.WriteValue(value.Exponent);
            writer.WriteEndObject();
        }

        public override BigDouble ReadJson(JsonReader reader, Type objectType, BigDouble existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return new BigDouble(0);
                case JsonToken.String:
                    return BigDouble.Parse((string)reader.Value);
                case JsonToken.Integer:
                case JsonToken.Float:
                    return new BigDouble(Convert.ToDouble(reader.Value));
                case JsonToken.StartObject:
                    return ReadObject(reader);
                default:
                    throw new JsonSerializationException($"Unexpected token {reader.TokenType} when reading BigDouble.");
            }
        }

        private static BigDouble ReadObject(JsonReader reader)
        {
            double m = 0;
            long e = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                {
                    return new BigDouble(m, e);
                }

                if (reader.TokenType != JsonToken.PropertyName) continue;
                var prop = (string)reader.Value;
                if (!reader.Read()) break;

                switch (prop)
                {
                    case "m":
                    case "mantissa":
                        m = Convert.ToDouble(reader.Value);
                        break;
                    case "e":
                    case "exponent":
                        e = Convert.ToInt64(reader.Value);
                        break;
                }
            }
            return new BigDouble(m, e);
        }
    }
}
