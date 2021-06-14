using BlazorIndexedDb.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Helpers
{
    /// <summary>
    /// Internal utilities
    /// </summary>
    class Utils
    {
        /// <summary>
        /// Get class name
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public static string GetName<TModel>()
        {
            Type myType = typeof(TModel);
            return myType.Name;
        }

        /// <summary>
        /// Get object type name in lower case
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static string GetNameToLower(object sender)
        {
            return sender.GetType().Name.ToLower();
        }

        /// <summary>
        /// Get only the name from the type passed if it's from a generic type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetGenericTypeName(Type t)
        {
            if (!t.IsGenericType)
                return t.FullName;

            string genericArgs = string.Join(".",
                t.GetGenericArguments()
                    .Select(ta => GetGenericTypeName(ta)).ToArray());
            string result;
            string[] nom = genericArgs.Split(".", StringSplitOptions.RemoveEmptyEntries);
            if (nom.Length > 0) result = nom[nom.Length - 1];
            else result = genericArgs;
            return result;
        }
    }
}
