﻿namespace BlazorIndexedDb.Extensions;
internal static class JsRuntimeExtensions
{
#nullable enable
    public static async ValueTask<T> GetJsonResult<T>(this IJSObjectReference jsObjectReference, string identifier, params object?[]? args)
    {
#nullable disable
        JsonElement element = await jsObjectReference.InvokeAsync<JsonElement>(identifier, args);
        return JsonSerializer.Deserialize<T>(element, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            Converters =
            {
                new CustomJsonStringEnumConverter(),
                new CustomStringBooleanConverter(),
                new CustomJsonTimeSpanConverter(),
                new CustomStringIntegerConverter(),
                new CustomDateTimeNullableConverter()
            }
        });
    }
}
