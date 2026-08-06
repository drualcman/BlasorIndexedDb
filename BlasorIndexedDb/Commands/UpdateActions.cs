namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Update commands
    /// </summary>
    internal sealed class UpdateActions
    {
        readonly Settings Setup;
        readonly IJSRuntime JS;
        readonly ObjectConverter ObjectConverter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="js"></param>
        /// <param name="setup"></param>
        internal UpdateActions(IJSRuntime js, Settings setup)
        {
            Setup = setup;
            JS = js;
            ObjectConverter = new ObjectConverter();
        }

        /// <summary>
        /// Update table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="data">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async Task<ResponseJsDb> DbUpdate<TModel>([NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>
            {
                data
            };
            List<ResponseJsDb> response = await DbUpdate(rows);
            if (response.Count > 0)
                return response[0];
            else
                return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Update table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal Task<List<ResponseJsDb>> DbUpdate<TModel>([NotNull] List<TModel> rows) =>
            DbUpdate(Setup.Tables.GetTable<TModel>(), rows);

        /// <summary>
        /// Update table to a db
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
        internal async Task<List<ResponseJsDb>> DbUpdate<TModel>([NotNull] string storeName, [NotNull] List<TModel> rows)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();

                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                Commands Commands = new Commands(jsRuntime, Setup);
                int c = rows.Count;
                if (c > 0)
                {
                    List<ResponseJsDb> response = await Commands.DbCommand(DbCommands.Update, storeName, await ObjectConverter.ToBytesAsync(rows));
                    if (Settings.EnableDebug)
                        Console.WriteLine($"{Setup.DBName} => Update response is null {response == null}");
                    if (response != null)
                        result.AddRange(response);
                }
                else
                {
                    if (Settings.EnableDebug)
                        Console.WriteLine($"{Setup.DBName} => DbUpdate No need update into {storeName}");
                    result = new List<ResponseJsDb>{
                            new ResponseJsDb { Result = true, Message =$"No need update into {storeName}!" }
                        };
                }
                return result;
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => DbUpdate Model: {ex.StoreName} Error: {ex.Message} PayLoad: {ex.TransactionData}");
                throw;
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbUpdate), storeName, ex.Message, ex);
            }

        }


        /// <summary>
        /// Update table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="data">data to insert</param>
        /// <returns></returns>
        internal async Task<ResponseJsDb> DbUpdateOffLine<TModel>([NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>
            {
                data
            };
            List<ResponseJsDb> response = await DbUpdateOffLine(rows);
            if (response.Count > 0)
                return response[0];
            else
                return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Update table to a db with offline property
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async Task<List<ResponseJsDb>> DbUpdateOffLine<TModel>([NotNull] List<TModel> rows)
        {
            // Resolved from TModel, which is the only place it can be read from: the expanded rows
            // below are ExpandoObjects and no longer name any store.
            string storeName = Setup.Tables.GetTable<TModel>();
            try
            {
                List<dynamic> expanded = await AddOflineProperty.AddOfflineAsync(rows);
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => DbUpdateOffLine in modelL  = {storeName}");
                return await DbUpdate(storeName, expanded);
            }

            catch (ResponseException ex)
            {
                if (Settings.EnableDebug)
                    Console.WriteLine($"{Setup.DBName} => DbUpdateOffLine Model: {ex.StoreName} Error: {ex.Message} PayLoad: {ex.TransactionData}");
                throw;
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbUpdateOffLine), storeName, ex.Message, ex);
            }

        }
    }
}
