using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Data;
using System.Reflection;
using BlazorIndexedDb.Configuration;

namespace BlazorIndexedDb.Helpers
{
    /// <summary>
    /// Tools to work with objects
    /// </summary>
    class ObjectConverter
    {
        #region Methods

        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static string ToJson(System.Collections.IList sender)
        {
            if (Settings.EnableDebug) Console.WriteLine("ToJson parse generic list");
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[");
            foreach (var item in sender)
            {
                if (Settings.EnableDebug) Console.WriteLine("ToJson parse generic list item");
                jsonString.Append(ToJson(item));
                jsonString.Append(",");
            }
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            if (Settings.EnableDebug) Console.WriteLine("ToJson parse System.Collections.IList result = {0}", jsonString.ToString());
            return jsonString.ToString();
        }
        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static string ToJson(List<object> sender)
        {
            if (Settings.EnableDebug) Console.WriteLine("ToJson parse generic list");
            Console.WriteLine("ToJson parse {0}", sender.ToList());
            Console.WriteLine("ToJson parse {0}", sender?.Count);
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[");
            foreach (var item in sender)
            {
                if (Settings.EnableDebug) Console.WriteLine("ToJson parse generic list item");
                jsonString.Append(ToJson(item));
                jsonString.Append(",");
            }
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            if (Settings.EnableDebug) Console.WriteLine("ToJson parse List<object> result = {0}", jsonString.ToString());
            return jsonString.ToString();
        }

        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static string ToJson<TModel>(List<TModel> sender)
        {
            if (Settings.EnableDebug) Console.WriteLine("ToJson parse generic list");
            Console.WriteLine("ToJson parse {0}", sender.ToList());
            Console.WriteLine("ToJson parse {0}", sender?.Count);
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[");
            foreach (TModel item in sender)
            {
                if (Settings.EnableDebug) Console.WriteLine("ToJson parse generic list item");
                jsonString.Append(ToJson(item));
                jsonString.Append(",");
            }
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            if (Settings.EnableDebug) Console.WriteLine("ToJson parse List<TModel> result = {0}", jsonString.ToString());
            return jsonString.ToString();
        }

        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static string ToJson<TModel>(IEnumerable<TModel> sender)
        {
            if (Settings.EnableDebug) Console.WriteLine("ToJson parse IEnumerable");
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[");
            foreach (TModel item in sender)
            {
                if (Settings.EnableDebug) Console.WriteLine("ToJson parse IEnumerable item");
                jsonString.Append(ToJson(item));
                jsonString.Append(",");
            }
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            if (Settings.EnableDebug) Console.WriteLine("ToJson parse IEnumerable result = {0}", jsonString.ToString());
            return jsonString.ToString();
        }

        /// <summary>
        /// Convert into a Json string the object send
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static string ToJson<TModel>(TModel sender)
        {
            StringBuilder jsonString = new StringBuilder();

            Type t = sender.GetType();

            if (IsGenericList(sender))
            {
                if (Settings.EnableDebug) Console.WriteLine("ToJson {0} IsGenericList = true", t.Name);

                System.Collections.IList toSend = sender as System.Collections.IList;
                if (toSend is not null)
                    jsonString.Append(ToJson(toSend));
                else
                {
                    if (Settings.EnableDebug) Console.WriteLine("ToJson {0} is null", t.Name);
                    jsonString.Append($"null");
                }
            }
            else
            {

                //read all properties
                PropertyInfo[] properties = t.GetProperties(BindingFlags.Public |           //get public names
                                                            BindingFlags.Instance);         //get instance names

                jsonString.Append("{");
                for (int i = 0; i < properties.Length; i++)
                {                    
                    PropertyOptions property = new PropertyOptions(properties[i]);
                    bool notInTables = !Settings.Tables.Contains(properties[i].PropertyType.Name) && !Settings.Tables.Contains(Utils.GetGenericTypeName(properties[i].PropertyType));
                    if (!property.ToIgnore && notInTables)
                    {
                        if (Settings.EnableDebug) Console.WriteLine("ToJson parse Property {0} not ignored", properties[i].Name);
                        jsonString.Append($"\"{property.Name}\":");
                        if (IsGenericList(properties[i]))
                        {
                            if (Settings.EnableDebug) Console.WriteLine("ToJson Property {0} IsGenericList = true", properties[i].Name);
                            jsonString.Append(ToJson(properties[i].GetValue(sender)));
                        }
                        else
                        {
                            string pName = properties[i].PropertyType.Name;
                            if (properties[i].GetValue(sender) is null)
                            {
                                jsonString.Append($"null");
                            }
                            else if (properties[i].GetType() == typeof(object))
                            {
                                jsonString.Append(ToJson(properties[i].GetValue(sender)));
                            }
                            else if (pName == typeof(DateTime).Name)
                            {
                                jsonString.Append("\"");
                                jsonString.Append(Convert.ToDateTime(properties[i].GetValue(sender)).ToString("yyyy-MM-dd HH':'mm':'ss"));
                                jsonString.Append("\"");
                            }
                            else if (pName == typeof(bool).Name)
                            {
                                jsonString.Append(Convert.ToBoolean(properties[i].GetValue(sender)) ? "true" : "false");
                            }
                            else if (pName == typeof(string).Name)
                            {
                                jsonString.Append($"\"{properties[i].GetValue(sender).ToString().Replace("\"", "\\\"")}\"");
                            }
                            else if (pName == typeof(String).Name)
                            {
                                jsonString.Append($"\"{properties[i].GetValue(sender).ToString().Replace("\"", "\\\"")}\"");
                            }
                            else if (pName == typeof(Int16).Name ||
                                pName == typeof(Int32).Name ||
                                pName == typeof(Int64).Name ||
                                pName == typeof(Double).Name ||
                                pName == typeof(Decimal).Name ||
                                pName == typeof(Single).Name ||
                                pName == typeof(Byte).Name ||
                                pName == typeof(int).Name ||
                                pName == typeof(float).Name ||
                                pName == typeof(long).Name ||
                                pName == typeof(short).Name)
                            {
                                try
                                {
                                    jsonString.Append($"{properties[i].GetValue(sender)}");
                                }
                                catch
                                {
                                    jsonString.Append("null");
                                }
                            }
                            else
                            {
                                jsonString.Append(ToJson(properties[i].GetValue(sender)));
                            }

                        }
                        jsonString.Append(",");
                    }
                    else
                        if (Settings.EnableDebug) Console.WriteLine("ToJson parse Property {0} ignored", properties[i].Name);
                }
                jsonString.Remove(jsonString.Length - 1, 1);
                jsonString.Append("}");

            }
            return jsonString.ToString();           
        }
        #endregion

        #region async
        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static Task<string> ToJsonAsync<TModel>(List<TModel> sender)
            => Task.FromResult(ToJson(sender));

        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static Task<string> ToJsonAsync<TModel>(IEnumerable<TModel> sender)
            => Task.FromResult(ToJson(sender));
        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static Task<string> ToJsonAsync(object sender)
            => Task.FromResult(ToJson(sender));
        #endregion

        #region helpers
        public static bool IsGenericList(object o)
        {
            Type oType = o.GetType();
            return (o.ToString().Contains("System.Collections.Generic.List") || oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }
        public static bool IsGenericList<T>(object o)
        {
            Type oType = o.GetType();
            return (o.ToString().Contains("System.Collections.Generic.List") || oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<T>)));
        }
        #endregion

    }
}
