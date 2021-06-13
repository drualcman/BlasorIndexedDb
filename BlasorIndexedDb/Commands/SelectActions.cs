using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Helpers;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Select commands
    /// </summary>
    public static class SelectActions
    {
        #region lists
        /// <summary>
        /// Get all resister from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <returns></returns>
        public static async ValueTask<List<TModel>> DbSelect<TModel>(this IJSRuntime jsRuntime)
        {
            List<TModel> data;
            if (Settings.Initiallezed)
            {
                try
                {
                    data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.Select", typeof(TModel).Name);
                }
                catch 
                {
                    data = new List<TModel>();
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initiallized yet!");
                data = new List<TModel>();
            }
            return data;
        }

        /// <summary>
        /// Get all resister from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="column">column to compare</param>
        /// <param name="value">value to compare</param>
        /// <returns></returns>
        public static async ValueTask<List<TModel>> DbSelect<TModel>(this IJSRuntime jsRuntime, [NotNull] string column, [NotNull] object value)
        {
            List<TModel> data;
            if (Settings.Initiallezed)
            {
                try
                {
                    data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectWhere", typeof(TModel).Name, column, value);
                }
                catch
                {
                    data = new List<TModel>();
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initiallized yet!");
                data = new List<TModel>();
            }
            return data;
        }
        #endregion

        #region single

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] object id) where TModel : class
        {
            if (Settings.Initiallezed)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", typeof(TModel).Name, id);
                    return data[0];
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initiallized yet!");
                return null;
            }           
        }

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] int id) where TModel : class
        {
            if (Settings.Initiallezed)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", typeof(TModel).Name, id);
                    return data[0];
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initiallized yet!");
                return null;
            }
        }


        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] double id) where TModel : class
        {
            if (Settings.Initiallezed)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", typeof(TModel).Name, id);
                    return data[0];
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initiallized yet!");
                return null;
            }
        }

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] decimal id) where TModel : class
        {
            if (Settings.Initiallezed)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", typeof(TModel).Name, id);
                    return data[0];
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initiallized yet!");
                return null;
            }
        }

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] long id) where TModel : class
        {
            if (Settings.Initiallezed)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", typeof(TModel).Name, id);
                    return data[0];
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initiallized yet!");
                return null;
            }
        }

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] string id) where TModel : class
        {
            if (Settings.Initiallezed)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", typeof(TModel).Name, id);
                    return data[0];
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initiallized yet!");
                return null;
            }
        }


        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] DateTime id) where TModel : class
        {
            if (Settings.Initiallezed)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", typeof(TModel).Name, id);
                    return data[0];
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initiallized yet!");
                return null;
            }
        }
        #endregion

    }
}
