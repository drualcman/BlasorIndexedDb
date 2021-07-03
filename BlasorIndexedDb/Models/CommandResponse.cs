using System.Collections.Generic;

namespace BlazorIndexedDb.Models
{
    /// <summary>
    /// Return a result from the transaction
    /// </summary>
    public record CommandResponse(bool Result, string Message, List<ResponseJsDb> Response);
}
