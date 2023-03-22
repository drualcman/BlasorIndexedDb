namespace BlazorIndexedDb.Attributes
{
    /// <summary>
    /// Relationship between tables
    /// </summary>
    public sealed class RelationAttribute : Attribute
    {
        /// <summary>
        /// Property name from the Model will create a relationship. Default value Id
        /// </summary>
        public string StoreIndexName { get; set; }
        /// <summary>
        /// Not nullable. Field name default value RelationId must be use in indexDb Store table to save the StoreIndexName value.
        /// This property no need to be created into the model to have a relationship.
        /// </summary>
        [NotNull] public string FieldName { get; set; }
        /// <summary>
        /// Setup default values
        /// </summary>
        public RelationAttribute()
        {
            StoreIndexName = "Id";
            FieldName = "RelationId";
        }
    }
}
