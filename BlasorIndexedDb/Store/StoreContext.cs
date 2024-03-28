namespace BlazorIndexedDb.Store
{
    /// <summary>
    ///     A StoreContext instance represents a instance of a indexedDb into the browser
    ///     with the stores and can be used to query and save instances of your entities.
    /// </summary>
    public abstract class StoreContext<TStore> where TStore : class
    {
        readonly Settings Setup;
        readonly DeleteActions DeleteActions;
        readonly IJSRuntime Js;

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
            Js = js;
            DeleteActions = new(Js, Setup);
            Init();
            InitStores();
        }

        /// <summary>
        /// Delete database from <inheritdocDb/>
        /// </summary>
        /// <returns></returns>
        public async Task<CommandResponse> DropDatabaseAsync()
        {
            CommandResponse response = Utils.CommandResponse(await DeleteActions.DropDatabase());
            if (response.Result)
            {
                InitializeDatabase.DropDatabase(Setup.DBName);
            }
            return response;
        }

        /// <summary>
        /// Initialize the connection with a indexedDb
        /// </summary>
        public Task Init()
        {
            Initalizing<TStore> initalizing = new Initalizing<TStore>(Setup);
            initalizing.DbInit();
            return Task.CompletedTask;
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
                properties[i].SetValue(this, Activator.CreateInstance(properties[i].PropertyType, Js, Setup));
            }
        }
    }
}
