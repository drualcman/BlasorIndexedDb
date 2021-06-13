using BlazorIndexedDb.Configuration;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Other commands to execute
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// Get is we are connected to a indexed db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <returns></returns>
        public static async ValueTask<string> DbConnected(this IJSRuntime jsRuntime)
        {
            string message;
            if (Settings.Initiallezed)
            {
                message = await jsRuntime.InvokeAsync<string>("MyDb.Connected");
            }
            else
            {
                message = $"Commands: IndexedDb not initiallized yet!";
                if (Settings.EnableDebug) Console.WriteLine(message);
            }
            return message;
        }
    }
}
