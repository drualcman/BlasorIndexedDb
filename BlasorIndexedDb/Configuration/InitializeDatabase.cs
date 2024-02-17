namespace BlazorIndexedDb.Configuration;
internal class InitializeDatabase
{
    readonly Lazy<Task<IJSObjectReference>> ModuleTask;
    private static List<string> DatabaseModels = new List<string>();

    public InitializeDatabase(IJSRuntime js)
    {
        if (Settings.EnableDebug)
            Console.WriteLine($"InitializeDatabase constructor");
        ModuleTask = new Lazy<Task<IJSObjectReference>>(() => GetJSObjectReference(js));
    }

    private Task<IJSObjectReference> GetJSObjectReference(IJSRuntime jsRuntime) =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", $"./_content/DrUalcman-BlazorIndexedDb/MyDbJS.js?v={DateTime.Today.Year}{DateTime.Today.Month}{DateTime.Today.Day}").AsTask();

    /// <summary>
    /// Initialize the connection with a indexedDb
    /// </summary>
    internal async Task<IJSObjectReference> GetJsReference() 
    {
        if (Settings.EnableDebug)
            Console.WriteLine($"InitializeDatabase GetJsReference => Is Initialized {IsInitilized(Settings.DataBaseModelAsJson)}");
        IJSObjectReference jsReference = await ModuleTask.Value;
        if (!IsInitilized(Settings.DataBaseModelAsJson))
        {
            try
            {
                await jsReference.InvokeVoidAsync("MyDb.Init", Settings.DataBaseModelAsJson);
                DatabaseModels.Add(Settings.DataBaseModelAsJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        return jsReference;
    }

    bool IsInitilized(string model) 
    {
        if(DatabaseModels.Contains(model)) return true;
        else return false;
    }

    internal static async Task<IJSObjectReference> GetIJSObjectReference(IJSRuntime js)
    {
        if (Settings.EnableDebug) Console.WriteLine($"InitializeDatabase.GetIJSObjectReference");
        InitializeDatabase initializeDatabase = new InitializeDatabase(js);
        return await initializeDatabase.GetJsReference();
    }
}
