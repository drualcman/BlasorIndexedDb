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
            ObjectConverter = new ObjectConverter(setup);
        }

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="data"></param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<ResponseJsDb> DbInsert<TModel>([NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>
            {
                data
            };
            List<ResponseJsDb> response = await DbInsert(rows);
            if (response.Count > 0) return response[0];
            else return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<List<ResponseJsDb>> DbInsert<TModel>([NotNull] List<TModel> rows)
        {
            try
            {
                IJSObjectReference js = await InitializeDatabase.GetIJSObjectReference(JS, Setup);
                Commands Commands = new Commands(js, Setup);
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                int c = rows.Count;
                if (c > 0)
                {
                    List<ResponseJsDb> response = await Commands.DbCommand(DbCommands.Insert, Setup.Tables.GetTable<TModel>(), await ObjectConverter.ToJsonAsync(rows));
                    if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => Insert response is null {response == null}");
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
                    if (allGood)
                    {
                        //need check the object received and do the action into the other tables if have
                        c = rows.Count;
                        for (i = 0; i < c; i++)
                        {
                            Type t = rows[i].GetType();

                            //read all properties
                            PropertyInfo[] properties = ObjectConverter.GetProperties(t);
                            int p = properties.Length;
                            for (int a = 0; a < p; a++)
                            {
                                if (Setup.Tables.InTables(properties[a].Name))
                                {
                                    if (ObjectConverter.IsGenericList(properties[a].GetValue(rows[i])))
                                    {
                                        Console.WriteLine($"{Setup.DBName} => esto es una lista que tendremos que recorrer");
                                    }
                                    else
                                    {
                                        if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => Add to the store {properties[a].PropertyType.Name} the value from the property {properties[a].Name}");
                                        //is single object add the data to the corresponding store
                                        result.AddRange(await Commands.DbCommand(DbCommands.Insert, properties[a].PropertyType.Name, await ObjectConverter.ToJsonAsync(properties[a].GetValue(rows[i]))));
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbInsert No need insert into {Setup.Tables.GetTable<TModel>()}");
                        result = new List<ResponseJsDb>{
                            new ResponseJsDb { Result = true, Message = $"{Setup.DBName} => No need insert into {Setup.Tables.GetTable<TModel>()}!" }
                        };
                }
                return result;
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbInsert Model: {Setup.Tables.GetTable<TModel>()} ResponseException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbInsert Model: {Setup.Tables.GetTable<TModel>()} Exception: {ex.Message}");
                throw new ResponseException(nameof(DbInsert), typeof(TModel).Name, ex.Message, ex);
            }

        }

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="data">data to insert</param>
        /// <returns></returns>
        internal async ValueTask<ResponseJsDb> DbInserOffline<TModel>([NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>();
            rows.Add(data);
            List<ResponseJsDb> response = await DbInserOffline(rows);
            if (response.Count > 0) return response[0];
            else return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Insert int a table to a db with off-line property
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<List<ResponseJsDb>> DbInserOffline<TModel>([NotNull] List<TModel> rows)
        {
            try
            {
                List<dynamic> expanded = await AddOflineProperty.AddOfflineAsync(rows);
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbInserOffline in model: {Setup.Tables.GetTable<TModel>()}");
                return await DbInsert(expanded);
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbInsert Model: {Setup.Tables.GetTable<TModel>()} Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbInserOffline), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
            }
        }

    }
}
