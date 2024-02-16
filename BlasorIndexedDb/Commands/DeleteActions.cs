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
        /// <param name="id"></param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<ResponseJsDb> DbDelete<TModel>([NotNull] int id)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS);
                result.AddRange(await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.Delete", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson));
                if (result.Count > 0) return result[0];
                else return new ResponseJsDb { Result = false, Message = "No results" };
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw new ResponseException(nameof(DbDelete), typeof(TModel).Name, ex.Message, ex);
            }

        }

        /// <summary>
        /// Delete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="id">id from the row to delete</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<ResponseJsDb> DbDelete<TModel>([NotNull] double id)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS);
                result.AddRange(await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.Delete", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson));
                if (result.Count > 0) return result[0];
                else return new ResponseJsDb { Result = false, Message = "No results" };

            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw new ResponseException(nameof(DbDelete), typeof(TModel).Name, ex.Message, ex);
            }

        }


        /// <summary>
        /// Delete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="id">id from the row to delete</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<ResponseJsDb> DbDelete<TModel>([NotNull] decimal id)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS);
                result.AddRange(await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.Delete", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson));
                if (result.Count > 0) return result[0];
                else return new ResponseJsDb { Result = false, Message = "No results" };
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw new ResponseException(nameof(DbDelete), typeof(TModel).Name, ex.Message, ex);
            }

        }


        /// <summary>
        /// Delete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="id">id from the row to delete</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<ResponseJsDb> DbDelete<TModel>([NotNull] string id)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS);
                result.AddRange(await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.Delete", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson));
                if (result.Count > 0) return result[0];
                else return new ResponseJsDb { Result = false, Message = "No results" };
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw new ResponseException(nameof(DbDelete), typeof(TModel).Name, ex.Message, ex);
            }

        }

        /// <summary>
        /// Delete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="id">id from the row to delete</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<ResponseJsDb> DbDelete<TModel>([NotNull] DateTime id)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS);
                result.AddRange(await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.Delete", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson));
                if (result.Count > 0) return result[0];
                else return new ResponseJsDb { Result = false, Message = "No results" };
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw new ResponseException(nameof(DbDelete), typeof(TModel).Name, ex.Message, ex);
            }

        }
    }
}
