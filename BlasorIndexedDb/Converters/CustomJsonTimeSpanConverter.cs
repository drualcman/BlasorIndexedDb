namespace BlazorIndexedDb.Converters;
internal class CustomJsonTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token type.");
        }

        long? ticks = null;
        int days = 0, hours = 0, minutes = 0, seconds = 0, milliseconds = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return ticks.HasValue ? new TimeSpan(ticks.Value) : new TimeSpan(days, hours, minutes, seconds, milliseconds);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "Ticks":
                        ticks = reader.GetInt64();
                        break;
                    case "Days":
                        days = reader.GetInt32();
                        break;
                    case "Hours":
                        hours = reader.GetInt32();
                        break;
                    case "Minutes":
                        minutes = reader.GetInt32();
                        break;
                    case "Seconds":
                        seconds = reader.GetInt32();
                        break; 
                    case "Milliseconds":
                        milliseconds = reader.GetInt32();
                        break; 
                    default:
                        break;
                }
            }
        }

        throw new JsonException("JSON is not a valid TimeSpan format.");
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("Days", value.Days);
        writer.WriteNumber("Hours", value.Hours);
        writer.WriteNumber("Minutes", value.Minutes);
        writer.WriteNumber("Seconds", value.Seconds);
        writer.WriteNumber("Milliseconds", value.Milliseconds);
        writer.WriteNumber("Ticks", value.Ticks);
        writer.WriteEndObject();
    }
}
