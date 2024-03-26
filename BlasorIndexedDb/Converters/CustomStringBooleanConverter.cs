namespace BlazorIndexedDb.Converters;

internal class CustomStringBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.String:
                var stringValue = reader.GetString();
                if (bool.TryParse(stringValue, out bool result))
                {
                    return result;
                }
                break;
        }
        throw new JsonException("Value is not a valid boolean.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value ? "true" : "false");
    }
}

