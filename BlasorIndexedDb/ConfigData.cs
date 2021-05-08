using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb
{
    public static class ConfigData
    {
        public static string DBName { get; set; }
        public static int Version { get; set; }
        public static string AssemblyName { get; set; }
        public static string EntitiesNamespace { get; set; }
        public static string[] Tables { get; set; }
    }
}
