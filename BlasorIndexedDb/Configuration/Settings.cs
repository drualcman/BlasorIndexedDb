namespace BlazorIndexedDb.Configuration
{
    /// <summary>
    /// Configuration class to setup a IndexedDb in a browser
    /// </summary>
    public sealed class Settings
    {
        /// <summary>
        /// Save a copy of how is the tables definition in JSON to send to the Javasctipt to compare the models
        /// </summary>
        internal string ModelsAsJson { get; set; }

        private string DbName_BK;
        /// <summary>
        /// Database name
        /// </summary>
        public string DBName
        {
            get { return DbName_BK; }
            set { DbName_BK = string.IsNullOrEmpty(value) ? AppDomain.CurrentDomain.FriendlyName : value; }
        }
        /// <summary>
        /// Version of the database
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Assembly name to search the entities
        /// </summary>
        internal string AssemblyName { get; set; }

        /// <summary>
        /// Default constructor get the AppDomain.CurrentDomain.FriendlyName like a DB name
        /// </summary>
        public Settings() : this(string.Empty) { }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="DbName"></param>
        /// <param name="version"></param>
        public Settings(string DbName, int version = 1)
        {
            this.DBName = DbName;
            this.Version = version;
        }

        /// <summary>
        /// Enable show console log info
        /// </summary>
        public static bool EnableDebug { get; set; }
        /// <summary>
        /// Names about the models to use
        /// </summary>
        internal StoreNames Tables { get; set; } = new StoreNames();
    }
}
