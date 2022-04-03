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
    public class TableActions
    {
        readonly IJSObjectReference jsRuntime;
        readonly Settings Setup;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="js"></param>
        /// <param name="setup"></param>
        public TableActions(IJSObjectReference js, Settings setup)
        {
            jsRuntime = js;
            Setup = setup;
        }

        /// <summary>
        /// Delete all rows from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public  async ValueTask<ResponseJsDb> DbCleanTable<TModel>()
        {
            return await DbCleanTable(Setup.Tables.GetTable<TModel>());
        }

        /// <summary>
        /// Delete all rows from a table
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async ValueTask<ResponseJsDb> DbCleanTable([NotNull] string name)
        {
            try
            {
                List<ResponseJsDb> result = new List<ResponseJsDb>();
                if (Setup.Initialized)
                {
                    result.AddRange(await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.Clean", name, Setup.DBName, Setup.Version, Setup.Tables.GetModel(name)));
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
