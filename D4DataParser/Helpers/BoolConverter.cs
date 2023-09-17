﻿using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace D4DataParser.Helpers
{
    public class BoolConverter : JsonConverter<bool>
    {
        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
            writer.WriteBooleanValue(value);

        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.String => bool.TryParse(reader.GetString(), out var b) ? b : string.IsNullOrWhiteSpace(reader.GetString()) ? false : reader.GetString().Equals("1") ? true : throw new JsonException(),
                JsonTokenType.Number => reader.TryGetInt64(out long l) ? Convert.ToBoolean(l) : reader.TryGetDouble(out double d) ? Convert.ToBoolean(d) : false,
                JsonTokenType.Null => false,
                _ => throw new JsonException(),
            };
    }
}
