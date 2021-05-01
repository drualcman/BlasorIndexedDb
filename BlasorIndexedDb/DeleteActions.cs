using BlasorIndexedDb.Helpers;
using BlasorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlasorIndexedDb
{
    public static class DeleteActions
    {
        /// <summary>
        /// Delete table to a db
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="id">id from the row to delete</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<T>(this IJSRuntime jsRuntime, int id)
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
        public static async ValueTask<ResponseJsDb> DbDelete<T>(this IJSRuntime jsRuntime, double id)
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
        public static async ValueTask<ResponseJsDb> DbDelete<T>(this IJSRuntime jsRuntime, decimal id)
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
        public static async ValueTask<ResponseJsDb> DbDelete<T>(this IJSRuntime jsRuntime, string id)
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
        public static async ValueTask<ResponseJsDb> DbDelete<T>(this IJSRuntime jsRuntime, DateTime id)
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
