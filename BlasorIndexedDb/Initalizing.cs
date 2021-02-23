using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlasorIndexedDb
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
        /// <param name="mynamespace">namespace from the models class</param>
        /// <returns></returns>
        public static ValueTask DbInit(this IJSRuntime jsRuntime, string name, int version, string[] tables, string mynamespace)
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
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                
                int c = 0;
                do
                {
                    if (assemblies[c].GetName().Name == mynamespace)
                    {
                        Assembly assembly = assemblies[c];

                        string tableJsonArray = string.Empty;
                        foreach (string table in tables)
                        {
                            StringBuilder tableModels = new StringBuilder();
                            string TableModel = $"{mynamespace}.{table}";

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
                                            identifer += property.Name;
                                            autoIncrement = property.IsAutoIncrement;
                                        }
                                        else if (propName == $"{table.ToLower()}id")
                                        {
                                            identifer += property.Name;
                                            autoIncrement = property.IsRequired;
                                        }
                                        else if (propName == $"id{table.ToLower()}")
                                        {
                                            identifer += property.Name;
                                            autoIncrement = property.IsAutoIncrement;
                                        }
                                        else if (propName == $"id{table.ToLower()}s")                 //plural possibility
                                        {
                                            identifer += property.Name;
                                            autoIncrement = property.IsAutoIncrement;
                                        }
                                        else if (propName == $"id{table.ToLower().Remove(table.Length - 1, 1)}")                 //singular possibility
                                        {
                                            identifer += property.Name;
                                            autoIncrement = property.IsAutoIncrement;
                                        }
                                        tableModels.Append($"{{\"name\": \"{property.Name}\", \"keyPath\": {property.IsRequired.ToString().ToLower()}, \"autoIncrement\": {property.IsAutoIncrement.ToString().ToLower()}, \"unique\": {property.IsRequired.ToString().ToLower()}}},");
                                    }
                                }
                            }

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
            Console.Write(model);
            return jsRuntime.InvokeVoidAsync("MyDb.Init", model);
        }

        public static ValueTask<string> DbConnected(this IJSRuntime jsRuntime) => jsRuntime.InvokeAsync<string>("MyDb.Init");
    }
}
