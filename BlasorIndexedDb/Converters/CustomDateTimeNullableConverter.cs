namespace BlazorIndexedDb.Converters;
internal class CustomDateTimeNullableConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            string dateString = reader.GetString();
            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return date;
            }
            return null;
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
            JsonElement root = jsonDocument.RootElement;
            if (root.TryGetProperty("date", out var dateElement) && dateElement.TryGetDateTime(out DateTime date))
            {
                return date;
            }        
            if (root.TryGetProperty("Date", out dateElement) && dateElement.TryGetDateTime(out date))
            {
                return date;
            }
            return null;
        }
        else
        {
            return null;
        }
    }


    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.Value.ToString("o")); // Formato ISO 8601
        }
    }
}
