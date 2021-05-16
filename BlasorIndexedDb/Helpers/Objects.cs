using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Data;
using System.Reflection;

namespace BlazorIndexedDb.Helpers
{
    public static class ObjectConverter
    {
        #region Methods
        public static string ToJson<T>(List<T> sender)
        {
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[");
            foreach (T item in sender)
            {
                jsonString.Append(ToJson(item));
                jsonString.Append(",");
            }
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            return jsonString.ToString();
        }

        public static string ToJson<T>(IEnumerable<T> sender) 
        {
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[");
            foreach (T item in sender)
            {
                jsonString.Append(ToJson(item));
                jsonString.Append(",");
            }
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            return jsonString.ToString();
        }

        public static string ToJson(object sender)
        {
            StringBuilder jsonString = new StringBuilder();

            Type t = sender.GetType();

            if (IsGenericList(sender))
            {
                List<object> toSend = sender as List<object>;
                jsonString.Append(ToJson(toSend));                
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
                    if (!property.ToIgnore)
                    {
                        string propName = property.Name.ToLower();
                        jsonString.Append($"\"{property.Name}\":");

                        string pName = properties[i].PropertyType.Name;
                        if (ConfigData.Tables.Contains(pName))
                            jsonString.Append(ToJson(properties[i].GetValue(sender)));
                        else
                        {
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
                                jsonString.Append(Convert.ToDateTime(properties[i].GetValue(sender)).ToString("yyyy-MM-dd HH':'mm':'ss"));
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
                                jsonString.Append($"\"{properties[i].GetValue(sender).ToString().Replace("\"", "\\\"")}\"");
                            }
                        }
                        jsonString.Append(",");
                    }
                }
                jsonString.Remove(jsonString.Length - 1, 1);
                jsonString.Append("}");

            }
            return jsonString.ToString();           
        }
        #endregion

        #region async
        public static Task<string> ToJsonAsync(object sender)
            => Task.FromResult(ToJson(sender));
        #endregion

        #region helpers
        public static bool IsGenericList(object o)
        {
            var oType = o.GetType();
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }
        public static bool IsGenericList<T>(object o)
        {
            var oType = o.GetType();
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<T>)));
        }
        #endregion

    }
}
