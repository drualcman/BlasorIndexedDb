using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Helpers;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
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
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbCleanTable<TModel>(this IJSRuntime jsRuntime)
        {
            return await DbCleanTable(jsRuntime, Utils.GetName<TModel>());
        }

        /// <summary>
        /// Delete all rows from a table
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbCleanTable(this IJSRuntime jsRuntime, [NotNull] string name)
        {
            List<ResponseJsDb> result = new List<ResponseJsDb>();
            if (Settings.Initialized)
            {
                try
                {
                    result.AddRange(await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.Clean", name));
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

    }
}
