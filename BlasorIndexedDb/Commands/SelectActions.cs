namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Select commands
    /// </summary>
    public class SelectActions
    {
        readonly IJSRuntime JS;
        readonly Settings Setup;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="js"></param>
        /// <param name="setup"></param>
        public SelectActions(IJSRuntime js, Settings setup)
        {
            JS = js;
            Setup = setup;
        }

        #region lists
        /// <summary>
        /// Get all resister from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async Task<List<TModel>> DbSelect<TModel>()
        {
            List<TModel> data = new List<TModel>();
            try
            {
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                byte[] dataBytes = await jsRuntime.InvokeAsync<byte[]>("MyDb.Select", Setup.Tables.GetTable<TModel>(), Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                data = DeserializeData<TModel>(dataBytes);
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => SelectActions exception: {ex}");
                throw new ResponseException(nameof(DbSelect), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
            }
            return data;
        }

        /// <summary>
        /// Get all resister from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="column">column to compare</param>
        /// <param name="value">value to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async Task<List<TModel>> DbSelect<TModel>([NotNull] string column, [NotNull] object value)
        {
            List<TModel> data;
            try
            {
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                // Was GetJsonResult<byte[]>, unlike every other Select here. That route reads the
                // response as a JsonElement first, and a Uint8Array arrives there as an array of
                // numbers while System.Text.Json expects base64 for a byte[] — so it could not
                // deserialize what JS actually sent. InvokeAsync<byte[]> takes the bytes straight.
                byte[] dataBytes = await jsRuntime.InvokeAsync<byte[]>("MyDb.SelectWhere", Setup.Tables.GetTable<TModel>(), column, value, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                data = DeserializeData<TModel>(dataBytes);
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => SelectActions exception: {ex}");
                throw new ResponseException(nameof(DbSelect), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
            }
            return data;
        }
        #endregion

        #region single

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async Task<TModel> SingleRecord<TModel>([NotNull] object id) where TModel : class
        {
            try
            {
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                byte[] dataBytes = await jsRuntime.InvokeAsync<byte[]>("MyDb.SelectId", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                List<TModel> data = DeserializeData<TModel>(dataBytes);
                if (data is not null && data.Any())
                    return data[0];
                else
                    return null;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => SelectActions exception: {ex}");
                return null;
            }
        }
        #endregion 

        private static List<TModel> DeserializeData<TModel>(byte[] dataBytes)
        {
            if (dataBytes is null || dataBytes.Length == 0)
                return new List<TModel>();
            // Shared options (see JsRuntimeExtensions): a second copy here had already drifted from
            // the one used elsewhere, and building a fresh JsonSerializerOptions per call also
            // defeats System.Text.Json's metadata cache.
            return JsonSerializer.Deserialize<List<TModel>>(dataBytes, JsRuntimeExtensions.InteropOptions);
        }

    }
}
