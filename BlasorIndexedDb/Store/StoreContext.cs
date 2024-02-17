namespace BlazorIndexedDb.Store
{
    /// <summary>
    ///     A StoreContext instance represents a instance of a indexedDb into the browser
    ///     with the stores and can be used to query and save instances of your entities.
    /// </summary>
    public abstract class StoreContext<TStore> where TStore : class
    {
        readonly Settings Setup;

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
                Console.WriteLine($"{settings.DBName} => StoreContext constructor with settings");
            Setup = settings;
            Initalizing<TStore> initalizing = new Initalizing<TStore>(Setup);
            initalizing.DbInit();
            InitStores(js);
        }

        /// <summary>
        /// Initialize the connection with a indexedDb
        /// </summary>
        [Obsolete("Not longer in use. Simple return a task completed")]
        public Task Init() 
        {               
            return Task.CompletedTask;
        }

        void InitStores(IJSRuntime jsReference)
        {
            PropertyInfo[] properties = this.GetType()
                   .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(StoreSet<>))).ToArray();

            int c = properties.Length;
            for (int i = 0; i < c; i++)
            {
                //instance the property with StoreSet type
                properties[i].SetValue(this, Activator.CreateInstance(properties[i].PropertyType, jsReference, Setup));
            }
        }
    }
}
