namespace BlazorIndexedDb.Helpers
{
    /// <summary>
    /// Tools to work with objects
    /// </summary>
    internal sealed class ObjectConverter
    {
        #region Methods
        /// <summary>
        /// Convert into a Json string the object send
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal string ToJson<TModel>(TModel sender)
        {
            StringBuilder jsonString = new StringBuilder();

            if (sender is not null)
            {
                jsonString.Append(JsonSerializer.Serialize(sender));

                return jsonString.ToString();
            }
            else
                return "null";
        }

        /// <summary>
        /// Convert the object send into UTF-8 encoded Json.
        /// </summary>
        /// <remarks>
        /// Preferred over <see cref="ToJson{TModel}(TModel)"/> for anything crossing into JS: a
        /// byte[] is marshalled straight through as a Uint8Array, so neither side has to build and
        /// re-parse a string for the whole payload. Serializing directly to UTF-8 also skips the
        /// intermediate .NET string entirely.
        /// </remarks>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal byte[] ToBytes<TModel>(TModel sender)
            => sender is null ? null : JsonSerializer.SerializeToUtf8Bytes(sender);
        #endregion

        #region async
        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal Task<string> ToJsonAsync<TModel>(List<TModel> sender)
            => Task.FromResult(ToJson(sender));

        /// <summary>
        /// Convert into UTF-8 encoded Json the model send
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal Task<byte[]> ToBytesAsync<TModel>(List<TModel> sender)
            => Task.FromResult(ToBytes(sender));
        #endregion

    }
}
