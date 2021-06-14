﻿using BlazorIndexedDb.Commands;
using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Helpers;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
        public async Task<TModel> SelectAsync(int id)=>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(double id)  =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(decimal id)  =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(long id)  =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(string id)  =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(DateTime id)  =>
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Add record in a table store
        /// </summary>
        /// <param name="toAdd"></param>
        /// <param name="isOffline"></param>
        /// <returns></returns>
        public async Task<ResponseJsDb> AddAsync(TModel toAdd, bool isOffline = false)
        {
            ResponseJsDb result;
            if (isOffline)
                result = await DBConn.DbInserOffline(toAdd);
            else
                result = await DBConn.DbInsert(toAdd);
            return result;
        }

        /// <summary>
        /// Add record in a table store
        /// </summary>
        /// <param name="toAdd"></param>
        /// <param name="isOffline"></param>
        /// <returns></returns>
        public async Task<List<ResponseJsDb>> AddAsync(List<TModel> toAdd, bool isOffline = false)
        {
            List<ResponseJsDb> result;
            if (isOffline)
                result = await DBConn.DbInserOffline(toAdd);
            else
                result = await DBConn.DbInsert(toAdd);
            return result;
        }

        /// <summary>
        /// Update record in a table store
        /// </summary>
        /// <param name="toUpdate"></param>
        /// <param name="isOffline"></param>
        /// <returns></returns>
        public async Task<ResponseJsDb> UpdateAsync(TModel toUpdate, bool isOffline = false)
        {
            ResponseJsDb result;
            if (isOffline)
                result = await DBConn.DbUpdateOffLine(toUpdate);
            else
                result = await DBConn.DbUpdate(toUpdate);
            return result;
        }

        /// <summary>
        /// Update record in a table store
        /// </summary>
        /// <param name="toUpdate"></param>
        /// <param name="isOffline"></param>
        /// <returns></returns>
        public async Task<List<ResponseJsDb>> UpdateAsync(List<TModel> toUpdate, bool isOffline = false)
        {
            List<ResponseJsDb> result;
            if (isOffline)
                result = await DBConn.DbUpdateOffLine(toUpdate);
            else
                result = await DBConn.DbUpdate(toUpdate);
            return result;
        }

        /// <summary>
        /// Detele record from a table sotre
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ResponseJsDb> DeleteAsync(int id)
            => await DBConn.DbDelete<TModel>(id);

        /// <summary>
        /// Detele record from a table sotre
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ResponseJsDb> DeleteAsync(double id)
            => await DBConn.DbDelete<TModel>(id);

        /// <summary>
        /// Detele record from a table sotre
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ResponseJsDb> DeleteAsync(decimal id)
            => await DBConn.DbDelete<TModel>(id);

        /// <summary>
        /// Detele record from a table sotre
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ResponseJsDb> DeleteAsync(string id)
            => await DBConn.DbDelete<TModel>(id);

        /// <summary>
        /// Detele record from a table sotre
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ResponseJsDb> DeleteAsync(DateTime id)
            => await DBConn.DbDelete<TModel>(id);

    }
}