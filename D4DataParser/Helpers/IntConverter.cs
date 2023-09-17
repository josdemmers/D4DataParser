using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace D4DataParser.Helpers
{
    public class IntConverter : JsonConverter<int>
    {
        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value);

        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.True => 1,
                JsonTokenType.False => 0,
                JsonTokenType.String => int.TryParse(reader.GetString(), out var i) ? i : 0,
                JsonTokenType.Number => reader.GetInt32(),
                JsonTokenType.Null => 0,
                _ => throw new JsonException(),
            };


    }
}
