[![Nuget](https://img.shields.io/nuget/v/DrUalcman-BlazorIndexedDb?style=for-the-badge)](https://www.nuget.org/packages/DrUalcman-BlazorIndexedDb)
[![Nuget](https://img.shields.io/nuget/dt/DrUalcman-BlazorIndexedDb?style=for-the-badge)](https://www.nuget.org/packages/DrUalcman-BlazorIndexedDb)


# BlazorIndexedDb
Manage indexedDb from c# with Blazor. Simple way to interact with IndexedDB similar how you can do with EntityFramework

# NuGet installation
```PM> Install-Package DrUalcman-BlazorIndexedDb```

## New version changes coming
Working on change version control to avoid need to delete the previous database if add, delete or change StoreSet

# Current features
Create StoreContext<TStore> from abstract class. Allow multiple StoreContext but dabase name should be different names.
StoreSet per each model you need into a database.
Set PrimaKey in the model. Using convention if have property Id or TableNameId or IdTableName then this is used like PrimaryKey AutoIncremental (only if it's a number is autoincremental)
CRUD from StoreSet
Select all or one by PrimaryKey or property from StoreSet
Clean all data in a storeSet

# How to use
BlazorIndexedDb requires an instance IJSRuntime, should normally already be registered.

Create any code first database model you'd like to create and inherit from IndexedDb. You must be use the attribute FieldAttribute to setup the properties.

Your model (eg. PlayList) should contain an Id property or a property marked with the key attribute.

```
    public class PlayList
    {
        [FieldAttribute(IsKeyPath = true, IsAutoIncemental = false, IsUnique = true)]           //not required from version 1.5.18
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

Then can create a DBContext class inherits from StoreContext<TSore> to manage the database like in EF using a constructor with IJSRuntime and properties with the server about how to manage your tables.

```
public class DBContext : StoreContext<DBContext>
    {
        #region properties
        public StoreSet<PlayList> PlaysList { get; set; }
        #endregion

        #region constructor
        public DBContext(IJSRuntime js) : base(js, new Settings { DBName = "MyDBName", Version = 1 }) { }
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
            //INJECT THE DBContext
            builder.AddBlazorIndexedDbContext<DBContext>();

            var app = builder.Build();

            //USE THE CONTEXT, if you are using more than one CONTEXT repeat per each context you are using
            await app.UseBlazorIndexedDbContext<DBContext>();

            await app.RunAsync();
        }
    }
```
In the Index.html no need add any javascript reference. This file in previous versions is now dynamic add when is needed.

In the component need to use a IndexDb inject DBContext

```
        [Inject]
        public DBContext DB { get; set; }

        void Select()
        {
            var PlaysList = await DB.PlaysList.SelectAsync();
        }

        void Add()
        {
            var NewItems = new List<PlayList>();
            CommandResponse response = await DB.PlaysList.AddAsync(NewItems);
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
            CommandResponse response = await DB.PlaysList.UpdateAsync(NewItem);
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
            CommandResponse response = await DB.PlaysList.DeleteAsync(id);
            Console.WriteLine(response.Message);
            Console.WriteLine(response.Result);
            //Get result per each element
            foreach (var item in response.Response)
            {
                Console.WriteLine(item.Message);
                Console.WriteLine(item.Result);
            }
        }
```

You can modify the model classes any time, but if the model you will pass don't match with the model created when create the IndexDb this will return a exception.

# Working with records
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

# Exceptions
When exception will return a ResponseException

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
