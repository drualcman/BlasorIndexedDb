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
            if (Settings.EnableDebug) Console.WriteLine($"ToJson parse generic list");
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[ ");
            int c = sender.Count;
            for (int i = 0; i < c; i++)
            {
                if (Settings.EnableDebug) Console.WriteLine($"ToJson parse generic list {sender[i].GetType().Name}");
                jsonString.Append(ToJson(sender[i]));
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
            if (Settings.EnableDebug) Console.WriteLine($"ToJson parse generic list {typeof(object).Name}");
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[ ");
            int c = sender.Count;
            for (int i = 0; i < c; i++)
            {
                if (Settings.EnableDebug) Console.WriteLine($"ToJson parse generic list {sender[i].GetType().Name}");
                jsonString.Append(ToJson(sender[i]));
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
            if (Settings.EnableDebug) Console.WriteLine($"ToJson parse generic list {typeof(TModel).Name}");
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[ ");
            int c = sender.Count;
            for (int i = 0; i < c; i++)
            {
                if (Settings.EnableDebug) Console.WriteLine($"ToJson parse generic list {sender[i].GetType().Name}");
                jsonString.Append(ToJson(sender[i]));
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
            if (Settings.EnableDebug) Console.WriteLine($"ToJson parse IEnumerable {typeof(TModel).Name}");
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[ ");
            foreach (TModel item in sender)
            {
                if (Settings.EnableDebug) Console.WriteLine($"ToJson parse IEnumerable {item.GetType().Name}");
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

            if (Settings.EnableDebug) Console.WriteLine($"ToJson parse model {typeof(TModel).Name} name {t.Name}");

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

                jsonString.Append("{ ");
                for (int i = 0; i < properties.Length; i++)
                {                    
                    PropertyOptions property = new PropertyOptions(properties[i]);
                    if (!property.ToIgnore)
                    {
                        bool notInTables = !Settings.Tables.Contains(properties[i].PropertyType.Name) && !Settings.Tables.Contains(Utils.GetGenericTypeName(properties[i].PropertyType));
                        if (notInTables)
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
                                jsonString.Append(ValueToString(sender, properties[i], properties[i].PropertyType.Name));
                            }
                        }
                        else
                        {
                            //because is other table check if have a relationship
                            if (!string.IsNullOrEmpty(property.FieldName))
                            {
                                jsonString.Append($"\"{property.FieldName}\":");
                                jsonString.Append(ValueToString(sender, property.StoreIndexName, properties[i]));
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
            Console.WriteLine(o.ToString());
            Console.WriteLine(oType.Name);
            return (o.ToString().Contains("System.Collections.Generic.List") || oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }
        public static bool IsGenericList<T>(object o)
        {
            Type oType = o.GetType();
            return (o.ToString().Contains("System.Collections.Generic.List") || oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<T>)));
        }
        #endregion


        public static string ToRepeat<TModel>(TModel sender)
        {
            StringBuilder jsonString = new StringBuilder();

            Type t = sender.GetType();

            if (Settings.EnableDebug) Console.WriteLine($"ToJson parse model {typeof(TModel).Name} name {t.Name}");

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

                jsonString.Append("{ ");
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyOptions property = new PropertyOptions(properties[i]);
                    if (!property.ToIgnore)
                    {
                        if (Settings.EnableDebug) Console.WriteLine("ToJson parse Property {0} not ignored", properties[i].Name);
                        bool notInTables = !Settings.Tables.Contains(properties[i].PropertyType.Name) && !Settings.Tables.Contains(Utils.GetGenericTypeName(properties[i].PropertyType));
                        if (notInTables)
                        {
                            jsonString.Append($"\"{property.Name}\":");
                            if (IsGenericList(properties[i]))
                            {
                                if (Settings.EnableDebug) Console.WriteLine("ToJson Property {0} IsGenericList = true", properties[i].Name);
                                jsonString.Append(ToJson(properties[i].GetValue(sender)));
                            }
                            else
                            {                                
                                jsonString.Append(ValueToString(sender, properties[i], properties[i].PropertyType.Name));
                            }
                        }
                        else
                        {
                            //because is other table check if have a relationship
                            if (!string.IsNullOrEmpty(property.FieldName))
                            {
                                jsonString.Append($"\"{property.FieldName}\":");
                                jsonString.Append(ValueToString(sender, property.StoreIndexName, properties[i]));
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

        static string ValueToString(object sender, PropertyInfo property, string pName)
        {
            string result;
            if (property.GetValue(sender) is null)
            {
                result = $"null";
            }
            else if (property.GetType() == typeof(object))
            {
                result = ToJson(property.GetValue(sender));
            }
            else if (pName == typeof(DateTime).Name)
            {
                result = "\"";
                result += Convert.ToDateTime(property.GetValue(sender)).ToString("yyyy-MM-dd HH':'mm':'ss");
                result += "\"";
            }
            else if (pName == typeof(bool).Name)
            {
                result = Convert.ToBoolean(property.GetValue(sender)) ? "true" : "false";
            }
            else if (pName == typeof(string).Name)
            {
                result = $"\"{property.GetValue(sender).ToString().Replace("\"", "\\\"")}\"";
            }
            else if (pName == typeof(String).Name)
            {
                result = $"\"{property.GetValue(sender).ToString().Replace("\"", "\\\"")}\"";
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
                    result = $"{property.GetValue(sender)}";
                }
                catch
                {
                    result = "null";
                }
            }
            else
            {
                result = ToJson(property.GetValue(sender));
            }
            return result;
        }
        static string ValueToString(object sender, string pValue, PropertyInfo property)
        {
            var field = property.GetValue(sender);

            if (!IsGenericList(field))
            {
                PropertyInfo[] properties = field.GetType().GetProperties();
                PropertyInfo prop = properties.Where(p => p.Name == pValue).FirstOrDefault();
                return ValueToString(field, prop, prop.PropertyType.Name);
            }
            else
            {
                StringBuilder jsonString = new StringBuilder();
                jsonString.Append("[ ");
                foreach (var item in field as System.Collections.IList)
                {
                    PropertyInfo[] properties = item.GetType().GetProperties();
                    PropertyInfo prop = properties.Where(p => p.Name == pValue).FirstOrDefault();
                    jsonString.Append(ValueToString(item, prop, prop.PropertyType.Name));
                    jsonString.Append(",");
                }
                jsonString.Remove(jsonString.Length - 1, 1);
                jsonString.Append("]");
                return jsonString.ToString();
            }

        }
    }
}
