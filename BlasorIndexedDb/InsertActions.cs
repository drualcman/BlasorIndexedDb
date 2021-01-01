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
        /// Extent JSRuntime to get a confirm
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="name">database name</param>
        /// <param name="version">database version</param>
        /// <param name="tables">string array with the names of the model classes to serialize</param>
        /// <param name="mynamespace">namespace from the models class</param>
        /// <returns></returns>
        public static async ValueTask<bool> DbInsert<T>(this IJSRuntime jsRuntime, List<T> table)
        {
            bool result;
            try
            {
                string data = JsonSerializer.Serialize(table);
                Console.WriteLine(data);
                result = await jsRuntime.InvokeAsync<bool>("MyDb.Insert", Utils.GetName<T>(), data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result = false;
            }
            return result;
        }

    }
}
