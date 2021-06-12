using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Configuration
{
    /// <summary>
    /// Configuration class to setup a IndexedDb in a browser
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Dabatase name
        /// </summary>
        public static string DBName { get; set; }
        /// <summary>
        /// Version of the database
        /// </summary>
        public static int Version { get; set; }
        /// <summary>
        /// Assembly name to search the entities
        /// </summary>
        public static string AssemblyName { get; set; }
        /// <summary>
        /// Namespace where is located the entities if it's different from the assembly name
        /// </summary>
        public static string EntitiesNamespace { get; set; }
        /// <summary>
        /// Names about the models to use
        /// </summary>
        public static string[] Tables { get; set; }
    }
}
