using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Select commands
    /// </summary>
    public class SelectActions
    {
        readonly IJSObjectReference jsRuntime;
        readonly Settings Setup;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="js"></param>
        /// <param name="setup"></param>
        public SelectActions(IJSObjectReference js, Settings setup)
        {
            jsRuntime = js;
            Setup = setup;
        }

        #region lists
        /// <summary>
        /// Get all resister from a table
        /// </summary>
        /// <typeparam name="TModel">Table or store to use</typeparam>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async ValueTask<List<TModel>> DbSelect<TModel>()
        {
            List<TModel> data;
            if (Setup.Initialized)
            {
                try
                {
                    data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.Select", Setup.Tables.GetTable<TModel>(), Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(DbSelect), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
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
        /// <param name="column">column to compare</param>
        /// <param name="value">value to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async ValueTask<List<TModel>> DbSelect<TModel>([NotNull] string column, [NotNull] object value)
        {
            List<TModel> data;
            if (Setup.Initialized)
            {
                try
                {
                    data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectWhere", Setup.Tables.GetTable<TModel>(), column, value, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(DbSelect), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
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
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async ValueTask<TModel> SingleRecord<TModel>([NotNull] object id) where TModel : class
        {
            if (Setup.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
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
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async ValueTask<TModel> SingleRecord<TModel>([NotNull] int id) where TModel : class
        {
            if (Setup.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
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
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public  async ValueTask<TModel> SingleRecord<TModel>([NotNull] double id) where TModel : class
        {
            if (Setup.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
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
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public  async ValueTask<TModel> SingleRecord<TModel>([NotNull] decimal id) where TModel : class
        {
            if (Setup.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
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
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async ValueTask<TModel> SingleRecord<TModel>([NotNull] long id) where TModel : class
        {
            if (Setup.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
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
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async ValueTask<TModel> SingleRecord<TModel>([NotNull] string id) where TModel : class
        {
            if (Setup.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
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
        /// <param name="id">column to compare</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public async ValueTask<TModel> SingleRecord<TModel>([NotNull] DateTime id) where TModel : class
        {
            if (Setup.Initialized)
            {
                try
                {
                    List<TModel> data = await jsRuntime.InvokeAsync<List<TModel>>("MyDb.SelectId", Setup.Tables.GetTable<TModel>(), id, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                    return data[0];
                }
                catch (Exception ex)
                {
                    throw new ResponseException(nameof(SingleRecord), Setup.Tables.GetTable<TModel>(), ex.Message, ex);
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
