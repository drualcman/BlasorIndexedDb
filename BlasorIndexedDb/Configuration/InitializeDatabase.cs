using BlazorIndexedDb.Extensions;

namespace BlazorIndexedDb.Configuration;
internal class InitializeDatabase
{
    readonly Lazy<Task<IJSObjectReference>> ModuleTask;
    readonly Settings Setup;
    private static HashSet<string> DatabaseModels = new HashSet<string>();

    private InitializeDatabase(IJSRuntime js, Settings setup)
    {
        if (Settings.EnableDebug)
            Console.WriteLine($"InitializeDatabase constructor");
        ModuleTask = new Lazy<Task<IJSObjectReference>>(() => GetJSObjectReference(js));
        Setup = setup;
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
            Console.WriteLine($"InitializeDatabase GetJsReference => Is Initialized {IsInitilized(Setup.DBName)}");

        IJSObjectReference jsReference = await ModuleTask.Value;

        if (!IsInitilized(Setup.DBName))
        {
            try
            {
                string dbName = await jsReference.GetJsonResult<string>("MyDb.Init", Setup.DataBaseModelAsJson);
                if (Settings.EnableDebug) Console.WriteLine($"Database Initialized => {dbName}");
                DatabaseModels.Add(Setup.DBName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        return jsReference;
    }

    bool IsInitilized(string dbName)
    {
        bool result;
        if (DatabaseModels.Contains(dbName)) result = true;
        else result = false;
        Console.WriteLine($"Database Initialized from c# name 1 =>: {result}");
        return result;
    }

    internal static async Task<IJSObjectReference> GetIJSObjectReference(IJSRuntime js, Settings setup)
    {
        if (Settings.EnableDebug) Console.WriteLine($"InitializeDatabase.GetIJSObjectReference");
        InitializeDatabase initializeDatabase = new InitializeDatabase(js, setup);
        return await initializeDatabase.GetJsReference();
    }

    public static void DropDatabase(string model)
    {
        DatabaseModels.Remove(model);
    }
}
