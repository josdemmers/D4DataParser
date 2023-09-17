using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace D4DataParser.Helpers
{
    public class UIntConverter : JsonConverter<uint>
    {
        public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value);

        public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.True => 1,
                JsonTokenType.False => 0,
                JsonTokenType.String => uint.TryParse(reader.GetString(), out var i) ? i : 0,
                JsonTokenType.Number => (uint)reader.GetInt32(),
                JsonTokenType.Null => 0,
                _ => throw new JsonException(),
            };
    }
}
