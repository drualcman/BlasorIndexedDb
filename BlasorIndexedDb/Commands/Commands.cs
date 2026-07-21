namespace BlazorIndexedDb.Commands
{
    /// <summary>
    /// Other commands to execute
    /// </summary>
    internal sealed class Commands
    {
        readonly IJSObjectReference jsRuntime;
        readonly Settings Setup;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="js"></param>
        /// <param name="setup"></param>
        internal Commands(IJSObjectReference js, Settings setup)
        {
            jsRuntime = js;
            Setup = setup;
        }

        /// <summary>
        /// Execute a command sending the payload as UTF-8 encoded Json.
        /// </summary>
        /// <remarks>
        /// The payload used to travel as a string, which meant .NET built the whole Json into a
        /// string, marshalled it, and JS parsed it back — and the response made the same trip in
        /// reverse through a JsonElement. Bytes cross the boundary as a Uint8Array with no string in
        /// between, which is what the read side (Select) has always done.
        /// </remarks>
        /// <param name="command"></param>
        /// <param name="storeName"></param>
        /// <param name="data">payload already encoded as UTF-8 Json</param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async Task<List<ResponseJsDb>> DbCommand(DbCommands command, string storeName, byte[] data)
        {
            if (Settings.EnableDebug)
                Console.WriteLine($"{Setup.DBName} => {command} store = {storeName}, data = {data?.Length ?? 0} bytes");
            if (string.IsNullOrEmpty(storeName))
                throw new ResponseException(command.ToString(), "StoreName can't be null", string.Empty);
            else if (data is null || data.Length == 0)
                throw new ResponseException(command.ToString(), storeName, "Data can't be null");
            else
            {
                try
                {
                    return await jsRuntime.GetBytesResult<List<ResponseJsDb>>($"MyDb.{command}", storeName, data, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                }
                catch (Exception ex)
                {
                    if (Settings.EnableDebug)
                        Console.WriteLine($"{Setup.DBName} => Exception: {ex}");
                    throw new ResponseException(command.ToString(), storeName, $"{data.Length} bytes", ex);
                }
            }
        }

        /// <summary>
        /// Get is we are connected to a indexed db
        /// </summary>
        /// <returns></returns>
        internal async Task<string> DbConnected() =>
            await jsRuntime.GetJsonResult<string>("MyDb.Connected", Setup.DBName, Setup.Version);
    }
}
