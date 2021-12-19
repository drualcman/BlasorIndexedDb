using BlazorIndexedDb.Commands;
using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Helpers;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Store
{
    /// <summary>
    /// Class represent
    /// </summary>
    public class StoreSet<TModel> where TModel : class
    {
        private readonly IJSRuntime DBConn;

        /// <summary>
        /// Constructor to get the connection
        /// </summary>
        /// <param name="js"></param>
        public StoreSet(IJSRuntime js)
        {
            if (Settings.EnableDebug) Console.WriteLine($"StoreSet constructor for : {Utils.GetGenericTypeName(this.GetType())}");
            DBConn = js;
        }

        /// <summary>
        /// Get a list with the content about the store model
        /// </summary>
        /// <returns></returns>
        public async Task<List<TModel>> SelectAsync() =>
            await DBConn.DbSelect<TModel>();

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(object id) =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(int id) =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(double id) =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(decimal id) =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(long id) =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(string id) =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(DateTime id) =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Add record in a table store
        /// </summary>
        /// <param name="toAdd"></param>
        /// <param name="isOffline"></param>
        /// <returns></returns>
        public async Task<CommandResponse> AddAsync(TModel toAdd, bool isOffline = false)
        {
            ResponseJsDb result;
            if (isOffline)
                result = await DBConn.DbInserOffline(toAdd);
            else
                result = await DBConn.DbInsert(toAdd);
            return Utils.CommandResponse(result);
        }

        /// <summary>
        /// Add record in a table store
        /// </summary>
        /// <param name="toAdd"></param>
        /// <param name="isOffline"></param>
        /// <returns></returns>
        public async Task<CommandResponse> AddAsync(List<TModel> toAdd, bool isOffline = false)
        {
            List<ResponseJsDb> result;
            if (isOffline)
                result = await DBConn.DbInserOffline(toAdd);
            else
                result = await DBConn.DbInsert(toAdd);
            return Utils.CommandResponse(result);
        }

        /// <summary>
        /// Update record in a table store
        /// </summary>
        /// <param name="toUpdate"></param>
        /// <param name="isOffline"></param>
        /// <returns></returns>
        public async Task<CommandResponse> UpdateAsync(TModel toUpdate, bool isOffline = false)
        {
            ResponseJsDb result;
            if (isOffline)
                result = await DBConn.DbUpdateOffLine(toUpdate);
            else
                result = await DBConn.DbUpdate(toUpdate);
            return Utils.CommandResponse(result);
        }

        /// <summary>
        /// Update record in a table store
        /// </summary>
        /// <param name="toUpdate"></param>
        /// <param name="isOffline"></param>
        /// <returns></returns>
        public async Task<CommandResponse> UpdateAsync(List<TModel> toUpdate, bool isOffline = false)
        {
            List<ResponseJsDb> result;
            if (isOffline)
                result = await DBConn.DbUpdateOffLine(toUpdate);
            else
                result = await DBConn.DbUpdate(toUpdate);
            return Utils.CommandResponse(result);
        }

        /// <summary>
        /// Delete all rows from a table
        /// </summary>
        /// <returns></returns>
        public async Task<CommandResponse> CleanAsync() =>
            Utils.CommandResponse(await DBConn.DbCleanTable<TModel>());

        /// <summary>
        /// Delete record from a table store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DeleteAsync(int id)
            => Utils.CommandResponse(await DBConn.DbDelete<TModel>(id));

        /// <summary>
        /// Delete record from a table store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DeleteAsync(double id)
            => Utils.CommandResponse(await DBConn.DbDelete<TModel>(id));

        /// <summary>
        /// Delete record from a table store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DeleteAsync(decimal id)
            => Utils.CommandResponse(await DBConn.DbDelete<TModel>(id));

        /// <summary>
        /// Delete record from a table store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DeleteAsync(string id)
            => Utils.CommandResponse(await DBConn.DbDelete<TModel>(id));

        /// <summary>
        /// Delete record from a table store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DeleteAsync(DateTime id)
            => Utils.CommandResponse(await DBConn.DbDelete<TModel>(id));

    }
}
