using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Attributes
{
    /// <summary>
    /// Custom Attribute to setup the model
    /// </summary>
    public class IndexDb : Attribute
    {
        /// <summary>
        /// If the proerty is a keypath or requered
        /// </summary>
        public bool IsKeyPath { get; set; } = false;
        /// <summary>
        /// If the property is auto incremental
        /// </summary>
        public bool IsAutoIncremental { get; set; } = false;
        /// <summary>
        /// If the property are unique
        /// </summary>
        public bool IsUnique { get; set; } = false;
        /// <summary>
        /// If the property must be ignored when parse into a indexedDb
        /// </summary>
        public bool IsIgnore { get; set; } = false;
    }
}
