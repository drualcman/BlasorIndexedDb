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
        public async ValueTask<List<TModel>> DbSelect<TModel>()
        {
            List<TModel> data;

            try
            {
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS);
                data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.Select", Setup.Tables.GetTable<TModel>(), Setup.DBName, Setup.Version, Setup.ModelsAsJson);
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => SelectActions exception: {ex}");
                throw new ResponseException(nameof(DbSelect), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
                //return null;
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
        public async ValueTask<List<TModel>> DbSelect<TModel>([NotNull] string column, [NotNull] object value)
        {
            List<TModel> data;

            try
            {
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS);
                data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectWhere", Setup.Tables.GetTable<TModel>(), column, value, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => SelectActions exception: {ex}");
                throw new ResponseException(nameof(DbSelect), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
                //return null;
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
        public async ValueTask<TModel> SingleRecord<TModel>([NotNull] object id) where TModel : class
        {
            try
            {
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS);
                List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                if (data is not null && data.Any())
                    return data[0];
                else
                    return null;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => SelectActions exception: {ex}");
                return null;
            }
        }
        #endregion

    }
}
