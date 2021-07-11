using BlazorIndexedDb.Commands;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Store
{
    /// <summary>
    /// Interface to implement in all services
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class StoreService<TModel> : IStoreService<TModel> where TModel : class
    {

        /// <summary>
        /// Manage JAVASCRIPT
        /// </summary>
        protected readonly IJSRuntime DBConn;

        /// <summary>
        /// Inject IJSRuntime when instance this class
        /// </summary>
        /// <param name="js"></param>
        public StoreService(IJSRuntime js)
        {
            DBConn = js;
        }

        /// <summary>
        /// Get all rows from the table
        /// </summary>
        /// <returns></returns>
        public async Task<List<TModel>> FilesStore() =>
            await DBConn.DbSelect<TModel>();

        /// <summary>
        /// Get Only one row from the table
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TModel> FilesStore(string id)=> 
            await DBConn.SingleRecord<TModel>(id);

        /// <summary>
        /// Add data to the table
        /// </summary>
        /// <param name="toAdd"></param>
        /// <returns></returns>
        public async Task<ResponseJsDb> AddAsync(TModel toAdd) => 
            await DBConn.DbInsert(toAdd);

        /// <summary>
        /// Update data into a table
        /// </summary>
        /// <param name="toAdd"></param>
        /// <returns></returns>
        public async Task<ResponseJsDb> UpdateAsync(TModel toAdd) => 
            await DBConn.DbUpdate(toAdd);


        /// <summary>
        /// Delete row from a table
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ResponseJsDb> DeleteAsync(string id) => 
            await DBConn.DbDelete<TModel>(id);
    }
}
