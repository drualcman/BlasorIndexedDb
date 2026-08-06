namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Insert commands
    /// </summary>
    internal sealed class InsertActions
    {
        readonly Settings Setup;
        readonly IJSRuntime JS;
        readonly ObjectConverter ObjectConverter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="js"></param>
        /// <param name="setup"></param>
        internal InsertActions(IJSRuntime js, Settings setup)
        {
            Setup = setup;
            JS = js;
            ObjectConverter = new ObjectConverter();
        }

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="data"></param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async Task<ResponseJsDb> DbInsert<TModel>([NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>
            {
                data
            };
            List<ResponseJsDb> response = await DbInsert(rows);
            if (response.Count > 0)
                return response[0];
            else
                return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal Task<List<ResponseJsDb>> DbInsert<TModel>([NotNull] List<TModel> rows) =>
            DbInsert(Setup.Tables.GetTable<TModel>(), rows);

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <remarks>
        /// The store is named by the caller instead of being resolved again from the rows. The
        /// offline write turns every row into an ExpandoObject, so resolving it here looked up a
        /// store for "Object", found none, and the write failed with "StoreName can't be null"
        /// before it ever reached the database.
        /// </remarks>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="storeName">store the rows belong to</param>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async Task<List<ResponseJsDb>> DbInsert<TModel>([NotNull] string storeName, [NotNull] List<TModel> rows)
        {
            try
            {
                IJSObjectReference js = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                Commands Commands = new Commands(js, Setup);
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                int c = rows.Count;
                if (c > 0)
                {
                    List<ResponseJsDb> response = await Commands.DbCommand(DbCommands.Insert, storeName, await ObjectConverter.ToBytesAsync(rows));
                    if (Settings.EnableDebug)
                        Console.WriteLine($"{Setup.DBName} => Insert response is null {response == null}");
                    bool allGood = false;
                    int i = 0;
                    if (response != null)
                    {
                        result.AddRange(response);
                        c = result.Count;
                        while (c > 0 && !allGood && i < c)
                        {
                            allGood = result[i].Result;
                            i++;
                        }
                    }

                }
                else
                {
                    if (Settings.EnableDebug)
                        Console.WriteLine($"{Setup.DBName} => DbInsert No need insert into {storeName}");
                    result = new List<ResponseJsDb>{
                            new ResponseJsDb { Result = true, Message = $"{Setup.DBName} => No need insert into {storeName}!" }
                        };
                }
                return result;
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => DbInsert Model: {storeName} ResponseException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => DbInsert Model: {storeName} Exception: {ex.Message}");
                throw new ResponseException(nameof(DbInsert), storeName, ex.Message, ex);
            }

        }

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="data">data to insert</param>
        /// <returns></returns>
        internal async Task<ResponseJsDb> DbInserOffline<TModel>([NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>
            {
                data
            };
            List<ResponseJsDb> response = await DbInserOffline(rows);
            if (response.Count > 0)
                return response[0];
            else
                return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Insert int a table to a db with off-line property
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async Task<List<ResponseJsDb>> DbInserOffline<TModel>([NotNull] List<TModel> rows)
        {
            // Resolved from TModel, which is the only place it can be read from: the expanded rows
            // below are ExpandoObjects and no longer name any store.
            string storeName = Setup.Tables.GetTable<TModel>();
            try
            {
                List<dynamic> expanded = await AddOflineProperty.AddOfflineAsync(rows);
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => DbInserOffline in model: {storeName}");
                return await DbInsert(storeName, expanded);
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => DbInsert Model: {storeName} Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbInserOffline), storeName, ex.Message, ex);
            }
        }

    }
}
