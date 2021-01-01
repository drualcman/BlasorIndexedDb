using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlasorIndexedDb.Helpers
{
    public class Utils
    {
        public static string GetName<T>()
        {
            Type myType = typeof(T);
            string name = myType.Name;
            return name;
        }
    }
}
