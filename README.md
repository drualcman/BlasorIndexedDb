# BlazorIndexedDb
Manage indexedDb from c# with Blazor. Simple way to interact with IndexedDB similar how you can do with EntityFramework

# NuGet installation
```PM> Install-Package DrUalcman-BlazorIndexedDb```

## New version changes coming
Working in a new version with StoreContext and StoreSet to easy manage and work from C#. New version 1.4.14. Last stable version 1.4.13
Add references between tables. Update multiple tables same time, and more...

# Current features
Create StoreContext.
StoreSet per each model you need into a database.
Set PrimaKey in the model.
CURD from StoreSet
Select all or one by PrimaryKey from StoreSet

# How to use
BlazorIndexedDb requires an instance IJSRuntime, should normally already be registered.

Create any code first database model you'd like to create and inherit from IndexedDb. You must be use the attribute FieldAttribute to setup the properties.

Your model (eg. PlayList) should contain an Id property or a property marked with the key attribute.

```
    public class PlayList
    {
        [FieldAttribute(IsKeyPath = true, IsAutoIncemental = false, IsUnique = true)]
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Ownner { get; set; }
    }
```

namespace

```
BlazorIndexedDb
BlazorIndexedDb.Attributes
BlazorIndexedDb.Commands
BlazorIndexedDb.Models
BlazorIndexedDb.Store
```

Then can create a DBContext class to manage the database like in EF using a constructor with IJSRuntime and properties with the server about how to manage your tables.

```
public class DBContext
    {
        #region properties
        public StoreSet<PlayList> PlaysList { get; set; }
        #endregion

        #region constructor
        public DBContext(IJSRuntime js)
        {
            Settings.EnableDebug = true;
            Settings settings = new Settings("PlaysListDb");
            settings.AssemblyName = "PlaysList.Shared";
            Init(settings);
        }
        #endregion

        #region helpers
        public void ProcessErrors(List<ResponseJsDb> result)
        {
            string errors = string.Empty;
            foreach (ResponseJsDb error in result)
            {
                errors += error.Message + "<br/>";
            }
            Console.WriteLine(errors);
        }
        #endregion
    }
```
In Program.cs add the service for the DBContext

```
public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            //INJECT ALWAYS AS SINGLETON
            builder.Services.AddSingleton<DBContext>();

            await builder.Build().RunAsync();
        }
    }
```

In the component need to use a IndexDb inject DBContext

```
        [Inject]
        public DBContext _DBContext { get; set; }
```

You can modify the model classes any time, but if the model you will pass don't match with the model created when create the IndexDb this will return a exception.

# IJSRuntime Db extensions
1. SingleRecord<TModel>
2. DbSelect<TModel>
3. DbInsert<TModel>
4. DbUpdate<TModel>
5. DbDelete<TModel>

# Working with records from IJSRuntime Db extensions
In all the select action you will receive the List<TModel> except if you are looking for one Key Id send, then you will receive the Model object.
In all actions you will receive a ResponseJsDb model or a List<ResponseJsDb> with all the responses, if you are sending a lot of rows.

```
    public class ResponseJsDb
    {
        public bool Result { get; set; }
        public string Message { get; set; }
    }
```

# Working with records from StoreSet
The store set always return the model or list of the model for all select actions and CommandResponse record for the comands actions

```
 public record CommandResponse(bool Result, string Message, List<ResponseJsDb> Response);
```

# More info
Check <a href="https://blazorindexdb.community-mall.com/">our web</a> to get more info.

## Dependencies
<a href="https://github.com/drualcman/jsDB">jsDB</a>
