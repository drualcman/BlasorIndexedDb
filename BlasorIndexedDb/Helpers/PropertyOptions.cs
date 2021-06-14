using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Helpers
{
    /// <summary>
    /// Get the options from the attributes setup into the model
    /// </summary>
    class PropertyOptions
    {
        public string Name { get; set; }
        public bool ToIgnore { get; set; }
        public bool IsKeyPath { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsUnique { get; set; }

        /// <summary>
        /// Get all the indexdb attributes from the porperty
        /// </summary>
        /// <param name="p"></param>
        public PropertyOptions(PropertyInfo p)
        {            
            this.Name = p.Name;
            IEnumerable<Attribute> attrs = p.GetCustomAttributes();  // Reflection.  
            // Displaying output.  
            foreach (Attribute attr in attrs)
            {
                if (attr is System.Text.Json.Serialization.JsonIgnoreAttribute) this.ToIgnore = true;
                else if (attr is Attributes.FieldAttribute)
                {
                    Attributes.FieldAttribute a = attr as Attributes.FieldAttribute;
                    this.IsKeyPath = a.IsKeyPath;
                    this.IsAutoIncrement = a.IsAutoIncremental;
                    this.ToIgnore = a.IsIgnore;
                    this.IsUnique = a.IsUnique;
                }
                else if (attr is System.ComponentModel.DataAnnotations.RequiredAttribute)
                {
                    this.IsKeyPath = true;
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
                else 
                {
                    this.IsKeyPath = false;
                    this.IsAutoIncrement = false;
                    this.ToIgnore = false;
                    this.IsUnique = false;
                }
                
            }
        }
    }
}
