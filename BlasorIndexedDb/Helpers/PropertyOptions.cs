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
        public string StoreIndexName { get; set; }
        public string FieldName { get; set; }


        /// <summary>
        /// Get all the indexdb attributes from the porperty
        /// </summary>
        /// <param name="p"></param>
        public PropertyOptions(PropertyInfo p) 
        {      
            string tableName = p.ReflectedType.Name;
            this.Name = p.Name;
            IEnumerable<Attribute> attrs = p.GetCustomAttributes();  // Reflection.  
            if (attrs.Any())
            {
                // Displaying output.  
                foreach (Attribute attr in attrs)
                {
                    //check main field attributes
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
                        this.IsUnique = true;
                    }
                    else
                    {
                        this.IsKeyPath = false;
                        this.IsAutoIncrement = false;
                        this.ToIgnore = false;
                        this.IsUnique = false;
                    }
                    //check relationship attribute
                    if (attr is Attributes.RelationAttribute)
                    {
                        Attributes.RelationAttribute a = attr as Attributes.RelationAttribute;
                        this.StoreIndexName = a.StoreIndexName;
                        this.FieldName = a.FieldName;
                    }
                    else
                    {
                        this.StoreIndexName = string.Empty;
                        this.FieldName = string.Empty;
                    }
                }
            }
            else
            {
                this.ToIgnore = false;
                if (this.Name.ToLower() == "id")
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                }
                else if (this.Name == $"{tableName.ToLower()}id")
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                }
                else if (this.Name == $"id{tableName.ToLower()}")
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                }
                else if (this.Name == $"id{tableName.ToLower()}s")                 //plural possibility
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                }
                else if (this.Name == $"id{tableName.ToLower().Remove(tableName.Length - 1, 1)}")                 //singular possibility
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                } //next
                else if (this.Name == $"{tableName.ToLower()}Id")
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                }
                else if (this.Name == $"Id{tableName.ToLower()}")
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                }
                else if (this.Name == $"Id{tableName.ToLower()}s")                 //plural possibility
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                }
                else if (this.Name == $"Id{tableName.ToLower().Remove(tableName.Length - 1, 1)}")                 //singular possibility
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;

                } //last
                else if (this.Name == $"{tableName.ToLower()}ID")
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                }
                else if (this.Name == $"ID{tableName.ToLower()}")
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                }
                else if (this.Name == $"ID{tableName.ToLower()}s")                 //plural possibility
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;
                }
                else if (this.Name == $"ID{tableName.ToLower().Remove(tableName.Length - 1, 1)}")                 //singular possibility
                {
                    this.IsKeyPath = true;
                    this.IsUnique = true;                     
                }
                else
                {
                    this.IsKeyPath = false;
                    this.IsAutoIncrement = false;
                    this.IsUnique = false;
                }
            }
            if (this.IsKeyPath && this.IsUnique)
            {
                Type t = p.GetMethod.ReturnType;
                if (t == typeof(Int16)) this.IsAutoIncrement = true;
                else if (t == typeof(Int32)) this.IsAutoIncrement = true;
                else if (t == typeof(Int64)) this.IsAutoIncrement = true;
                else if (t == typeof(Double)) this.IsAutoIncrement = true;
                else if (t == typeof(Decimal)) this.IsAutoIncrement = true;
                else if (t == typeof(UInt16)) this.IsAutoIncrement = true;
                else if (t == typeof(UInt32)) this.IsAutoIncrement = true;
                else if (t == typeof(UInt64)) this.IsAutoIncrement = true;
                else this.IsAutoIncrement = false;
            }
        }
    }
}
