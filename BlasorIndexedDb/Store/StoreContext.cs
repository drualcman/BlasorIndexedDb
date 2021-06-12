using BlazorIndexedDb.Commands;
using BlazorIndexedDb.Configuration;
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
    ///     A StoreContext instance represents a instance of a indexedDb into the browser
    ///     with the stores and can be used to query and save instances of your entities.
    /// </summary>
    public class StoreContext
    {
        private readonly IJSRuntime DBConn;

        /// <summary>
        /// Constructor to get the connection
        /// </summary>
        /// <param name="js"></param>
        public StoreContext(IJSRuntime js)
        {
            DBConn = js;
        }

        /// <summary>
        /// Initialize the connection with a indexedDb
        /// </summary>
        /// <param name="settings"></param>
        public void Init(Settings settings)
        {
            Task task = Initalizing.DbInit(DBConn);
            task.Start();
        }

       
    }
}
