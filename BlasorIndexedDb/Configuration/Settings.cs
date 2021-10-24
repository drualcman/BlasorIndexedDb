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
    public class Settings
    {
        private string DbName_BK;
        /// <summary>
        /// Database name
        /// </summary>
        public string DBName
        {
            get { return DbName_BK; }
            set { DbName_BK = string.IsNullOrEmpty(value) ? AppDomain.CurrentDomain.FriendlyName : value; }
        }
        /// <summary>
        /// Version of the database
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Assembly name to search the entities
        /// </summary>
        public string AssemblyName { get; set; }

        private string entitiesNamespace_BK;

        /// <summary>
        /// Namespace where is located the entities if it's different from the assembly name
        /// </summary>
        [Obsolete("No need anymore")]
        public string EntitiesNamespace
        {
            get { return string.IsNullOrEmpty(entitiesNamespace_BK) ? AssemblyName: entitiesNamespace_BK; }
            set { entitiesNamespace_BK = value; }
        }

        /// <summary>
        /// Default constructor get the AppDomain.CurrentDomain.FriendlyName like a DB name
        /// </summary>
        public Settings() : this(string.Empty) { }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="DbName"></param>
        /// <param name="version"></param>
        public Settings(string DbName, int version = 1)
        {
            this.DBName = DbName;
            this.Version = version;
        }


        #region static
        /// <summary>
        /// Enable show console log info
        /// </summary>
        public static bool EnableDebug { get; set; }
        /// <summary>
        /// Names about the models to use
        /// </summary>
        public static string[] Tables { get; set; }

        ///// <summary>
        ///// Names about the models to use
        ///// </summary>
        //public static Dictionary<string, string> Tables1 { get; set; }

        private static bool InitializedBk;

        /// <summary>
        /// Know if the instance of a indexDb it's already initialized
        /// </summary>
        public static bool Initialized
        {
            get { return InitializedBk; }
            set { InitializedBk = value; }
        }

        /// <summary>
        /// Know if the instance of a indexDb it's already initialized
        /// </summary>
        [Obsolete("Use Initialized because a wrong spelling")]
        public static bool Initiallezed
        {
            get { return InitializedBk; }
            set { InitializedBk = value; }
        }

        static Settings()
        {            
            Tables = new string[0];
            Initialized = false;
        }
        #endregion

    }
}
