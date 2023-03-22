namespace BlazorIndexedDb.ValueObjects;

/// <summary>
/// Database Commands to use
/// </summary>
internal enum DbCommands
{
    /// <summary>
    /// Insert array data
    /// </summary>
    Insert,
    /// <summary>
    /// Update array data
    /// </summary>
    Update,
    /// <summary>
    /// Delete single row from id send
    /// </summary>
    Delete,
}
