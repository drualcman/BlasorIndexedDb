using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Helpers
{
    /// <summary>
    /// Add offline property to the entities to control when the transaction can't be saved into a live db
    /// </summary>
    class AddOflineProperty
    {
        /// <summary>
        /// Add offline property
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="toCopy"></param>
        /// <returns></returns>
        static dynamic AddOffline<TModel>(TModel toCopy)
        {
            dynamic expando = new ExpandoObject();
            // ExpandoObject supports IDictionary so we can extend it like this
            Type t = toCopy.GetType();

            // ExpandoObject supports IDictionary so we can extend it like this
            IDictionary<string, object> expandoDict = expando as IDictionary<string, object>;

            //read all properties
            PropertyInfo[] properties = t.GetProperties(BindingFlags.Public |           //get public names
                                                        BindingFlags.Instance);         //get instance names

            foreach (var item in properties)
            {
                expandoDict.Add(item.Name, item.GetValue(toCopy));
            }
            expando.OffLine = true;
            return expando;
        }

        /// <summary>
        /// Add offline property
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="toCopy"></param>
        /// <returns></returns>
        static List<dynamic> AddOffline<TModel>(List<TModel> toCopy)
        {
            List<dynamic> result = new List<dynamic>();
            foreach (TModel father in toCopy)
            {
                result.Add(AddOffline(father));
            }
            return result;
        }


        /// <summary>
        /// Add offline property
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="toCopy"></param>
        /// <returns></returns>
        public static Task<List<dynamic>> AddOfflineAsync<TModel>(List<TModel> toCopy) =>
            Task.FromResult(AddOffline(toCopy));
    }
}
