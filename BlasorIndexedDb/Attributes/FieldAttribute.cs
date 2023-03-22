namespace BlazorIndexedDb.Attributes
{
    /// <summary>
    /// Custom Attribute to setup the model
    /// </summary>
    public sealed class FieldAttribute : Attribute
    {
        /// <summary>
        /// If the proerty is a keypath or requered
        /// </summary>
        public bool IsKeyPath { get; set; }
        /// <summary>
        /// If the property is auto incremental
        /// </summary>
        public bool IsAutoIncremental { get; set; }
        /// <summary>
        /// If the property are unique
        /// </summary>
        public bool IsUnique { get; set; }
        /// <summary>
        /// If the property must be ignored when parse into a indexedDb
        /// </summary>
        public bool IsIgnore { get; set; }

        /// <summary>
        /// Contructor to setup default values
        /// </summary>
        public FieldAttribute()
        {
            IsKeyPath = false;
            IsAutoIncremental = false;
            IsUnique = false;
            IsIgnore = false;
        }
    }
}
