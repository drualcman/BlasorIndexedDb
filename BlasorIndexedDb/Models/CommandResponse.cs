namespace BlazorIndexedDb.Models
{
    /// <summary>
    /// Return a result from the transaction
    /// </summary>
    public sealed record CommandResponse(bool Result, string Message, List<ResponseJsDb> Response);
}
