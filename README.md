[![Nuget](https://img.shields.io/nuget/v/DrUalcman-BlazorIndexedDb?style=for-the-badge)](https://www.nuget.org/packages/DrUalcman-BlazorIndexedDb)
[![Nuget](https://img.shields.io/nuget/dt/DrUalcman-BlazorIndexedDb?style=for-the-badge)](https://www.nuget.org/packages/DrUalcman-BlazorIndexedDb)


# BlazorIndexedDb
Manage IndexedDB from C# with Blazor. A simple way to interact with IndexedDB, similar to how you can do with Entity Framework.

# NuGet installation
```PM> Install-Package DrUalcman-BlazorIndexedDb```

## New version changes coming
Working on changing version control to avoid the need to delete the existing database when adding, removing, or modifying a StoreSet.

## What changed in this version
**Offline writes work.** `AddAsync(row, isOffline: true)` and `UpdateAsync(row, isOffline: true)` never wrote anything, in any version: they came back with `[Insert] StoreName can't be null` and never reached the database. The offline write adds an `OffLine` flag to each row by turning it into an `ExpandoObject`, and then handed the result back to the ordinary insert — where the generic argument is no longer your entity but `object`, so looking up which store it belongs to asked for a model named "Object" and got nothing. The database was never involved, which is why deleting and recreating it changed nothing. If you ever gave up on the offline flag because it did not work, this is the release to try again.

**Updating a row that is not there no longer discards the rest of the batch.** `Update` reads the stored row first so it can keep the columns you left out, and from a row that does not exist that merge produced an empty object — which IndexedDB refuses, from a place where the error aborts the whole transaction and rolls back every other row in the call. It is a `put`, so a row that is not stored yet is now simply written as it is. Updating a row that something else had just deleted was enough to hit this.

## What changed in 1.10.50
Writes got dramatically faster, a broken filter got fixed, and failures that used to be reported as success are now reported as failures. Nothing in your code has to change, but two of these are visible from the outside.

**Writes no longer wait on a guess.** Insert and Update used to resolve on a timer of 50ms per row, so a batch of 4944 rows took 247 seconds no matter how fast IndexedDB really was. They now resolve when the transaction actually completes. That same batch takes milliseconds of database time.

**New SelectAsync(column, value), resolved by the index.** The indexed lookup existed but nothing could reach it: no method on StoreSet called it, and the interop wrapper was calling the wrong JavaScript function anyway, so the filter was dropped and the whole store came back. Both are fixed and the overload is now public.

**GetAsync(id) works.** Looking a record up by its primary key built its key range from a variable that does not exist, so it threw before reaching the store and the error was swallowed into a null. It always came back empty, and never said why. If you worked around it by reading the whole store and filtering in C#, you can stop.

**A read that cannot open the database no longer hangs.** Select handled a failed open by calling a method that does not exist, so the promise was never settled at all.

**A blocked database is no longer reported as an error.** Blocked means your operation is queued behind another connection that has not closed yet, and it goes ahead on its own once that happens. DropDatabase rejected on it, reporting a failure for a delete that then completed anyway.

**A write that failed no longer looks like a success.** Nothing handled an aborted transaction or a failed database open, and the timer resolved regardless, so rows that were never written were reported as written. Those cases are now reported as failures.

**Deleting many rows takes one transaction.** See DeleteAsync with a list of ids below.

**A refused row no longer discards the rest of the batch.** See "Insert, update and duplicated keys" below.

Everything crossing to JavaScript now travels as UTF-8 bytes in both directions instead of strings. This is invisible from C#.

# Current features
Create StoreContext<TStore> from abstract class. Allow multiple StoreContext but database name should be different names.
StoreSet per each model you need into a database.
Set PrimaryKey in the model. Using convention if have property Id or TableNameId or IdTableName then this is used like PrimaryKey AutoIncremental (only if it's a number is autoincremental)
CRUD from StoreSet
Select all or one by PrimaryKey or property from StoreSet, filtering either in C# or through the store index
Clean all data in a StoreSet
Delete many rows in a single transaction
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

        void SelectWhere()
        {
            //resolved by the store index, only the matching rows cross into .NET
            var mine = await DB.PlayList.SelectAsync(nameof(PlayList.Ownner), "me");

            //this other overload reads the whole store and filters it in C#
            var also = await DB.PlayList.SelectAsync(x => x.Ownner == "me");
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

        void DeleteMany()
        {
            //all the ids are removed inside a single transaction
            var ids = new List<int> { 1, 2, 3 };
            CommandResponse response = await DB.PlayList.DeleteAsync(ids);
            Console.WriteLine(response.Message);
            Console.WriteLine(response.Result);
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
 public sealed record CommandResponse(bool Result, string Message, List<ResponseJsDb> Response)
 {
     public IEnumerable<ResponseJsDb> Errors { get; }
     public CommandResponse EnsureSuccess();
 }
```

Result is false when any row was refused. Errors lists only those rows, so you do not have to walk the whole Response looking for them.

```
        CommandResponse response = await DB.PlayList.AddAsync(NewItems);
        foreach (var error in response.Errors)
        {
            //names the record and the reason, e.g. duplicated key
            Console.WriteLine(error.Message);
        }
```

# Insert, update and duplicated keys
AddAsync is an insert, not an upsert. A key that already exists is refused, the same way a primary key behaves in SQL. Use UpdateAsync to overwrite an existing row.

A refused row does not take the rest of the batch with it: everything else in the call is written, and the response carries one entry per refused row naming its key and the reason. There is no need to split a write into chunks in order to find out which rows failed.

# Exceptions
Writes report failures in the response instead of throwing, because the detail of which row failed cannot survive an exception raised on the JavaScript side. Call EnsureSuccess() when you would rather handle a failed write the way Entity Framework handles a failed SaveChanges. It returns the same response when everything went in, so it can be chained.

```
        //throws ResponseException if any row was refused
        CommandResponse response = (await DB.PlayList.AddAsync(NewItems)).EnsureSuccess();
```

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
