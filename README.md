# BlazorIndexedDb
Manage indexedDb from c# with Blazor. Simple way to interact with IndexedDB and manage the data using List<> and LinkQ

# NuGet installation
```PM> Install-Package DrUalcman-BlazorIndexedDb```

# Current features
Connect and create database
Define PrimaKey in the model
Add record
Remove record
Edit record


# How to use
BlazorIndexedDb requires an instance IJSRuntime, should normally already be registered.

Create any code first database model you'd like to create and inherit from IndexedDb. You must be use the attribute IndexDb to setup the properties.

Your model (eg. PlayList) should contain an Id property or a property marked with the key attribute.

```
    public class PlayList
    {
        [IndexDb(IsKeyPath = true, IsAutoIncemental = false, IsUnique = true)]
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Ownner { get; set; }
    }
```

You need import namespace

```
using Microsoft.JSInterop;
using BlazorIndexedDb;
using BlazorIndexedDb.Models;
```

Then can create a DBContext class to manage the database like in EF using a constructor with IJSRuntime and properties with the server about how to manage your tables.

```
public class DBContext
    {
        #region properties
        public PlayListService PlayList { get; private set; }
        #endregion

        #region constructor
        public DBContext(IJSRuntime js)
        {
            _ = js.DbInit("BlazorYoutubePlayer", 1, 			//db name and version
                new string[] { "PlayList" }, 				//table to use, each table must be match with the model class
                "BlazorYoutubePlayerViewer", 				//assembly name
                "BlazorYoutubePlayerViewer.DataBase.Entities");		//namespace where is defined the models classes
            PlayList = new PlayListService(js);
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

The service class example

```
public class PlayListService
    {
        private readonly IJSRuntime DBConn;
        public PlayListService(IJSRuntime js)
        {
            DBConn = js;
        }

        public async Task<List<PlayList>> GetAsync() =>
            await DBConn.DbSelect<PlayList>();

        public async Task<PlayList> GetAsync(string id) =>
            await DBConn.DbSelect<PlayList>(id);

        public async Task<ResponseJsDb> AddAsync(PlayList toAdd)
            => await DBConn.DbInsert(toAdd);

        public async Task<ResponseJsDb> UpdateAsync(PlayList toAdd)
            => await DBConn.DbUpdate(toAdd);

        public async Task<ResponseJsDb> DeleteAsync(string id)
            => await DBConn.DbDelete<PlayList>(id);
    }
```

In the index.razor component, or the first component need to use a IndexDb, use this code to create or open the database with all your model classes.

```
        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        public DBContext _DBContext;
        protected override void OnInitialized()
        {
            _DBContext = new DBContext(JsRuntime);
        }
```

You can modify the model classes any time, but if the model you will pass don't match with the model created when create the IndexDb this will return a exception.

# IJSRuntime Db extensions
1. DbSelect<TModel>
2. DbInsert<TModel>
3. DbUpdate<TModel>
4. DbDelete<TModel>

# Working with records
In all the select action you will receive the List<TModel> except if you are looking for one Key Id send, then you will receive the Model object.
In all actions you will receive a ResponseJsDb model or a List<ResponseJsDb> with all the responses, if you are sending a lot of rows.\

```
    public class ResponseJsDb
    {
        public bool Result { get; set; }
        public string Message { get; set; }
    }
```

# More info
Check our web to (under construction) to get more info.

## Dependencies
<a href="https://github.com/drualcman/jsDB">jsDB</a>
