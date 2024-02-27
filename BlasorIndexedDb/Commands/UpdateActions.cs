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
            ObjectConverter = new ObjectConverter(setup);
        }

        /// <summary>
        /// Update table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="data">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<ResponseJsDb> DbUpdate<TModel>([NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>();
            rows.Add(data);
            List<ResponseJsDb> response = await DbUpdate(rows);
            if (response.Count > 0) return response[0];
            else return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Update table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<List<ResponseJsDb>> DbUpdate<TModel>([NotNull] List<TModel> rows)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();

                IJSObjectReference jsRuntime = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                Commands Commands = new Commands(jsRuntime, Setup);
                int c = rows.Count;
                if (c > 0)
                {
                    List<ResponseJsDb> response = await Commands.DbCommand(DbCommands.Update, Setup.Tables.GetTable<TModel>(), await ObjectConverter.ToJsonAsync(rows));
                    if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => Update response is null {response == null}");
                    if (response != null) result.AddRange(response);
                }
                else
                {
                    if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbUpdate No need update into {Setup.Tables.GetTable<TModel>()}");
                    result = new List<ResponseJsDb>{
                            new ResponseJsDb { Result = true, Message =$"No need update into {Setup.Tables.GetTable<TModel>()}!" }
                        };
                }
                return result;
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbUpdate Model: {ex.StoreName} Error: {ex.Message} PayLoad: {ex.TransactionData}");
                throw;
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbUpdate), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
            }

        }


        /// <summary>
        /// Update table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="data">data to insert</param>
        /// <returns></returns>
        internal async ValueTask<ResponseJsDb> DbUpdateOffLine<TModel>([NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>();
            rows.Add(data);
            List<ResponseJsDb> response = await DbUpdateOffLine(rows);
            if (response.Count > 0) return response[0];
            else return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Update table to a db with offline property
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<List<ResponseJsDb>> DbUpdateOffLine<TModel>([NotNull] List<TModel> rows)
        {
            try
            {
                List<dynamic> expanded = await AddOflineProperty.AddOfflineAsync(rows);
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbUpdateOffLine in modelL  = {Setup.Tables.GetTable<TModel>()}");
                return await DbUpdate(expanded);
            }

            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbUpdateOffLine Model: {ex.StoreName} Error: {ex.Message} PayLoad: {ex.TransactionData}");
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbUpdateOffLine), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
            }

        }
    }
}
