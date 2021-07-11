using BlazorIndexedDb.Models;
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
    public interface IStoreService<TModel>
    {
        /// <summary>
        /// Get all rows from the table
        /// </summary>
        /// <returns></returns>
        public Task<List<TModel>> FilesStore();

        /// <summary>
        /// Get Only one row from the table
        /// </summary>
        /// <param name="id">column name to compare in the where</param>
        /// <returns></returns>
        public Task<TModel> FilesStore(string id);

        /// <summary>
        /// Add data to the table
        /// </summary>
        /// <param name="toAdd"></param>
        /// <returns></returns>
        public Task<ResponseJsDb> AddAsync(TModel toAdd);

        /// <summary>
        /// Update data into a table
        /// </summary>
        /// <param name="toAdd"></param>
        /// <returns></returns>
        public Task<ResponseJsDb> UpdateAsync(TModel toAdd);

        /// <summary>
        /// Delete row from a table
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<ResponseJsDb> DeleteAsync(string id);
    }
}
