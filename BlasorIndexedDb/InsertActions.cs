using BlasorIndexedDb.Helpers;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlasorIndexedDb
{
    public static class InsertActions
    {
        /// <summary>
        /// Insert table to a db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="table">database name</param>
        /// <returns></returns>
        public static async ValueTask<bool> DbInsert<T>(this IJSRuntime jsRuntime, List<T> table)
        {
            bool result;
            try
            {
                string data = JsonSerializer.Serialize(table);
                result = await jsRuntime.InvokeAsync<bool>("MyDb.Insert", Utils.GetName<T>(), data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DbInsert Error: {ex.Message}");
                result = false;
            }
            return result;
        }

    }
}
