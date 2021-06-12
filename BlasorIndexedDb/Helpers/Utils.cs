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
            string name = myType.Name;
            return name;
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
    }
}
