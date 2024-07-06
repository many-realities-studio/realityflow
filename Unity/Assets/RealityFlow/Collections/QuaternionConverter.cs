using System;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

namespace RealityFlow.Collections
{
    public class QuaternionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(Quaternion);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (objectType.IsNullable() == false)
                    throw new JsonSerializationException("Cannot convert null value to Quaternion.");

                return null;
            }

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException("A Quaternion must be deserialized from an object");

            reader.Read();

            Quaternion value = Quaternion.identity;
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string property = (string)reader.Value;
                reader.Read();
                if (reader.TokenType != JsonToken.Float)
                    throw new JsonSerializationException("Quaternion properties must be floats");
                _ = property switch
                {
                    "x" => value.x = (float)(double)reader.Value,
                    "y" => value.y = (float)(double)reader.Value,
                    "z" => value.z = (float)(double)reader.Value,
                    "w" => value.w = (float)(double)reader.Value,
                    _ => throw new JsonSerializationException($"Unknown Quaternion property {property} encountered"),
                };
                reader.Read();
            }

            if (reader.TokenType != JsonToken.EndObject)
                throw new JsonSerializationException("A Quaternion must be be ended with EndObject");

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Quaternion quat = (Quaternion)value;

            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(quat.x);
            writer.WritePropertyName("y");
            writer.WriteValue(quat.y);
            writer.WritePropertyName("z");
            writer.WriteValue(quat.z);
            writer.WritePropertyName("w");
            writer.WriteValue(quat.w);
            writer.WriteEndObject();
        }
    }
}