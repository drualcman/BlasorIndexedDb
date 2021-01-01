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
        /// Extent JSRuntime to get a confirm
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="name">database name</param>
        /// <param name="version">database version</param>
        /// <param name="tables">string array with the names of the model classes to serialize</param>
        /// <param name="mynamespace">namespace from the models class</param>
        /// <returns></returns>
        public static async ValueTask<T> DbSelect<T>(this IJSRuntime jsRuntime, T table)
        {
            T data = await jsRuntime.InvokeAsync<T>("MyDb.Select", Utils.GetName<T>());
            return data;
        }

    }
}
