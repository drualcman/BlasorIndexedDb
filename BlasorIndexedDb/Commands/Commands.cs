using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Database Commands to use
    /// </summary>
    public enum DbCommands
    {
        /// <summary>
        /// Insert array data
        /// </summary>
        Insert,
        /// <summary>
        /// Update array data
        /// </summary>
        Update,
        /// <summary>
        /// Delete single row from id send
        /// </summary>
        Delete,
    }

    /// <summary>
    /// Other commands to execute
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// Execute a command directly sending a json string
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="command"></param>
        /// <param name="storeName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbCommand(this IJSRuntime jsRuntime,
            DbCommands command, string storeName, string data)
        {            
            if (Settings.EnableDebug) Console.WriteLine($"DbInsert store = {storeName}, data = {data}");
            if (string.IsNullOrEmpty(storeName)) throw new ResponseException(command.ToString(), "StoreName can't be null", data);
            else if (string.IsNullOrEmpty(data)) throw new ResponseException(command.ToString(), storeName, "Data can't be null");
            else
            {
                try
                {
                    return await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.{command}", storeName, data);
                }
                catch (Exception ex)
                {
                    throw new ResponseException(command.ToString(), storeName, data, ex);
                }
            }
        }

        /// <summary>
        /// Get is we are connected to a indexed db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <returns></returns>
        public static async ValueTask<string> DbConnected(this IJSRuntime jsRuntime)
        {
            string message;
            if (Settings.Initialized)
            {
                message = await jsRuntime.InvokeAsync<string>("MyDb.Connected");
            }
            else
            {
                message = $"Commands: IndexedDb not initialized yet!";
                if (Settings.EnableDebug) Console.WriteLine(message);
            }
            return message;
        }
    }
}
