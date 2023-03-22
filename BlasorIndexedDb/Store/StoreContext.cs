namespace BlazorIndexedDb.Store
{
    /// <summary>
    ///     A StoreContext instance represents a instance of a indexedDb into the browser
    ///     with the stores and can be used to query and save instances of your entities.
    /// </summary>
    public abstract class StoreContext<TStore> : IAsyncDisposable, IDisposable where TStore : class
    {
        /// <summary>
        /// Manage the javascript file
        /// </summary>
        protected IJSObjectReference JsReference = null!;
        readonly Settings Setup;
        readonly IJSRuntime JSRuntime;

        /// <summary>
        /// Constructor to get the connection
        /// </summary>
        /// <param name="js"></param>
        public StoreContext(IJSRuntime js) : this(js, new()) { }

        /// <summary>
        /// Constructor to get the connection
        /// </summary>
        /// <param name="js"></param>
        /// <param name="settings"></param>
        public StoreContext(IJSRuntime js, Settings settings)
        {
            if(Settings.EnableDebug)
                Console.WriteLine($"StoreContext constructor with settings");
            Setup = settings;
            JSRuntime = js;
        }

        /// <summary>
        /// Initialize the connection with a indexedDb
        /// </summary>
        /// <param name="js"></param>
        internal async Task Init()
        {
            if(Settings.EnableDebug)
                Console.WriteLine($"StoreContext Init => Is Initialized {Setup.Initialized}");
            if(!Setup.Initialized)
            {
                try
                {
                    JsReference = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/DrUalcman-BlazorIndexedDb/MyDbJS.js");
                    Initalizing<TStore> initalizing = new Initalizing<TStore>(JsReference, Setup);
                    await initalizing.DbInit();
                    InitStores();
                    Setup.Initialized = true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Setup.Initialized = false;
                    throw;
                }
            }
        }

        void InitStores()
        {
            PropertyInfo[] properties = this.GetType()
                   .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(StoreSet<>))).ToArray();

            int c = properties.Length;
            for(int i = 0; i < c; i++)
            {
                //instance the property with StoreSet type
                properties[i].SetValue(this, Activator.CreateInstance(properties[i].PropertyType, JsReference, Setup));
            }
        }

        /// <summary>
        ///       Implementation of IDisposable.Dispose()
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Implementation of IDisposable.Dispose()
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                // Dispose of any managed resources
            }
        }

        /// <summary>
        ///  Implementation of IAsyncDisposable.DisposeAsync()
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implementation of IAsyncDisposable.DisposeAsync()
        /// </summary>
        /// <returns></returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            // Asynchronously dispose of any managed resources
            if (JsReference is not null)
                await JsReference.DisposeAsync();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~StoreContext()
        {
            Dispose(false);
        }
    }
}
