using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlasorIndexedDb.Helpers
{
    public class PropertyOptions
    {
        public string Name { get; set; }
        public bool ToIgnore { get; set; }
        public bool IsRequired { get; set; }
        public bool IsAutoIncrement { get; set; }

        public PropertyOptions(PropertyInfo p)
        {
            this.Name = p.Name;
            IEnumerable<Attribute> attrs = p.GetCustomAttributes();  // Reflection.  
            // Displaying output.  
            foreach (Attribute attr in attrs)
            {
                if (attr is System.ComponentModel.DataAnnotations.RequiredAttribute)
                {
                    this.IsAutoIncrement = true;
                    string t = p.GetMethod.ReturnType.Name.ToLower();
                    switch (t)
                    {
                        case "int":
                        case "int16":
                        case "int32":
                        case "int64":
                            this.IsAutoIncrement = true;
                            break;
                        default:
                            this.IsAutoIncrement = false;
                            break;
                    }
                }
                if (attr is System.Text.Json.Serialization.JsonIgnoreAttribute) this.ToIgnore = true;
            }
        }
    }
}
