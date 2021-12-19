using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Manage tables
    /// </summary>
    public static class TableActions
    {
        /// <summary>
        /// Delete all rows from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbCleanTable<TModel>(this IJSRuntime jsRuntime)
        {
            return await DbCleanTable(jsRuntime, Settings.Tables.GetTable<TModel>());
        }

        /// <summary>
        /// Delete all rows from a table
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="name"></param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbCleanTable(this IJSRuntime jsRuntime, [NotNull] string name)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                if (Settings.Initialized)
                {
                    result.AddRange(await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.Clean", name));
                    try
                    {
                    }
                    catch (Exception ex)
                    {
                        if (Settings.EnableDebug) Console.WriteLine($"DbCleanTable: {name} Error: {ex}");
                        result = new List<ResponseJsDb>();
                        result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
                    }
                }
                else
                {
                    if (Settings.EnableDebug) Console.WriteLine($"TableActions: IndexedDb not initialized yet!");
                    result = new List<ResponseJsDb>();
                    result.Add(new ResponseJsDb { Result = false, Message = $"IndexedDb not initialized yet!" });
                }
                return result[0];
            }
            catch (Exception ex)
            {
                throw new ResponseException(nameof(DbCleanTable), name, ex.Message, ex);
            }

        }

    }
}
