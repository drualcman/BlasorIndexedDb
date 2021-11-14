using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Store
{
    /// <summary>
    /// Define Tables and model name
    /// </summary>
    public class StoreSetDefinition
    {
        /// <summary>
        /// StoreSet Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// StoreSet Model name
        /// </summary>
        public string Model { get; set; }
    }
}
