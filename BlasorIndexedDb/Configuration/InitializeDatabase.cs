namespace BlazorIndexedDb.Configuration;
internal class InitializeDatabase
{
    readonly Lazy<Task<IJSObjectReference>> ModuleTask;
    private static bool IsInitilized;

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
            Console.WriteLine($"InitializeDatabase GetJsReference => Is Initialized {IsInitilized}");
        IJSObjectReference jsReference = await ModuleTask.Value;
        if (!IsInitilized)
        {
            try
            {
                await jsReference.InvokeVoidAsync("MyDb.Init", Settings.DataBaseModelAsJson);
                IsInitilized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                IsInitilized = false;
                throw;
            }
        }
        return jsReference;
    }

    internal static async Task<IJSObjectReference> GetIJSObjectReference(IJSRuntime js)
    {
        InitializeDatabase initializeDatabase = new InitializeDatabase(js);
        return await initializeDatabase.GetJsReference();
    }
}
