using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Attributes
{
    public class IndexDb : Attribute
    {
        public bool IsKeyPath { get; set; } = false;
        public bool IsAutoIncemental { get; set; } = false;
        public bool IsUnique { get; set; } = false;
        public bool IsIgnore { get; set; } = false;
    }
}
