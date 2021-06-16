using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Models
{
    /// <summary>
    /// Return a result from the transaction
    /// </summary>
    public record CommandResult(bool Result, string Message, List<ResponseJsDb> Response);
}
