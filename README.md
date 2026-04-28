[![Nuget](https://img.shields.io/nuget/v/DrUalcman-BlazorIndexedDb?style=for-the-badge)](https://www.nuget.org/packages/DrUalcman-BlazorIndexedDb)
[![Nuget](https://img.shields.io/nuget/dt/DrUalcman-BlazorIndexedDb?style=for-the-badge)](https://www.nuget.org/packages/DrUalcman-BlazorIndexedDb)


# BlazorIndexedDb
Manage IndexedDB from C# with Blazor. A simple way to interact with IndexedDB, similar to how you can do with Entity Framework.

# NuGet installation
```PM> Install-Package DrUalcman-BlazorIndexedDb```

## New version changes coming
Working on changing version control to avoid the need to delete the existing database when adding, removing, or modifying a StoreSet.

# Current features
Create StoreContext<TStore> from abstract class. Allow multiple StoreContext but database name should be different names.
StoreSet per each model you need into a database.
Set PrimaryKey in the model. Using convention if have property Id or TableNameId or IdTableName then this is used like PrimaryKey AutoIncremental (only if it's a number is autoincremental)
CRUD from StoreSet
Select all or one by PrimaryKey or property from StoreSet
Clean all data in a StoreSet
Drop Database

# How to use
BlazorIndexedDb requires an instance of IJSRuntime, which should normally already be registered.

Create a code-first database model and inherit from IndexedDb. You must use the FieldAttribute to configure the properties.

Your model (e.g., PlayList) should contain an Id property or a property marked with the key attribute.

```
    public class PlayList
    {
        [FieldAttribute(IsKeyPath = true, IsAutoIncremental = false, IsUnique = true)]           //not required from version 1.5.18
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

Then create a DBContext class inheriting from StoreContext<TStore> to manage the database, similar to Entity Framework. Define properties that represent your tables.

```
public class DBContext : StoreContext<DBContext>
    {
        #region properties
        public StoreSet<PlayList> PlayList { get; set; }
        #endregion

        #region constructor
        public DBContext(IJSRuntime js) : base(js, new Settings { DBName = "MyDBName", Version = 1 }) { }
        #endregion
    }
```
In Program.cs add the service for the DBContext

```
using BlazorIndexedDb;
public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            //INJECT THE DBContext or use your implementation
            builder.AddBlazorIndexedDbContext<DBContext>();

            var app = builder.Build();
            await app.RunAsync();
        }
    }
```
In index.html, there is no need to add any JavaScript reference. In previous versions this was required, but now it is added dynamically when needed.

In the component, inject DBContext to access IndexedDB.

```
        [Inject]
        public DBContext DB { get; set; }

        void Select()
        {
            var PlayList = await DB.PlayList.SelectAsync();
        }

        void Add()
        {
            var NewItems = new List<PlayList>();
            CommandResponse response = await DB.PlayList.AddAsync(NewItems);
            Console.WriteLine(response.Message);
            Console.WriteLine(response.Result);
            //Get result per each element
            foreach (var item in response.Response)
            {
                Console.WriteLine(item.Message);
                Console.WriteLine(item.Result);
            }
        }

        void Update()
        {
            var NewItem = new PlayList();
            CommandResponse response = await DB.PlayList.UpdateAsync(NewItem);
            Console.WriteLine(response.Message);
            Console.WriteLine(response.Result);
            //Get result per each element
            foreach (var item in response.Response)
            {
                Console.WriteLine(item.Message);
                Console.WriteLine(item.Result);
            }
        }

        void Delete()
        {
            int id = 1;
            CommandResponse response = await DB.PlayList.DeleteAsync(id);
            Console.WriteLine(response.Message);
            Console.WriteLine(response.Result);
            //Get result per each element
            foreach (var item in response.Response)
            {
                Console.WriteLine(item.Message);
                Console.WriteLine(item.Result);
            }
        }

        void Drop()
        {
            CommandResponse response = await DB.DropDatabaseAsync();
            Console.WriteLine(response.Message);
            Console.WriteLine(response.Result);
        }

        void Init()
        {
            //if you delete a db and want to initialize again
            await DB.Init();
        }
```

You can modify the model classes any time, but if the model you will pass don't match with the model created when create the IndexDb this will return a exception.

# Working with records
In all select actions, you will receive a List<TModel>, except when querying by a single key, in which case you will receive a single model instance.

All actions return either a ResponseJsDb or a List<ResponseJsDb> when multiple rows are processed.

```
    public class ResponseJsDb
    {
        public bool Result { get; set; }
        public string Message { get; set; }
    }
```

# Working with records from StoreSet
The store set always returns the model or list of the model for all select actions and CommandResponse record for the commands actions

```
 public record CommandResponse(bool Result, string Message, List<ResponseJsDb> Response);
```

# Exceptions
When an exception occurs, a ResponseException will be returned.

```
    public class ResponseException : Exception
    {
        public string Command { get; set; }

        public string StoreName { get; set; }

        public string TransactionData { get; set; }
    }
```

# More info
Check [website](https://blazorindexdb.community-mall.com/) for more info.
