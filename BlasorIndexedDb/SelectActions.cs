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
        /// Get all resigster from a table
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="table">table name</param>
        /// <returns></returns>
        public static async ValueTask<List<T>> DbSelect<T>(this IJSRuntime jsRuntime, string table)
        {
            List<T> data = await jsRuntime.InvokeAsync<List<T>>("MyDb.Select", table);
            return data;
        }

        /// <summary>
        /// Get all resigster from a table
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="table">table name</param>
        /// <param name="id">column to compare</param>
        /// <returns></returns>
        public static async ValueTask<List<T>> DbSelect<T>(this IJSRuntime jsRuntime, string table, int id)
        {
            List<T> data = await jsRuntime.InvokeAsync<List<T>>("MyDb.SelectId", table, id);
            return data;
        }

        /// <summary>
        /// Get all resigster from a table
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="table">table name</param>
        /// <param name="column">column to compare</param>
        /// <param name="value">value to compare</param>
        /// <returns></returns>
        public static async ValueTask<List<T>> DbSelect<T>(this IJSRuntime jsRuntime, string table, string column, object value)
        {
            List<T> data = await jsRuntime.InvokeAsync<List<T>>("MyDb.SelectWhere", table, column, value);
            return data;
        }

    }
}
