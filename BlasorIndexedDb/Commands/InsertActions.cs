using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Helpers;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbInsert<TModel>(this IJSRuntime jsRuntime, [NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>();
            rows.Add(data);
            List<ResponseJsDb> response = await DbInsert(jsRuntime, rows);
            return response[0];
        }

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="rows">data to insert</param>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbInsert<TModel>(this IJSRuntime jsRuntime, [NotNull] List<TModel> rows)
        {
            List<ResponseJsDb> result = new List<ResponseJsDb>();
            if (Settings.Initiallezed)
            {
                int c = rows.Count;
                if (c > 0)
                {
                    try
                    {                        
                        result.AddRange(await Commands.DbCommand(jsRuntime, DbCommands.Insert, Utils.GetName<TModel>(), await ObjectConverter.ToJsonAsync(rows)));
                    }
                    catch (Exception ex)
                    {
                        if (Settings.EnableDebug) Console.WriteLine($"DbInsert Model: {Utils.GetName<TModel>()} Error: {ex}");
                        result = new List<ResponseJsDb>{
                            new ResponseJsDb { Result = false, Message = ex.Message }
                        };
                    }
                }
                else
                {
                    if (Settings.EnableDebug) Console.WriteLine($"DbInsert No need insert into {Utils.GetName<TModel>()}");
                    result = new List<ResponseJsDb>{
                        new ResponseJsDb { Result = true, Message = $"No need insert into {Utils.GetName<TModel>()}!" }
                    };
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"InsertActions: IndexedDb not initiallized yet!");
                result = new List<ResponseJsDb>()
                {
                    new ResponseJsDb()
                    {
                         Message = $"IndexedDb not initiallized yet!",
                         Result = false
                    }
                };
            }
            return result;
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
            return response[0];
        }

        /// <summary>
        /// Insert int a table to a db with offline property
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="rows">data to insert</param>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbInserOffline<TModel>(this IJSRuntime jsRuntime, [NotNull] List<TModel> rows)
        {
            List<dynamic> expanded = await AddOflineProperty.AddOfflineAsync(rows);
            if (Settings.EnableDebug) Console.WriteLine($"DbInserOffline in model: {Utils.GetName<TModel>()}");
            return await DbInsert(jsRuntime, expanded);
        }

    }
}
