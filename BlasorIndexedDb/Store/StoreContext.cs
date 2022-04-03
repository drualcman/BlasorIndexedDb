using BlazorIndexedDb.Configuration;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Reflection;

namespace BlazorIndexedDb.Store
{
    /// <summary>
    ///     A StoreContext instance represents a instance of a indexedDb into the browser
    ///     with the stores and can be used to query and save instances of your entities.
    /// </summary>
    public abstract class StoreContext<TStore>
    {
        /// <summary>
        /// Manage the javascript file
        /// </summary>
        protected IJSObjectReference JsReference = null!;
        readonly Settings Setup;

        /// <summary>
        /// Constructor to get the connection
        /// </summary>
        /// <param name="js"></param>
        public StoreContext(IJSRuntime js)
        {
            if (Settings.EnableDebug) Console.WriteLine($"StoreContext minimum constructor");
            //GetTables();
            Setup = new Settings();
            Init(js);
        }

        /// <summary>
        /// Constructor to get the connection
        /// </summary>
        /// <param name="js"></param>
        /// <param name="settings"></param>
        public StoreContext(IJSRuntime js, Settings settings)
        {
            if (Settings.EnableDebug) Console.WriteLine($"StoreContext constructor with settings");
            Setup = settings;
            Init(js);
        }

        /// <summary>
        /// Initialize the connection with a indexedDb
        /// </summary>
        /// <param name="js"></param>
        public async void Init(IJSRuntime js)
        {
            if (Settings.EnableDebug) Console.WriteLine($"StoreContext Init => Is Initialized {Setup.Initialized}");
            if (!Setup.Initialized)
            {
                JsReference = await js.InvokeAsync<IJSObjectReference>("import", "./_content/DrUalcman-BlazorIndexedDb/MyDbJS.js");                
                Initalizing<TStore> initalizing = new Initalizing<TStore>(JsReference, Setup);
                await initalizing.DbInit();
                InitStores();
            }
        }

        void InitStores()
        {
            PropertyInfo[] properties = this.GetType()
                   .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(StoreSet<>))).ToArray();

            int c = properties.Length;
            for (int i = 0; i < c; i++)
            {
                //instance the property with StoreSet type
                properties[i].SetValue(this, Activator.CreateInstance(properties[i].PropertyType, JsReference, Setup));
            }
        }
    }
}
