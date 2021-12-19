using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Helpers;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Insert commands
    /// </summary>
    public static class InsertActions
    {
        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="data"></param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbInsert<TModel>(this IJSRuntime jsRuntime, [NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>();
            rows.Add(data);
            List<ResponseJsDb> response = await DbInsert(jsRuntime, rows);
            if (response.Count > 0) return response[0];
            else return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbInsert<TModel>(this IJSRuntime jsRuntime, [NotNull] List<TModel> rows)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                if (Settings.Initialized)
                {
                    int c = rows.Count;
                    if (c > 0)
                    {
                        result.AddRange(await Commands.DbCommand(jsRuntime, DbCommands.Insert, Settings.Tables.GetTable<TModel>(), await ObjectConverter.ToJsonAsync(rows)));

                        bool allGood = false;
                        c = result.Count;

                        int i = 0;
                        while (c > 0 && !allGood && i < c)
                        {
                            allGood = result[i].Result;
                            i++;
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
                                    if (Settings.Tables.InTables(t.Name))
                                    {
                                        if (ObjectConverter.IsGenericList(properties[a].GetValue(rows[i])))
                                        {
                                            Console.WriteLine("esto es una lista que tendremos que recorrer");
                                        }
                                        else
                                        {
                                            if (Settings.EnableDebug) Console.WriteLine($"Add to the store {properties[a].PropertyType.Name} the value from the property {properties[a].Name}");
                                            //is single object add the data to the corresponding store
                                            result.AddRange(await Commands.DbCommand(jsRuntime, DbCommands.Insert, properties[a].PropertyType.Name, await ObjectConverter.ToJsonAsync(properties[a].GetValue(rows[i]))));
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        if (Settings.EnableDebug) Console.WriteLine($"DbInsert No need insert into {Settings.Tables.GetTable<TModel>()}");
                        result = new List<ResponseJsDb>{
                            new ResponseJsDb { Result = true, Message = $"No need insert into {Settings.Tables.GetTable<TModel>()}!" }
                        };
                    }
                }
                else
                {
                    if (Settings.EnableDebug) Console.WriteLine($"InsertActions: IndexedDb not initialized yet!");
                    result = new List<ResponseJsDb>()
                    {
                        new ResponseJsDb()
                        {
                             Message = $"IndexedDb not initialized yet!",
                             Result = false
                        }
                    };
                }
                return result;
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbInsert Model: {Settings.Tables.GetTable<TModel>()} Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbInsert), typeof(TModel).Name, ex.Message, ex);
            }

        }

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="data">data to insert</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbInserOffline<TModel>(this IJSRuntime jsRuntime, [NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>();
            rows.Add(data);
            List<ResponseJsDb> response = await DbInserOffline(jsRuntime, rows);
            if (response.Count > 0) return response[0];
            else return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Insert int a table to a db with off-line property
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbInserOffline<TModel>(this IJSRuntime jsRuntime, [NotNull] List<TModel> rows)
        {
            try
            {
                List<dynamic> expanded = await AddOflineProperty.AddOfflineAsync(rows);
                if (Settings.EnableDebug) Console.WriteLine($"DbInserOffline in model: {Settings.Tables.GetTable<TModel>()}");
                return await DbInsert(jsRuntime, expanded);
            }
            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbInsert Model: {Settings.Tables.GetTable<TModel>()} Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbInserOffline), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
            }
        }

    }
}
