namespace BlazorIndexedDb.Extensions;
internal static class JsRuntimeExtensions
{
    /// <summary>
    /// Options every payload crossing the JS boundary is read with. Kept in one place so the byte
    /// path and the JsonElement path can never drift apart.
    /// </summary>
    internal static JsonSerializerOptions InteropOptions { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters =
        {
            new CustomJsonStringEnumConverter(),
            new CustomStringBooleanConverter(),
            new CustomJsonTimeSpanConverter(),
            new CustomStringIntegerConverter(),
            new CustomDateTimeNullableConverter()
        }
    };

#nullable enable
    public static async ValueTask<T> GetJsonResult<T>(this IJSObjectReference jsObjectReference, string identifier, params object?[]? args)
    {
#nullable disable
        JsonElement element = await jsObjectReference.InvokeAsync<JsonElement>(identifier, args);
        return JsonSerializer.Deserialize<T>(element, InteropOptions);
    }

    /// <summary>
    /// Invoke a JS function that answers with UTF-8 encoded Json and deserialize straight from the
    /// bytes.
    /// </summary>
    /// <remarks>
    /// Prefer this over <see cref="GetJsonResult{T}"/>. A byte[] crosses the boundary as a
    /// Uint8Array, so nothing has to build a JS string, marshal it and re-parse it into a
    /// JsonElement only to serialize that back out again — which is what the JsonElement path does
    /// for every single response.
    /// </remarks>
#nullable enable
    public static async ValueTask<T> GetBytesResult<T>(this IJSObjectReference jsObjectReference, string identifier, params object?[]? args)
    {
#nullable disable
        byte[] bytes = await jsObjectReference.InvokeAsync<byte[]>(identifier, args);
        if (bytes is null || bytes.Length == 0)
            return default;
        return JsonSerializer.Deserialize<T>(bytes, InteropOptions);
    }
}
