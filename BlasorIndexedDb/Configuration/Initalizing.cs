using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using BlazorIndexedDb.Helpers;

namespace BlazorIndexedDb.Configuration
{
    /// <summary>
    /// Setup de connection with the indexedDb in the browser
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
            await DbInit(jsRuntime, assemblyName, 1, assemblyName,
                string.Empty, Settings.Tables);
        }

        /// <summary>
        /// Setup a indexedDb in a browser
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="settings">database name</param>
        /// <returns></returns>
        public async static Task DbInit([NotNull] IJSRuntime jsRuntime, [NotNull] Settings settings) 
        {
            await DbInit(jsRuntime, settings.DBName, settings.Version, settings.AssemblyName, 
                settings.EntitiesNamespace, Settings.Tables);
        }

        /// <summary>
        /// Setup a indexedDb in a browser
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="name">database name</param>
        /// <param name="version">database version</param>
        /// <param name="assemblyName">assembly contenies the namespaces from the model class to use</param>
        /// <param name="entitiesNamespace">namespace from the models class to use</param>
        /// <param name="tables">string array with the names of the model classes to serialize</param>
        /// <returns></returns>
        public async static Task DbInit([NotNull] IJSRuntime jsRuntime, [NotNull] string name,
            int version, [NotNull] string assemblyName, string entitiesNamespace,
            [NotNull] string[] tables)
        {

            if (Settings.EnableDebug) Console.WriteLine($"DbInit need be initiallized? {(Settings.Initiallezed ? "NO":"YES")}");
            if (!Settings.Initiallezed) 
            {
                if (tables.Length >= 0)
                {
                    Settings.Tables = tables;
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
                        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                        int c = 0;
                        do
                        {
                            if (assemblies[c].GetName().Name == assemblyName)
                            {
                                Assembly assembly = assemblies[c];

                                string tableJsonArray = string.Empty;
                                int tc = tables.Length;
                                for (int tci = 0; tci < tc; tci++)
                                {
                                    StringBuilder tableModels = new StringBuilder();
                                    string TableModel = $"{entitiesNamespace}.{tables[tci]}";
                                    // {name: 'table name', options:{keyPath: 'primary id to use', autoIncrement: true/false},
                                    Type t = assembly.GetType(TableModel, true, true);
                                    tableModels.Append($"{{\"name\": \"{tables[tci]}\",");

                                    //read all properties
                                    PropertyInfo[] properties = t.GetProperties(BindingFlags.Public |           //get public names
                                                                                BindingFlags.Instance);         //get instance names

                                    //main field from the model, normalize like Id or ModelNameId or IdModelName
                                    string identifer = "ssn";
                                    bool autoIncrement = false;     //if it's a integer must be true
                                    tableModels.Append("\"options\": {\"keyPath\": \"#1\", \"autoIncrement\": #2},");
                                    //add columns name from the properties names
                                    tableModels.Append($"\"columns\": [");                  //open json array

                                    for (int i = 0; i < properties.Length; i++)
                                    {
                                        PropertyOptions property = new PropertyOptions(properties[i]);
                                        if (!property.ToIgnore)
                                        {
                                            //if the property is other table don't do nothing
                                            bool notInTables = !Utils.InTables(properties[i]);
                                            //  columns: [{name: 'property name', keyPath: true/false, autoIncrement: true/false, unique: true/false}]}                                            
                                            string propName = property.Name.ToLower();
                                            Console.WriteLine("DbInit notInTables {0} propName {1}", notInTables, propName);
                                            if (notInTables)
                                            {
                                                //main field from the model, normalize like Id or ModelNameId or IdModelName
                                                if (propName == "id")
                                                {
                                                    identifer = property.Name;
                                                    autoIncrement = property.IsAutoIncrement;
                                                }
                                                else if (propName == $"{tables[tci].ToLower()}id")
                                                {
                                                    identifer = property.Name;
                                                    autoIncrement = property.IsKeyPath;
                                                }
                                                else if (propName == $"id{tables[tci].ToLower()}")
                                                {
                                                    identifer = property.Name;
                                                    autoIncrement = property.IsAutoIncrement;
                                                }
                                                else if (propName == $"id{tables[tci].ToLower()}s")                 //plural possibility
                                                {
                                                    identifer = property.Name;
                                                    autoIncrement = property.IsAutoIncrement;
                                                }
                                                else if (propName == $"id{tables[tci].ToLower().Remove(tables[tci].Length - 1, 1)}")                 //singular possibility
                                                {
                                                    identifer = property.Name;
                                                    autoIncrement = property.IsAutoIncrement;
                                                }
                                                else if (property.IsKeyPath)
                                                {
                                                    identifer = property.Name;
                                                    autoIncrement = property.IsAutoIncrement;
                                                }
                                                else tableModels.Append($"{{\"name\": \"{property.Name}\", \"keyPath\": {property.IsKeyPath.ToString().ToLower()}, \"autoIncrement\": {property.IsAutoIncrement.ToString().ToLower()}, \"unique\": {property.IsUnique.ToString().ToLower()}}},");
                                            }
                                            else
                                            {
                                                Console.WriteLine("DbInit fieldname {0} storeindexname {1}", property.FieldName, property.StoreIndexName);
                                                //because is other table check if have a relationship
                                                if (!string.IsNullOrEmpty(property.FieldName))
                                                {
                                                    tableModels.Append($"{{\"name\": \"{property.FieldName}\", \"keyPath\": false, \"autoIncrement\": false, \"unique\": {property.IsUnique.ToString().ToLower()}}},");
                                                }
                                            }
                                        }
                                    }
                                    //always add control if the register it's offline NULL = not offline, only when it's true it's applicable
                                    tableModels.Append($"{{\"name\": \"OffLine\", \"keyPath\": false, \"autoIncrement\": false, \"unique\": false}},");
                                    if (string.IsNullOrEmpty(identifer)) identifer = "ssn";
                                    string tmpString = tableModels.ToString();
                                    tableJsonArray += tmpString
                                        .Remove(tmpString.Length - 1, 1)                                                    //remove the last ,
                                        .Replace("#1", identifer).Replace("#2", autoIncrement.ToString().ToLower()) +       //set the identity of the table
                                        "]},";                                                                              //close json array
                                }
                                tableJsonArray = tableJsonArray.Remove(tableJsonArray.Length - 1, 1);     //remove last ,
                                model = model.Replace("[]", "[" + tableJsonArray + "]");                  //replace with tables array

                                c = assemblies.Length;
                            }
                            c++;
                        } while (c < assemblies.Length);

                        Settings.Initiallezed = true;
                        if (Settings.EnableDebug) Console.WriteLine("DbInit DB Model = {0}", model);
                        await jsRuntime.InvokeVoidAsync("MyDb.Init", model);
                    }
                    catch (Exception ex)
                    {
                        if (Settings.EnableDebug) Console.WriteLine("DbInit exception {0}", ex.Message);
                        Settings.Initiallezed = false;
                    }
                    finally
                    {
                        if (Settings.EnableDebug) Console.WriteLine("DbInit finished with Settings.Initiallezed = {0}", Settings.Initiallezed);
                    }
                }
                else
                    Console.WriteLine("DbInit error: Number of tables is {0}", tables.Length);
            }
        }
    }
}
