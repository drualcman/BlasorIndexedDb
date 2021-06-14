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
    /// Update commands
    /// </summary>
    public static class UpdateActions
    {
        /// <summary>
        /// Update table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="data">data to insert</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbUpdate<TModel>(this IJSRuntime jsRuntime, [NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>();
            rows.Add(data);
            List<ResponseJsDb> response = await DbUpdate(jsRuntime, rows);
            return response[0];
        }

        /// <summary>
        /// Update table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="rows">data to insert</param>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbUpdate<TModel>(this IJSRuntime jsRuntime, [NotNull] List<TModel> rows)
        {
            List<ResponseJsDb> result;
            if (Settings.Initiallezed)
            {
                if (rows.Count > 0)
                {
                    try
                    {
                        string data = await ObjectConverter.ToJsonAsync(rows);
                        if (Settings.EnableDebug) Console.WriteLine($"DbUpdate data = {data}");
                        result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Update", Utils.GetName<TModel>(), data);
                    }
                    catch (Exception ex)
                    {
                        if (Settings.EnableDebug) Console.WriteLine($"DbUpdate Error: {ex}");
                        result = new List<ResponseJsDb>{
                        new ResponseJsDb { Result = false, Message = ex.Message }
                    };
                    }
                }
                else result = new List<ResponseJsDb>{
                        new ResponseJsDb { Result = true, Message = "No need update!" }
                    };
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"UpdateActions: IndexedDb not initiallized yet!");
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
        /// Update table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="data">data to insert</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbUpdateOffLine<TModel>(this IJSRuntime jsRuntime, [NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>();
            rows.Add(data);
            List<ResponseJsDb> response = await DbUpdateOffLine(jsRuntime, rows);
            return response[0];
        }

        /// <summary>
        /// Update table to a db with offline property
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="rows">data to insert</param>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbUpdateOffLine<TModel>(this IJSRuntime jsRuntime, [NotNull] List<TModel> rows)
        {
            List<dynamic> expanded = await AddOflineProperty.AddOfflineAsync(rows);
            if (Settings.EnableDebug) Console.WriteLine($"DbUpdateOffLine in modelL  = {Utils.GetName<TModel>()}");            
            return await DbUpdate(jsRuntime, expanded);
        }
    }
}
