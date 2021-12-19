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
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbUpdate<TModel>(this IJSRuntime jsRuntime, [NotNull] TModel data)
        {
            List<TModel> rows = new List<TModel>();
            rows.Add(data);
            List<ResponseJsDb> response = await DbUpdate(jsRuntime, rows);
            if (response.Count > 0) return response[0];
            else return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Update table to a db
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbUpdate<TModel>(this IJSRuntime jsRuntime, [NotNull] List<TModel> rows)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                if (Settings.Initialized)
                {
                    int c = rows.Count;
                    if (c > 0)
                    {
                        result.AddRange(await Commands.DbCommand(jsRuntime, DbCommands.Update, Settings.Tables.GetTable<TModel>(), await ObjectConverter.ToJsonAsync(rows)));                        
                    }
                    else
                    {
                        if (Settings.EnableDebug) Console.WriteLine($"DbUpdate No need update into {Settings.Tables.GetTable<TModel>()}");
                            result = new List<ResponseJsDb>{
                            new ResponseJsDb { Result = true, Message =$"No need update into {Settings.Tables.GetTable<TModel>()}!" }
                        };
                    }
                }
                else
                {
                    if (Settings.EnableDebug) Console.WriteLine($"UpdateActions: IndexedDb not initialized yet!");
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
                if (Settings.EnableDebug) Console.WriteLine($"DbUpdate Model: {ex.StoreName} Error: {ex.Message} PayLoad: {ex.TransactionData}");
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbUpdate), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
            }
            
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
            if (response.Count > 0) return response[0];
            else return new ResponseJsDb { Result = false, Message = "No results" };
        }

        /// <summary>
        /// Update table to a db with offline property
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="rows">data to insert</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbUpdateOffLine<TModel>(this IJSRuntime jsRuntime, [NotNull] List<TModel> rows)
        {
            try
            {
                List<dynamic> expanded = await AddOflineProperty.AddOfflineAsync(rows);
                if (Settings.EnableDebug) Console.WriteLine($"DbUpdateOffLine in modelL  = {Settings.Tables.GetTable<TModel>()}");
                return await DbUpdate(jsRuntime, expanded);
            }

            catch (ResponseException ex)
            {
                if (Settings.EnableDebug) Console.WriteLine($"DbUpdateOffLine Model: {ex.StoreName} Error: {ex.Message} PayLoad: {ex.TransactionData}");
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbUpdateOffLine), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
            }
            
        }
    }
}
