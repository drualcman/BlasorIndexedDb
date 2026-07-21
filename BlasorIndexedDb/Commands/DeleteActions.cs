namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Delete commands
    /// </summary>
    internal sealed class DeleteActions
    {
        readonly IJSRuntime JS;
        readonly Settings Setup;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="js"></param>
        /// <param name="setup"></param>
        internal DeleteActions(IJSRuntime js, Settings setup)
        {
            JS = js;
            Setup = setup;
        }


        /// <summary>
        /// Delete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="id">id from the row to delete</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal Task<ResponseJsDb> DbDelete<TModel>([NotNull] int id) => Invoke<TModel>(id);

        /// <summary>
        /// Delete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="id">id from the row to delete</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal Task<ResponseJsDb> DbDelete<TModel>([NotNull] double id) => Invoke<TModel>(id);

        /// <summary>
        /// Delete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="id">id from the row to delete</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal Task<ResponseJsDb> DbDelete<TModel>([NotNull] decimal id) => Invoke<TModel>(id);

        /// <summary>
        /// Delete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="id">id from the row to delete</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal Task<ResponseJsDb> DbDelete<TModel>([NotNull] string id) => Invoke<TModel>(id);

        /// <summary>
        /// Delete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="id">id from the row to delete</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal Task<ResponseJsDb> DbDelete<TModel>([NotNull] DateTime id) => Invoke<TModel>(id);

        /// <summary>
        /// Delete many rows from a table in a single transaction
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <typeparam name="TKey">type of the keypath</typeparam>
        /// <param name="ids">ids from the rows to delete</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal Task<ResponseJsDb> DbDeleteMany<TModel, TKey>([NotNull] IEnumerable<TKey> ids)
        {
            if (ids is null)
                return Task.FromResult(new ResponseJsDb { Result = false, Message = "Ids can't be null" });
            TKey[] keys = ids as TKey[] ?? ids.ToArray();
            if (keys.Length == 0)
                return Task.FromResult(new ResponseJsDb { Result = true, Message = "Nothing to delete" });
            return Invoke<TModel>(keys);
        }

        /// <summary>
        /// Send the delete to JavaScript. The id can be a single value or an array of values, and
        /// an array is deleted in one transaction instead of one call, one connection and one
        /// transaction per row.
        /// </summary>
        private async Task<ResponseJsDb> Invoke<TModel>(object id)
        {
            string table = Setup.Tables.GetTable<TModel>();
            if (Settings.EnableDebug)
                Console.WriteLine($"{Setup.DBName} => DbDelete Model: {table}.");
            try
            {
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                List<ResponseJsDb> result = await jsRuntime.GetBytesResult<List<ResponseJsDb>>("MyDb.Delete", table, id, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                if (result is not null && result.Count > 0)
                    return result[0];
                else
                    return new ResponseJsDb { Result = false, Message = "No results" };
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => DbDelete Model: {table} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => DbDelete Model: {table} Error: {ex}");
                throw new ResponseException(nameof(DbDelete), typeof(TModel).Name, ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove the database from indexed db
        /// </summary>
        /// <returns></returns>
        internal async Task<ResponseJsDb> DropDatabase()
        {
            try
            {
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                ResponseJsDb response = await jsRuntime.GetJsonResult<ResponseJsDb>("MyDb.Drop", Setup.DBName);
                if (Settings.EnableDebug)
                    Console.WriteLine($"DropDatabase: {Setup.DBName} => result {response.Result} with message: {response.Message}");
                return response;
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"Can't drop database: {Setup.DBName} => Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"Can't drop database: {Setup.DBName} => Error: {ex}");
                throw new ResponseException(nameof(DbDelete), Setup.DBName, ex.Message, ex);
            }

        }

    }
}
