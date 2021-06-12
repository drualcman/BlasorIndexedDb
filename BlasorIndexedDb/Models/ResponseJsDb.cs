using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Models
{
    /// <summary>
    /// Response object for the commands
    /// </summary>
    public class ResponseJsDb
    {
        /// <summary>
        /// Transaction result
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// Transaction message
        /// </summary>
        public string Message { get; set; }
    }
}
