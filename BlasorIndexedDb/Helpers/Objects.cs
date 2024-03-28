namespace BlazorIndexedDb.Helpers
{
    /// <summary>
    /// Tools to work with objects
    /// </summary>
    internal sealed class ObjectConverter
    {
        readonly Settings Settings;

        internal ObjectConverter(Settings settings)
        {
            Settings = settings;
        }


        #region Methods

        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal string ToJson(System.Collections.IList sender)
        {
            if(Settings.EnableDebug) Console.WriteLine($"ToJson parse generic list");
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[ ");
            int c = sender.Count;
            for(int i = 0; i < c; i++)
            {
                if(Settings.EnableDebug) Console.WriteLine($"ToJson parse generic list {sender[i].GetType().Name}");
                jsonString.Append(ToJson(sender[i]));
                jsonString.Append(",");
            }
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            if(Settings.EnableDebug) Console.WriteLine("ToJson parse System.Collections.IList result = {0}", jsonString.ToString());
            return jsonString.ToString();
        }

        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal string ToJson(List<object> sender) =>
            ToJson(sender.AsEnumerable());

        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal string ToJson<TModel>(List<TModel> sender) =>
            ToJson(sender.AsEnumerable());

        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal string ToJson<TModel>(IEnumerable<TModel> sender)
        {
            if(Settings.EnableDebug) Console.WriteLine($"ToJson parse IEnumerable list {typeof(TModel).Name}");
            StringBuilder jsonString = new StringBuilder();
            jsonString.Append("[ ");
            foreach(TModel item in sender)
            {
                if(Settings.EnableDebug) Console.WriteLine($"ToJson parse IEnumerable item {item.GetType().Name}");
                jsonString.Append(ToJson(item));
                jsonString.Append(",");
            }
            jsonString.Remove(jsonString.Length - 1, 1);
            jsonString.Append("]");
            if(Settings.EnableDebug) Console.WriteLine($"ToJson parse IEnumerable result {jsonString}");
            return jsonString.ToString();
        }

        /// <summary>
        /// Convert into a Json string the object send
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal string ToJson<TModel>(TModel sender)
        {
            StringBuilder jsonString = new StringBuilder();

            Type t = sender.GetType();

            if(Settings.EnableDebug) Console.WriteLine($"ToJson parse model {typeof(TModel).Name}");

            if(IsGenericList(sender))
            {
                if(Settings.EnableDebug) Console.WriteLine("ToJson {0} IsGenericList = true", t.Name);

                System.Collections.IList toSend = sender as System.Collections.IList;
                if(toSend is not null)
                    jsonString.Append(ToJson(toSend));
                else
                {
                    if(Settings.EnableDebug) Console.WriteLine("ToJson {0} is null", t.Name);
                    jsonString.Append($"null");
                }
            }
            else
            {
                //read all properties
                PropertyInfo[] properties = GetProperties(t);

                jsonString.Append("{ ");
                for(int i = 0; i < properties.Length; i++)
                {
                    PropertyOptions property = new PropertyOptions(properties[i]);
                    if(!property.ToIgnore)
                    {
                        if(Settings.Tables.InTables<TModel>())
                        {
                            if(Settings.EnableDebug) Console.WriteLine("ToJson parse Property {0} not ignored", properties[i].Name);
                            jsonString.Append($"\"{property.Name}\":");
                            if(IsGenericList(properties[i]))
                            {
                                if(Settings.EnableDebug) Console.WriteLine("ToJson Property {0} IsGenericList = true", properties[i].Name);
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
                            if(!string.IsNullOrEmpty(property.FieldName))
                            {
                                jsonString.Append($"\"{property.FieldName}\":");
                                jsonString.Append(ValueToString(sender, property.StoreIndexName, properties[i]));
                            }
                            else
                            {
                                //is not other table parse the model directly
                                jsonString.Append($"\"{property.Name}\":");
                                jsonString.Append(ValueToString(sender, properties[i], properties[i].PropertyType.Name));
                            }
                        }
                        jsonString.Append(",");
                    }
                    else
                        if(Settings.EnableDebug) Console.WriteLine("ToJson parse Property {0} ignored", properties[i].Name);
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
        internal Task<string> ToJsonAsync<TModel>(List<TModel> sender)
            => Task.FromResult(ToJson(sender));

        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal Task<string> ToJsonAsync<TModel>(IEnumerable<TModel> sender)
            => Task.FromResult(ToJson(sender));
        /// <summary>
        /// Convert into a Json string the model send
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        internal Task<string> ToJsonAsync(object sender)
            => Task.FromResult(ToJson(sender));
        #endregion

        #region helpers
        internal PropertyInfo[] GetProperties(Type t)
        {
            //read all properties
            return t.GetProperties(BindingFlags.Public |           //get public names
                                   BindingFlags.Instance);         //get instance names
        }
        internal bool IsGenericList(object o)
        {
            Type oType = o.GetType();
            return (o.ToString().Contains("System.Collections.Generic.List") || oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }
        internal bool IsGenericList<T>(object o)
        {
            Type oType = o.GetType();
            return (o.ToString().Contains("System.Collections.Generic.List") || oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<T>)));
        }
        #endregion


        internal string ToRepeat<TModel>(TModel sender)
        {
            StringBuilder jsonString = new StringBuilder();

            Type t = sender.GetType();

            if(Settings.EnableDebug) Console.WriteLine($"ToJson parse model {typeof(TModel).Name}");

            if(IsGenericList(sender))
            {
                if(Settings.EnableDebug) Console.WriteLine("ToJson {0} IsGenericList = true", t.Name);

                System.Collections.IList toSend = sender as System.Collections.IList;
                if(toSend is not null)
                    jsonString.Append(ToJson(toSend));
                else
                {
                    if(Settings.EnableDebug) Console.WriteLine("ToJson {0} is null", t.Name);
                    jsonString.Append($"null");
                }
            }
            else
            {

                //read all properties
                PropertyInfo[] properties = GetProperties(t);

                jsonString.Append("{ ");
                for(int i = 0; i < properties.Length; i++)
                {
                    PropertyOptions property = new PropertyOptions(properties[i]);
                    if(!property.ToIgnore)
                    {
                        if(Settings.EnableDebug) Console.WriteLine("ToJson parse Property {0} not ignored", properties[i].Name);
                        if(Settings.Tables.InTables<TModel>())
                        {
                            jsonString.Append($"\"{property.Name}\":");
                            if(IsGenericList(properties[i]))
                            {
                                if(Settings.EnableDebug) Console.WriteLine("ToJson Property {0} IsGenericList = true", properties[i].Name);
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
                            if(!string.IsNullOrEmpty(property.FieldName))
                            {
                                jsonString.Append($"\"{property.FieldName}\":");
                                jsonString.Append(ValueToString(sender, property.StoreIndexName, properties[i]));
                            }
                        }
                        jsonString.Append(",");
                    }
                    else
                        if(Settings.EnableDebug) Console.WriteLine("ToJson parse Property {0} ignored", properties[i].Name);
                }
                jsonString.Remove(jsonString.Length - 1, 1);
                jsonString.Append("}");

            }
            return jsonString.ToString();
        }

        string ValueToString(object sender, PropertyInfo property, string pName)
        {
            StringBuilder result = new StringBuilder();

            if(property.GetValue(sender) is null)
            {
                result.Append("null");
            }
            else
            {
                bool isEnum = property.GetValue(sender).GetType().IsEnum;
                if(!isEnum)
                {
                    if(property.PropertyType == typeof(object))
                    {
                        result.Append(ToJson(property.GetValue(sender)));
                    }
                    else if(pName == typeof(DateTime).Name)
                    {
                        result.Append("\"");
                        result.Append(Convert.ToDateTime(property.GetValue(sender)).ToString("o"));
                        result.Append("\"");
                    }
                    else if(pName == typeof(bool).Name)
                    {
                        result.Append(Convert.ToBoolean(property.GetValue(sender)) ? "true" : "false");
                    }
                    else if(pName == typeof(string).Name)
                    {
                        result.Append($"\"{property.GetValue(sender).ToString().Replace("\"", "\\\"").Replace("\t", "\\t").Replace("\r", "\\r")?.Replace("\n", "\\n").Replace(Environment.NewLine, "\\n")}\"");
                    }
                    else if(pName == typeof(Int16).Name ||
                        pName == typeof(Int32).Name ||
                        pName == typeof(Int64).Name ||
                        pName == typeof(Double).Name ||
                        pName == typeof(Decimal).Name ||
                        pName == typeof(Single).Name ||
                        pName == typeof(Byte).Name ||
                        pName == typeof(int).Name ||
                        pName == typeof(double).Name ||
                        pName == typeof(float).Name ||
                        pName == typeof(long).Name ||
                        pName == typeof(decimal).Name ||
                        pName == typeof(short).Name)
                    {
                        try
                        {
                            result.Append($"{property.GetValue(sender)}");
                        }
                        catch
                        {
                            result.Append("0");
                        }
                    }
                    else
                    {
                        result.Append(ToJson(property.GetValue(sender)));
                    }
                }
                else
                {
                    int e = (int)property.GetValue(sender);
                    result.Append(e.ToString());
                }
            }
            return result.ToString();
        }
        string ValueToString(object sender, string pValue, PropertyInfo property)
        {
            var field = property.GetValue(sender);

            if(!IsGenericList(field))
            {
                PropertyInfo[] properties = GetProperties(field.GetType());
                PropertyInfo prop = properties.Where(p => p.Name == pValue).FirstOrDefault();
                return ValueToString(field, prop, prop.PropertyType.Name);
            }
            else
            {
                StringBuilder jsonString = new StringBuilder();
                jsonString.Append("[ ");
                foreach(var item in field as System.Collections.IList)
                {
                    PropertyInfo[] properties = GetProperties(item.GetType());
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
