using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlasorIndexedDb.Helpers
{
    public class AddOflineProperty
    {
        public static dynamic Add<T>(T toCopy)
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

        public static List<dynamic> Add<T>(List<T> toCopy)
        {
            List<dynamic> result = new List<dynamic>();
            foreach (T father in toCopy)
            {
                result.Add(Add(father));
            }
            return result;
        }
    }
}
