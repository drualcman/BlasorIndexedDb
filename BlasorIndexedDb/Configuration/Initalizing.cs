using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using BlazorIndexedDb.Helpers;
using BlazorIndexedDb.Store;

namespace BlazorIndexedDb.Configuration
{
    /// <summary>
    /// Setup connection with the indexedDb in the browser
    /// </summary>
    sealed class Initalizing
    {
        /// <summary>
        /// Setup a indexedDb in a browser
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <returns></returns>
        public async static Task DbInit([NotNull] IJSRuntime jsRuntime)
        {
            string assemblyName = AppDomain.CurrentDomain.FriendlyName;
            await DbInit(jsRuntime, assemblyName, 1);
        }

        /// <summary>
        /// Setup a indexedDb in a browser
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="settings">database name</param>
        /// <returns></returns>
        public async static Task DbInit([NotNull] IJSRuntime jsRuntime, [NotNull] Settings settings)
        {
            await DbInit(jsRuntime, settings.DBName, settings.Version);
        }

        /// <summary>
        /// Setup a indexedDb in a browser
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="name">database name</param>
        /// <param name="version">database version</param>
        /// <param name="assemblyName">assembly contains the namespaces from the model class to use</param>
        /// <param name="entitiesNamespace">namespace from the models class to use</param>
        /// <param name="tables">string array with the names of the model classes to serialize</param>
        /// <returns></returns>
        [Obsolete("This constructor is not necessary anymore. Use DbInit(name, version)")]
        public async static Task DbInit([NotNull] IJSRuntime jsRuntime, [NotNull] string name,
            int version, [NotNull] string assemblyName, string entitiesNamespace,
            [NotNull] string[] tables)
        {
            await DbInit(jsRuntime, name, version);
        }

        /// <summary>
        /// Setup a indexedDb in a browser
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="name">database name</param>
        /// <param name="version">database version</param>
        /// <returns></returns>
        public async static Task DbInit([NotNull] IJSRuntime jsRuntime, [NotNull] string name,  int version)
        {
            if (Settings.EnableDebug) Console.WriteLine($"DbInit need be initialized? {(Settings.Initialized ? "NO":"YES")}");
            if (!Settings.Initialized)
            {
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
                        .Where(s => s.IsAssignableTo(typeof(StoreContext)))
                        .SelectMany(p => p.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        .Where(t => t.PropertyType.IsGenericType && t.PropertyType.GetGenericTypeDefinition().IsAssignableTo(typeof(StoreSet<>)));

                    string tableJsonArray = string.Empty;
                    bool haveTables = false;
                    foreach (PropertyInfo item in storeSets)
                    {
                        Console.WriteLine($"table name {item.Name}");
                        string tableName = item.Name;
                        Settings.Tables.AddTable(tableName, item.PropertyType.GetGenericArguments()[0].Name);
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
                                    if (propName == "id")
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
                                    else if (propName == "Id")
                                    {
                                        identifer = property.Name;
                                        autoIncrement = property.IsAutoIncrement;
                                    }
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
                                    else if (propName == "ID")
                                    {
                                        identifer = property.Name;
                                        autoIncrement = property.IsAutoIncrement;
                                    }
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

                    tableJsonArray = tableJsonArray.Remove(tableJsonArray.Length - 1, 1);     //remove last ,
                    model = model.Replace("[]", "[" + tableJsonArray + "]");                  //replace with tables array

                    if (haveTables)
                    {
                        if (Settings.EnableDebug) Console.WriteLine("DbInit DB Model = {0}", model);
                        await jsRuntime.InvokeVoidAsync("MyDb.Init", model);
                        Settings.Initialized = true;
                    }
                    else
                    {
                        Settings.Initialized = false;
                        if (Settings.EnableDebug) Console.WriteLine("No tables in a DB Model = {0}", model);
                    }
                }
                catch (Exception ex)
                {
                    if (Settings.EnableDebug) Console.WriteLine("DbInit exception {0}", ex.Message);
                    Settings.Initialized = false;
                }
                finally
                {
                    if (Settings.EnableDebug) Console.WriteLine("DbInit finished with Settings.Initiallezed = {0}", Settings.Initialized);
                }
            }
        }
    }
}
