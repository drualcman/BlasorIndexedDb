namespace BlazorIndexedDb.Models
{
    /// <summary>
    /// Return a result from the transaction
    /// </summary>
    public sealed record CommandResponse(bool Result, string Message, List<ResponseJsDb> Response)
    {
        /// <summary>
        /// Rows the transaction refused, one entry each, naming the record and the reason.
        /// </summary>
        /// <remarks>
        /// A write reports what it could not do row by row, so a duplicated key in the middle of a
        /// batch tells you which key it was instead of only that something went wrong.
        /// </remarks>
        public IEnumerable<ResponseJsDb> Errors =>
            Response is null ? Enumerable.Empty<ResponseJsDb>() : Response.Where(r => !r.Result);

        /// <summary>
        /// Throw a <see cref="ResponseException"/> if any row was refused, otherwise return this
        /// same response so the call can be chained.
        /// </summary>
        /// <remarks>
        /// Writes report failures in <see cref="Response"/> rather than throwing, because the
        /// per-row detail cannot survive an exception thrown on the JavaScript side — a rejected
        /// promise reaches .NET as a message and nothing else. Call this when you would rather
        /// handle a failed write the way EF handles a failed SaveChanges; the decision belongs to
        /// the caller, and it is the same decision whether one row failed or all of them did.
        /// </remarks>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        public CommandResponse EnsureSuccess()
        {
            if (Result)
                return this;
            string[] errors = Errors.Select(r => r.Message).ToArray();
            throw new ResponseException(errors.Length > 0 ? string.Join(Environment.NewLine, errors) : Message);
        }
    }
}
