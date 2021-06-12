using BlazorIndexedDb.Helpers;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Delete commands
    /// </summary>
    public static class DeleteActions
    {
        /// <summary>
        /// Delete table to a db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="id">id from the row to delete</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<T>(this IJSRuntime jsRuntime, [NotNull] int id)
        {
            List<ResponseJsDb> result;
            try
            {
                result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Delete", Utils.GetName<T>(), id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DbDelete Error: {ex}");
                result = new List<ResponseJsDb>();
                result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
            }
            return result[0];
        }

        /// <summary>
        /// Delete table to a db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="id">id from the row to delete</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<T>(this IJSRuntime jsRuntime, [NotNull] double id)
        {
            List<ResponseJsDb> result;
            try
            {
                result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Delete", Utils.GetName<T>(), id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DbDelete Error: {ex}");
                result = new List<ResponseJsDb>();
                result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
            }
            return result[0]; 
        }


        /// <summary>
        /// Delete table to a db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="id">id from the row to delete</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<T>(this IJSRuntime jsRuntime, [NotNull] decimal id)
        {
            List<ResponseJsDb> result;
            try
            {
                result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Delete", Utils.GetName<T>(), id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DbDelete Error: {ex}");
                result = new List<ResponseJsDb>();
                result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
            }
            return result[0];
        }


        /// <summary>
        /// Delete table to a db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="id">id from the row to delete</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<T>(this IJSRuntime jsRuntime, [NotNull] string id)
        {
            List<ResponseJsDb> result;
            try
            {
                result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Delete", Utils.GetName<T>(), id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DbDelete Error: {ex}");
                result = new List<ResponseJsDb>();
                result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
            }
            return result[0];
        }

        /// <summary>
        /// Delete table to a db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="id">id from the row to delete</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<T>(this IJSRuntime jsRuntime, [NotNull] DateTime id)
        {
            List<ResponseJsDb> result;
            try
            {
                result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Delete", Utils.GetName<T>(), id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DbDelete Error: {ex}");
                result = new List<ResponseJsDb>();
                result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
            }
            return result[0];
        }
    }
}
