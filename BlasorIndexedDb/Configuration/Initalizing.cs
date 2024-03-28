namespace BlazorIndexedDb.Configuration
{
    /// <summary>
    /// Setup connection with the indexedDb in the browser
    /// </summary>
    internal sealed class Initalizing<TStore> where TStore : class
    {
        readonly Settings Setup;

        internal Initalizing(Settings setup)
        {
            Setup = setup;
        }

        /// <summary>
        /// Setup a indexedDb in a browser
        /// </summary>
        /// <returns></returns>
        internal void DbInit()
        {
            if (string.IsNullOrEmpty(Setup.DBName))
            {
                Setup.DBName = typeof(TStore).Name;
            }
            if (string.IsNullOrEmpty(Setup.AssemblyName))
            {
                Setup.AssemblyName = AppDomain.CurrentDomain.FriendlyName;
            }
            if (Setup.Version < 1) Setup.Version = 1;
            DbInit(Setup.DBName, Setup.Version);
        }

        /// <summary>
        /// Setup a indexedDb in a browser
        /// </summary>
        /// <param name="name">database name</param>
        /// <param name="version">database version</param>
        /// <returns></returns>
        private void DbInit(string name, int version)
        {
            if (version < 1) version = 1;
            if (string.IsNullOrEmpty(name))
            {
                name = AppDomain.CurrentDomain.FriendlyName;
            }
            Setup.DBName = name;
            Setup.Version = version;
            if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => DbInit initializing...");

            string model = $@"{{
                                ""name"": ""{name}"",
                                ""version"": {version},
                                ""tables"": []
                             }}";
            //use reflexion to serialize the tables like
            // {name: 'table name', options:{keyPath: 'primary id to use', autoIncrement: true/false},
            //  columns: [{name: 'property name', keyPath: true/false, autoIncrement: true/false, unique: true/false}]}

            try
            {
                IEnumerable<PropertyInfo> storeSets = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(s => s.IsAssignableTo(typeof(StoreContext<TStore>)))
                    .SelectMany(p => p.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    .Where(t => t.PropertyType.IsGenericType && t.PropertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(StoreSet<>)));

                string tableJsonArray = string.Empty;
                bool haveTables = false;
                foreach (PropertyInfo item in storeSets)
                {
                    if (Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => table name {item.Name}");
                    string tableName = item.Name;
                    Setup.Tables.AddTable(tableName, item.PropertyType.GetGenericArguments()[0].Name);
                    StringBuilder tableModels = new StringBuilder();
                    // {name: 'table name', options:{keyPath: 'primary id to use', autoIncrement: true/false},
                    //string tableName = Utils.GetGenericTypeName(t);
                    tableModels.Append($"{{\"name\": \"{tableName}\",");
                    //read all properties
                    PropertyInfo[] properties = item.PropertyType.GetGenericArguments()[0]          //get first generic type (only one can be)
                        .GetProperties(BindingFlags.Public |           //get public names
                                       BindingFlags.Instance);         //get instance names

                    //main field from the model, normalize like Id or ModelNameId or IdModelName
                    string identifer = string.Empty;
                    bool autoIncrement = false;     //if it's a integer must be true
                    tableModels.Append("\"options\": {\"keyPath\": \"#1\", \"autoIncrement\": #2},");
                    //add columns name from the properties names
                    tableModels.Append($"\"columns\": [");                  //open json array
                    int c = properties.Length;
                    for (int i = 0; i < c; i++)
                    {
                        PropertyOptions property = new PropertyOptions(properties[i]);
                        if (!property.ToIgnore)
                        {
                            //  columns: [{name: 'property name', keyPath: true/false, autoIncrement: true/false, unique: true/false}]}                                            
                            string propName = property.Name.ToLower();
                            haveTables = true;
                            //main field from the model, normalize like Id or ModelNameId or IdModelName
                            if (property.IsKeyPath)
                            {
                                identifer = property.Name;
                                autoIncrement = property.IsAutoIncrement;
                            }
                            else
                            {
                                if (propName.ToLower() == "id")
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsAutoIncrement;
                                }
                                else if (propName == $"{tableName.ToLower()}id")
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsKeyPath;
                                }
                                else if (propName == $"id{tableName.ToLower()}")
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsAutoIncrement;
                                }
                                else if (propName == $"id{tableName.ToLower()}s")                 //plural possibility
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsAutoIncrement;
                                }
                                else if (propName == $"id{tableName.ToLower().Remove(tableName.Length - 1, 1)}")                 //singular possibility
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsAutoIncrement;
                                } //next
                                else if (propName == $"{tableName.ToLower()}Id")
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsKeyPath;
                                }
                                else if (propName == $"Id{tableName.ToLower()}")
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsAutoIncrement;
                                }
                                else if (propName == $"Id{tableName.ToLower()}s")                 //plural possibility
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsAutoIncrement;
                                }
                                else if (propName == $"Id{tableName.ToLower().Remove(tableName.Length - 1, 1)}")                 //singular possibility
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsAutoIncrement;

                                } //last
                                else if (propName == $"{tableName.ToLower()}ID")
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsKeyPath;
                                }
                                else if (propName == $"ID{tableName.ToLower()}")
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsAutoIncrement;
                                }
                                else if (propName == $"ID{tableName.ToLower()}s")                 //plural possibility
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsAutoIncrement;
                                }
                                else if (propName == $"ID{tableName.ToLower().Remove(tableName.Length - 1, 1)}")                 //singular possibility
                                {
                                    identifer = property.Name;
                                    autoIncrement = property.IsAutoIncrement;
                                }
                                else tableModels.Append($"{{\"name\": \"{property.Name}\", \"keyPath\": {property.IsKeyPath.ToString().ToLower()}, \"autoIncrement\": {property.IsAutoIncrement.ToString().ToLower()}, \"unique\": {property.IsUnique.ToString().ToLower()}}},");
                            }
                        }
                    }
                    //always add control if the register it's offline NULL = not offline, only when it's true it's applicable
                    tableModels.Append($"{{\"name\": \"OffLine\", \"keyPath\": false, \"autoIncrement\": false, \"unique\": false}},");
                    if (string.IsNullOrEmpty(identifer))
                    {
                        //because don't have primary key add one
                        identifer = "ssnId";
                        autoIncrement = true;
                        tableModels.Append($"{{\"name\": \"ssnId\", \"keyPath\": true, \"autoIncrement\": true, \"unique\": true}},");
                    }
                    string tmpString = tableModels.ToString();
                    tableJsonArray += tmpString
                        .Remove(tmpString.Length - 1, 1)                                                    //remove the last ,
                        .Replace("#1", identifer).Replace("#2", autoIncrement.ToString().ToLower()) +       //set the identity of the table
                        "]},";                                                                              //close json array

                }

                if (haveTables)
                {
                    tableJsonArray = tableJsonArray.Remove(tableJsonArray.Length - 1, 1);     //remove last ,
                    Setup.ModelsAsJson = $"[{tableJsonArray}]";
                    model = model.Replace("[]", $"[{tableJsonArray}]");                  //replace with tables array
                    Setup.DataBaseModelAsJson = model;
                }
                else
                {
                    if (Settings.EnableDebug) Console.WriteLine("{0} => No tables in a DB Model = {1}", Setup.DBName, model);
                }
            }
            catch (Exception ex)
            {
                if (Settings.EnableDebug) Console.WriteLine("{0} => DbInit exception {1}", Setup.DBName, ex.Message);
            }
            finally
            {
                if (Settings.EnableDebug)
                    Console.WriteLine("{0} => DbInit DB Model = {1}", Setup.DBName, model);
            }

        }
    }
}
