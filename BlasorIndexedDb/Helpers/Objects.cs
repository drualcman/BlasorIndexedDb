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
        #endregion

    }
}
