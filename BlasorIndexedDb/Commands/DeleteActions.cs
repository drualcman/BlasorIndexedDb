using BlazorIndexedDb.Configuration;
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
        /// Detete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<TModel>(this IJSRuntime jsRuntime, [NotNull] int id)
        {
            List<ResponseJsDb> result;
            if (Settings.Initiallezed)
            {
                try
                {
                    result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Delete", Utils.GetName<TModel>(), id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DbDelete Error: {ex}");
                    result = new List<ResponseJsDb>();
                    result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
                }
            }
            else
            {
                Console.WriteLine($"IndexedDb not initiallized yet!");
                result = new List<ResponseJsDb>();
                result.Add(new ResponseJsDb { Result = false, Message = $"IndexedDb not initiallized yet!" });
            }
            return result[0];
        }

        /// <summary>
        /// Detete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">id from the row to delete</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<TModel>(this IJSRuntime jsRuntime, [NotNull] double id)
        {
            List<ResponseJsDb> result;

            if (Settings.Initiallezed)
            {
                try
                {
                    result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Delete", Utils.GetName<TModel>(), id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DbDelete Error: {ex}");
                    result = new List<ResponseJsDb>();
                    result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
                }
            }
            else
            {
                Console.WriteLine($"IndexedDb not initiallized yet!");
                result = new List<ResponseJsDb>();
                result.Add(new ResponseJsDb { Result = false, Message = $"IndexedDb not initiallized yet!" });
            }
            return result[0]; 
        }


        /// <summary>
        /// Detete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">id from the row to delete</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<TModel>(this IJSRuntime jsRuntime, [NotNull] decimal id)
        {
            List<ResponseJsDb> result;

            if (Settings.Initiallezed)
            {
                try
                {
                    result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Delete", Utils.GetName<TModel>(), id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DbDelete Error: {ex}");
                    result = new List<ResponseJsDb>();
                    result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
                }
            }
            else
            {
                Console.WriteLine($"IndexedDb not initiallized yet!");
                result = new List<ResponseJsDb>();
                result.Add(new ResponseJsDb { Result = false, Message = $"IndexedDb not initiallized yet!" });
            }
            return result[0];
        }


        /// <summary>
        /// Detete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">id from the row to delete</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<TModel>(this IJSRuntime jsRuntime, [NotNull] string id)
        {
            List<ResponseJsDb> result;

            if (Settings.Initiallezed)
            {
                try
                {
                    result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Delete", Utils.GetName<TModel>(), id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DbDelete Error: {ex}");
                    result = new List<ResponseJsDb>();
                    result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
                }
            }
            else
            {
                Console.WriteLine($"IndexedDb not initiallized yet!");
                result = new List<ResponseJsDb>();
                result.Add(new ResponseJsDb { Result = false, Message = $"IndexedDb not initiallized yet!" });
            }
            return result[0];
        }

        /// <summary>
        /// Detete one row from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">id from the row to delete</param>
        /// <returns></returns>
        public static async ValueTask<ResponseJsDb> DbDelete<TModel>(this IJSRuntime jsRuntime, [NotNull] DateTime id)
        {
            List<ResponseJsDb> result;
            if (Settings.Initiallezed)
            {
                try
                {
                    result = await jsRuntime.InvokeAsync<List<ResponseJsDb>>("MyDb.Delete", Utils.GetName<TModel>(), id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DbDelete Error: {ex}");
                    result = new List<ResponseJsDb>();
                    result.Add(new ResponseJsDb { Result = false, Message = ex.Message });
                }
            }
            else
            {
                Console.WriteLine($"IndexedDb not initiallized yet!");
                result = new List<ResponseJsDb>();
                result.Add(new ResponseJsDb { Result = false, Message = $"IndexedDb not initiallized yet!" });
            }
            return result[0];
        }
    }
}
