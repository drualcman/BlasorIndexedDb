using BlazorIndexedDb.Extensions;

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
        internal async Task<ResponseJsDb> DbDelete<TModel>([NotNull] int id)
        {
            if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()}.");
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                result.AddRange(await jsRuntime.GetJsonResult<List<ResponseJsDb>>($"MyDb.Delete", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson));
                if (result.Count > 0) return result[0];
                else return new ResponseJsDb { Result = false, Message = "No results" };
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
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
        internal async Task<ResponseJsDb> DbDelete<TModel>([NotNull] double id)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                result.AddRange(await jsRuntime.GetJsonResult<List<ResponseJsDb>>($"MyDb.Delete", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson));
                if (result.Count > 0) return result[0];
                else return new ResponseJsDb { Result = false, Message = "No results" };

            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
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
        internal async Task<ResponseJsDb> DbDelete<TModel>([NotNull] decimal id)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                result.AddRange(await jsRuntime.GetJsonResult<List<ResponseJsDb>>($"MyDb.Delete", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson));
                if (result.Count > 0) return result[0];
                else return new ResponseJsDb { Result = false, Message = "No results" };
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
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
        internal async Task<ResponseJsDb> DbDelete<TModel>([NotNull] string id)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                result.AddRange(await jsRuntime.GetJsonResult<List<ResponseJsDb>>($"MyDb.Delete", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson));
                if (result.Count > 0) return result[0];
                else return new ResponseJsDb { Result = false, Message = "No results" };
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
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
        internal async Task<ResponseJsDb> DbDelete<TModel>([NotNull] DateTime id)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                result.AddRange(await jsRuntime.GetJsonResult<List<ResponseJsDb>>($"MyDb.Delete", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson));
                if (result.Count > 0) return result[0];
                else return new ResponseJsDb { Result = false, Message = "No results" };
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbDelete Model: {Setup.Tables.GetTable<TModel>()} Error: {ex}");
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
                if (Settings.EnableDebug) Console.WriteLine($"DropDatabase: {Setup.DBName} => result {response.Result} with message: {response.Message}");
                return response;
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"Can't drop database: {Setup.DBName} => Error: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"Can't drop database: {Setup.DBName} => Error: {ex}");
                throw new ResponseException(nameof(DbDelete), Setup.DBName, ex.Message, ex);
            }

        }

    }
}
