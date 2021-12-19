using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Helpers;
using BlazorIndexedDb.Models;
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
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<List<TModel>> DbSelect<TModel>(this IJSRuntime jsRuntime)
        {
            List<TModel> data;
            if (Settings.Initialized)
            {
                try
                {
                    data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.Select", Settings.Tables.GetTable<TModel>());
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(DbSelect), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initialized yet!");
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
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<List<TModel>> DbSelect<TModel>(this IJSRuntime jsRuntime, [NotNull] string column, [NotNull] object value)
        {
            List<TModel> data;
            if (Settings.Initialized)
            {
                try
                {
                    data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectWhere", Settings.Tables.GetTable<TModel>(), column, value);
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(DbSelect), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initialized yet!");
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
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] object id) where TModel : class
        {
            if (Settings.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Settings.Tables.GetTable<TModel>(), id);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initialized yet!");
                return null;
            }
        }

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] int id) where TModel : class
        {
            if (Settings.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Settings.Tables.GetTable<TModel>(), id);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initialized yet!");
                return null;
            }
        }


        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] double id) where TModel : class
        {
            if (Settings.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Settings.Tables.GetTable<TModel>(), id);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initialized yet!");
                return null;
            }
        }

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] decimal id) where TModel : class
        {
            if (Settings.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Settings.Tables.GetTable<TModel>(), id);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initialized yet!");
                return null;
            }
        }

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] long id) where TModel : class
        {
            if (Settings.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Settings.Tables.GetTable<TModel>(), id);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initialized yet!");
                return null;
            }
        }

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] string id) where TModel : class
        {
            if (Settings.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Settings.Tables.GetTable<TModel>(), id);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initialized yet!");
                return null;
            }
        }


        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <param name="jsRuntime"></param>
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public static async ValueTask<TModel> SingleRecord<TModel>(this IJSRuntime jsRuntime, [NotNull] DateTime id) where TModel : class
        {
            if (Settings.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Settings.Tables.GetTable<TModel>(), id);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Settings.Tables.GetTable<TModel>(), ex.Message, ex);
                }
            }
            else
            {
                if (Settings.EnableDebug) Console.WriteLine($"SelectActions: IndexedDb not initialized yet!");
                return null;
            }
        }
        #endregion

    }
}
