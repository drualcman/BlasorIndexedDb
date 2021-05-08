using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Helpers
{
    public class Utils
    {
        /// <summary>
        /// Get class name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetName<T>()
        {
            Type myType = typeof(T);
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
