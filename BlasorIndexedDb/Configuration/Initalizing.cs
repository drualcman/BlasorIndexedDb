using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Configuration
{
    public static class Initalizing
    {
        /// <summary>
        /// Extent JSRuntime to get a confirm
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="name">database name</param>
        /// <param name="version">database version</param>
        /// <param name="tables">string array with the names of the model classes to serialize</param>
        /// <param name="assemblyName">namespace from the models class</param>
        /// <returns></returns>
        public static async Task DbInit(this IJSRuntime jsRuntime, string name, int version, string[] tables, string assemblyName)
            => await DbInit(jsRuntime, name, version, tables, assemblyName, assemblyName);

        /// <summary>
        /// Extent JSRuntime to get a confirm
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="name">database name</param>
        /// <param name="version">database version</param>
        /// <param name="tables">string array with the names of the model classes to serialize</param>
        /// <param name="entitiesNamespace">namespace from the models class</param>
        /// <returns></returns>
        public static Task DbInit(this IJSRuntime jsRuntime, string name, int version, string[] tables, string assemblyName, string entitiesNamespace)
        {
            Settings.DBName = name;
            Settings.Version = version;
            Settings.AssemblyName = assemblyName;
            Settings.EntitiesNamespace = entitiesNamespace;
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
                        foreach (string table in tables)
                        {
                            StringBuilder tableModels = new StringBuilder();
                            string TableModel = $"{entitiesNamespace}.{table}";

                            // {name: 'table name', options:{keyPath: 'primary id to use', autoIncrement: true/false},
                            Type t = assembly.GetType(TableModel, true, true);
                            tableModels.Append($"{{\"name\": \"{table}\",");

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
                                Helpers.PropertyOptions property = new Helpers.PropertyOptions(properties[i]);
                                if (!property.ToIgnore)
                                {
                                    //  columns: [{name: 'property name', keyPath: true/false, autoIncrement: true/false, unique: true/false}]}
                                    string propName = property.Name.ToLower();
                                    if (table.ToLower() != propName)            //if the property is other table don't do nothing
                                    {
                                        //main field from the model, normalize like Id or ModelNameId or IdModelName
                                        if (propName == "id")
                                        {
                                            identifer = property.Name;
                                            autoIncrement = property.IsAutoIncrement;
                                        }
                                        else if (propName == $"{table.ToLower()}id")
                                        {
                                            identifer = property.Name;
                                            autoIncrement = property.IsKeyPath;
                                        }
                                        else if (propName == $"id{table.ToLower()}")
                                        {
                                            identifer = property.Name;
                                            autoIncrement = property.IsAutoIncrement;
                                        }
                                        else if (propName == $"id{table.ToLower()}s")                 //plural possibility
                                        {
                                            identifer = property.Name;
                                            autoIncrement = property.IsAutoIncrement;
                                        }
                                        else if (propName == $"id{table.ToLower().Remove(table.Length - 1, 1)}")                 //singular possibility
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

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //Console.Write(model);
            _ = jsRuntime.InvokeVoidAsync("MyDb.Init", model);          //don't stop the process
            return Task.CompletedTask;
        }

        public static ValueTask<string> DbConnected(this IJSRuntime jsRuntime) => 
            jsRuntime.InvokeAsync<string>("MyDb.Connected");        
    }
}
