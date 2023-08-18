namespace BlazorIndexedDb.Store
{
    /// <summary>
    ///     A StoreContext instance represents a instance of a indexedDb into the browser
    ///     with the stores and can be used to query and save instances of your entities.
    /// </summary>
    public abstract class StoreContext<TStore> : IAsyncDisposable where TStore : class
    {
        readonly Settings Setup;
        readonly Lazy<Task<IJSObjectReference>> ModuleTask;

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
            ModuleTask = new Lazy<Task<IJSObjectReference>>(() => GetJSObjectReference(js));
        }

        private Task<IJSObjectReference> GetJSObjectReference(IJSRuntime jsRuntime) =>
            jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", $"./_content/DrUalcman-BlazorIndexedDb/MyDbJS.js?v={DateTime.Today.Year}{DateTime.Today.Month}{DateTime.Today.Day}").AsTask();

        /// <summary>
        /// Initialize the connection with a indexedDb
        /// </summary>
        public async Task Init()
        {
            if(Settings.EnableDebug)
                Console.WriteLine($"StoreContext Init => Is Initialized {Setup.Initialized}");
            if(!Setup.Initialized)
            {
                try
                {
                    IJSObjectReference jsReference = await ModuleTask.Value;
                    Initalizing<TStore> initalizing = new Initalizing<TStore>(jsReference, Setup);
                    await initalizing.DbInit();
                    InitStores(jsReference);
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

        void InitStores(IJSObjectReference jsReference)
        {
            PropertyInfo[] properties = this.GetType()
                   .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(StoreSet<>))).ToArray();

            int c = properties.Length;
            for(int i = 0; i < c; i++)
            {
                //instance the property with StoreSet type
                properties[i].SetValue(this, Activator.CreateInstance(properties[i].PropertyType, jsReference, Setup));
            }
        }

        /// <summary>
        ///  Implementation of IAsyncDisposable.DisposeAsync()
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
        }

        /// <summary>
        /// Implementation of IAsyncDisposable.DisposeAsync()
        /// </summary>
        /// <returns></returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            // Asynchronously dispose of any managed resources
            if(ModuleTask.IsValueCreated)
            {
                IJSObjectReference module = await ModuleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}
