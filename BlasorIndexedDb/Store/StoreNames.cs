namespace BlazorIndexedDb.Store
{
    /// <summary>
    /// Encapsulate manage tables and model for the database
    /// </summary>
    internal class StoreNames
    {
        readonly List<StoreSetDefinition> Tables;
        /// <summary>
        /// Create a instance and initialized private tabes list with TableWithModel class
        /// </summary>
        public StoreNames() => this.Tables = new List<StoreSetDefinition>();
        /// <summary>
        /// Create a instance with a list of tables
        /// </summary>
        /// <param name="tables"></param>
        public StoreNames(IEnumerable<StoreSetDefinition> tables) => Tables = new(tables);


        /// <summary>
        /// Add new table with object definition
        /// </summary>
        /// <param name="table"></param>
        public void AddTable(StoreSetDefinition table) => Tables.Add(table);
        /// <summary>
        /// Add new table with model
        /// </summary>
        /// <param name="name"></param>
        /// <param name="model"></param>
        public void AddTable(string name, string model) => AddTable(new StoreSetDefinition { Name = name, Model = model });

        /// <summary>
        /// Serach if the table already exists
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool InTables(string tableName) => Tables.Exists(t => t.Name == tableName);

        /// <summary>
        /// Serach if the model already exists in some table
        /// </summary>
        /// <returns></returns>
        public bool InTables<TModel>() => InTables(GetTable(typeof(TModel).Name));

        /// <summary>
        /// Get table name from the model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string GetTable(string model) => Tables.Where(m => m.Model == model).Select(t => t.Name).FirstOrDefault();

        /// <summary>
        /// Get table name from the model
        /// </summary>
        /// <returns></returns>
        public string GetTable<TModel>() => GetTable(typeof(TModel).Name);

        /// <summary>
        /// Get table name from the model
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public string GetModel(string table) => Tables.Where(m => m.Name == table).Select(t => t.Model).FirstOrDefault();
    }
}
