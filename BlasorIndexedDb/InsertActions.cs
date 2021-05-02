using BlazorIndexedDb.Helpers;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorIndexedDb
{
    public static class InsertActions
    {
        /// Insert into a table to a db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="data">data to insert</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbInsert<T>(this IJSRuntime jsRuntime, T data)
        {
            List<T> rows = new List<T>();
            rows.Add(data);
            List<ResponseJsDb> response = await DbInsert(jsRuntime, rows);
            return response[0];
        }

        /// <summary>
        /// Insert into a table to a db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="rows">data to insert</param>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbInsert<T>(this IJSRuntime jsRuntime, List<T> rows)
        {
            List<ResponseJsDb> result;
            if (rows.Count > 0)
            {
                try
                {
                    string data = JsonSerializer.Serialize(rows);
                    result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Insert", Utils.GetName<T>(), data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DbInsert Error: {ex}");
                    result = new List<ResponseJsDb>{
                        new ResponseJsDb { Result = false, Message = ex.Message }
                    };
                }
            }
            else result = new List<ResponseJsDb>{
                        new ResponseJsDb { Result = true, Message = "No need insert!" }
                    };
            return result;
        }

        /// <summary>
        /// Insert int a table to a db with offline property
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="rows">data to insert</param>
        /// <returns></returns>
        public static async ValueTask<List<ResponseJsDb>> DbInserOfline<T>(this IJSRuntime jsRuntime, List<T> rows)
        {
            List<ResponseJsDb> result;
            if (rows.Count > 0)
            {
                var expanded = AddOflineProperty.AddOffline(rows);
                try
                {
                    string data = JsonSerializer.Serialize(expanded);
                    result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Insert", Utils.GetName<T>(), data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DbInsert Error: {ex}");
                    result = new List<ResponseJsDb>{
                        new ResponseJsDb { Result = false, Message = ex.Message }
                    };
                }
            }
            else result = new List<ResponseJsDb>{
                        new ResponseJsDb { Result = true, Message = "No need insert!" }
                    };
            return result;
        }

    }
}
