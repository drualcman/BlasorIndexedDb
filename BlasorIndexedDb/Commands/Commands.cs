using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
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
    public class Commands
    {
        readonly IJSObjectReference jsRuntime;
        readonly Settings Setup;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="js"></param>
        /// <param name="setup"></param>
        public Commands(IJSObjectReference js, Settings setup)
        {
            jsRuntime = js;
            Setup = setup;
        }

        /// <summary>
        /// Execute a command directly sending a json string
        /// </summary>
        /// <param name="command"></param>
        /// <param name="storeName"></param>
        /// <param name="data"></param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async ValueTask<List<ResponseJsDb>> DbCommand(DbCommands command, string storeName, string data)
        {
            if (Settings.EnableDebug) Console.WriteLine($"{command} store = {storeName}, data = {data}");
            if (string.IsNullOrEmpty(storeName)) throw new ResponseException(command.ToString(), "StoreName can't be null", data);
            else if (string.IsNullOrEmpty(data)) throw new ResponseException(command.ToString(), storeName, "Data can't be null");
            else
            {
                try
                {
                    return await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.{command}", storeName, data, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                }
                catch (Exception ex)
                {
                    if (Settings.EnableDebug) Console.WriteLine($"Exception: {ex.Message}");
                    throw new ResponseException(command.ToString(), storeName, data, ex);
                }
            }
        }

        /// <summary>
        /// Get is we are connected to a indexed db
        /// </summary>
        /// <returns></returns>
        public async ValueTask<string> DbConnected()
        {
            string message;
            if (Setup.Initialized)
            {
                message = await jsRuntime.InvokeAsync<string>("MyDb.Connected", Setup.DBName, Setup.Version);
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
