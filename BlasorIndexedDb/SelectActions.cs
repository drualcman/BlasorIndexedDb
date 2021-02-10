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
        public static async ValueTask<T> DbSelect<T>(this IJSRuntime jsRuntime, T table)
        {
            T data = await jsRuntime.InvokeAsync<T>("MyDb.Select", Utils.GetName<T>());
            return data;
        }

    }
}
