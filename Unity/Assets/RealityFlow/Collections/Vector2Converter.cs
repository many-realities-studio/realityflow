using System;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

namespace RealityFlow.Collections
{
    public class Vector2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(Vector2);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (objectType.IsNullable() == false)
                    throw new JsonSerializationException("Cannot convert null value to Vector2.");

                return null;
            }

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException("A Vector2 must be deserialized from an object");

            reader.Read();

            Vector2 value = Vector2.zero;
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string property = (string)reader.Value;
                reader.Read();
                if (reader.TokenType != JsonToken.Float)
                    throw new JsonSerializationException("Vector2 properties must be floats");
                _ = property switch
                {
                    "x" => value.x = (float)(double)reader.Value,
                    "y" => value.y = (float)(double)reader.Value,
                    _ => throw new JsonSerializationException($"Unknown Vector2 property {property} encountered"),
                };
                reader.Read();
            }

            if (reader.TokenType != JsonToken.EndObject)
                throw new JsonSerializationException("A Vector2 must be be ended with EndObject");

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Vector2 vector = (Vector2)value;

            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(vector.x);
            writer.WritePropertyName("y");
            writer.WriteValue(vector.y);
            writer.WriteEndObject();
        }
    }
}