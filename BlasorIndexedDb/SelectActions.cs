using BlasorIndexedDb.Helpers;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlasorIndexedDb
{
    public static class SelectActions
    {
        /// <summary>
        /// Get all resister from a table
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <returns></returns>
        public static async ValueTask<List<T>> DbSelect<T>(this IJSRuntime jsRuntime)
        {
            List<T> data = await jsRuntime.InvokeAsync<List<T>>("MyDb.Select", typeof(T).Name);
            return data;
        }

        /// <summary>
        /// Get all resister from a table
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <returns></returns>
        public static async ValueTask<List<T>> DbSelect<T>(this IJSRuntime jsRuntime, int id)
        {
            List<T> data = await jsRuntime.InvokeAsync<List<T>>("MyDb.SelectId", typeof(T).Name, id);
            return data;
        }

        /// <summary>
        /// Get all resister from a table
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="column">column to compare</param>
        /// <param name="value">value to compare</param>
        /// <returns></returns>
        public static async ValueTask<List<T>> DbSelect<T>(this IJSRuntime jsRuntime, string column, object value)
        {
            List<T> data = await jsRuntime.InvokeAsync<List<T>>("MyDb.SelectWhere", typeof(T).Name, column, value);
            return data;
        }

    }
}
