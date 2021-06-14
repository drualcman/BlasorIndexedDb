using BlazorIndexedDb.Commands;
using BlazorIndexedDb.Configuration;
using BlazorIndexedDb.Helpers;
using BlazorIndexedDb.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            if (Settings.EnableDebug) Console.WriteLine($"StoreContext minimum constructor");
            DBConn = js;
            Settings.EnableDebug = true;
            GetTables();
        }

        /// <summary>
        /// Constructor to get the connection
        /// </summary>
        /// <param name="js"></param>
        /// <param name="settings"></param>
        public StoreContext(IJSRuntime js, Settings settings)
        {
            if (Settings.EnableDebug) Console.WriteLine($"StoreContext constructor with settings");
            DBConn = js;
            Settings.EnableDebug = true;
            GetTables();
            Init(settings);
        }

        /// <summary>
        /// Initialize the connection with a indexedDb
        /// </summary>
        /// <param name="settings"></param>
        public void Init(Settings settings)
        {
            if (!Settings.Initiallezed)
            {
                _ = Initalizing.DbInit(DBConn, settings);
            }
        }

       void GetTables()
        {
            if (!Settings.Initiallezed)
            {
                PropertyInfo[] properties = this.GetType()
                       .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Where(x => x.PropertyType.Name == "StoreSet`1").ToArray();                

                List<string> tables = new List<string>();
                foreach (PropertyInfo item in properties)
                {
                    tables.Add(Utils.GetGenericTypeName(item.PropertyType));
                    //instanze the propertie with StoreSet type
                    item.SetValue(this, Activator.CreateInstance(item.PropertyType, DBConn));
                    
                }
                Settings.Tables = tables.ToArray();
            }

        }
    }
}
