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
        /// Execute a command directly sending a json string
        /// </summary>
        /// <param name="command"></param>
        /// <param name="storeName"></param>
        /// <param name="data"></param>
        /// <exception cref="ResponseException"></exception>
        /// <returns></returns>
        internal async ValueTask<List<ResponseJsDb>> DbCommand(DbCommands command, string storeName, string data)
        {
            if(Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => {command} store = {storeName}, data = {data}");
            if(string.IsNullOrEmpty(storeName)) throw new ResponseException(command.ToString(), "StoreName can't be null", data);
            else if(string.IsNullOrEmpty(data)) throw new ResponseException(command.ToString(), storeName, "Data can't be null");
            else
            {
                try
                {
                    return await jsRuntime.InvokeAsync<List<ResponseJsDb>>($"MyDb.{command}", storeName, data, Setup.DBName, Setup.Version, Setup.ModelsAsJson);
                }
                catch(Exception ex)
                {
                    if(Settings.EnableDebug) Console.WriteLine($"{Setup.DBName} => Exception: {ex}");
                    throw new ResponseException(command.ToString(), storeName, data, ex);
                }
            }
        }

        /// <summary>
        /// Get is we are connected to a indexed db
        /// </summary>
        /// <returns></returns>
        internal async ValueTask<string> DbConnected() =>
            await jsRuntime.InvokeAsync<string>("MyDb.Connected", Setup.DBName, Setup.Version);
    }
}
