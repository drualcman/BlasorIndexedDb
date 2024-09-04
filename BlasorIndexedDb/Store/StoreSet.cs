namespace BlazorIndexedDb.Store
{
    /// <summary>
    /// Class represent
    /// </summary>
    public class StoreSet<TModel> where TModel : class
    {
        readonly DeleteActions DeleteActions;
        readonly InsertActions InsertActions;
        readonly UpdateActions UpdateActions;
        readonly SelectActions SelectActions;
        readonly TableActions TableActions;

        /// <summary>
        /// Constructor to get the connection
        /// </summary>
        /// <param name="js"></param>
        /// <param name="setup"></param>
        public StoreSet(IJSRuntime js, Settings setup)
        {
            if (Settings.EnableDebug) Console.WriteLine($"StoreSet constructor for: {Utils.GetGenericTypeName(this.GetType())}");
            DeleteActions = new(js, setup);
            InsertActions = new(js, setup);
            UpdateActions = new(js, setup);
            SelectActions = new(js, setup);
            TableActions = new(js, setup);
        }

        /// <summary>
        /// Get a list with the content about the store model
        /// </summary>       
        /// <param name="query">Query</param>
        /// <returns></returns>
        public async Task<List<TModel>> SelectAsync(Func<TModel, bool> query)
        {
            List<TModel> response = await SelectActions.DbSelect<TModel>();
            return response.Where(query).ToList();
        }

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        public async Task<TModel> GetAsync(Func<TModel, bool> query)
        {
            IEnumerable<TModel> response = await SelectAsync(query);
            return response.FirstOrDefault(query);
        }

        /// <summary>
        /// Get a list with the content about the store model
        /// </summary>
        /// <returns></returns>
        public async Task<List<TModel>> SelectAsync() =>
            await SelectActions.DbSelect<TModel>();

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> GetAsync(object id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> GetAsync(int id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> GetAsync(double id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> GetAsync(decimal id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> GetAsync(long id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> GetAsync(string id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> GetAsync(DateTime id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Add record in a table store
        /// </summary>
        /// <param name="toAdd"></param>
        /// <param name="isOffline"></param>
        /// <returns></returns>
        public async Task<CommandResponse> AddAsync(TModel toAdd, bool isOffline = false)
        {
            ResponseJsDb result;
            if (isOffline)
                result = await InsertActions.DbInserOffline(toAdd);
            else
                result = await InsertActions.DbInsert(toAdd);
            return Utils.CommandResponse(result);
        }

        /// <summary>
        /// Add record in a table store
        /// </summary>
        /// <param name="toAdd"></param>
        /// <param name="isOffline"></param>
        /// <param name="limitRowTansaction"></param>
        /// <returns></returns>
        public async Task<CommandResponse> AddAsync(List<TModel> toAdd, bool isOffline = false, int limitRowTansaction = 500)
        {
            ConcurrentBag<ResponseJsDb> result = new();

            List<List<TModel>> rowsList = Split(toAdd, limitRowTansaction);
            List<Task> tasks = new List<Task>();
            foreach (var rows in rowsList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    List<ResponseJsDb> partialResult;
                    if (isOffline)
                        partialResult = await InsertActions.DbInserOffline(rows);
                    else
                        partialResult = await InsertActions.DbInsert(rows);

                    foreach (var item in partialResult)
                    {
                        result.Add(item);
                    }
                }));
            }
            await Task.WhenAll(tasks);

            return Utils.CommandResponse(result.ToList());
        }

        /// <summary>
        /// Update record in a table store
        /// </summary>
        /// <param name="toUpdate"></param>
        /// <param name="isOffline"></param>
        /// <returns></returns>
        public async Task<CommandResponse> UpdateAsync(TModel toUpdate, bool isOffline = false)
        {
            ResponseJsDb result;
            if (isOffline)
                result = await UpdateActions.DbUpdateOffLine(toUpdate);
            else
                result = await UpdateActions.DbUpdate(toUpdate);
            return Utils.CommandResponse(result);
        }

        /// <summary>
        /// Update record in a table store
        /// </summary>
        /// <param name="toUpdate"></param>
        /// <param name="isOffline"></param>
        /// <param name="limitRowTansaction"></param>
        /// <returns></returns>
        public async Task<CommandResponse> UpdateAsync(List<TModel> toUpdate, bool isOffline = false, int limitRowTansaction = 500)
        {
            ConcurrentBag<ResponseJsDb> result = new();

            List<List<TModel>> rowsList = Split(toUpdate, limitRowTansaction);
            List<Task> tasks = new List<Task>();
            foreach (var rows in rowsList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    List<ResponseJsDb> partialResult;
                    if (isOffline)
                        partialResult = await UpdateActions.DbUpdateOffLine(rows);
                    else
                        partialResult = await UpdateActions.DbUpdate(rows);

                    foreach (var item in partialResult)
                    {
                        result.Add(item);
                    }
                }));
            }
            await Task.WhenAll(tasks);
            return Utils.CommandResponse(result.ToList());
        }

        /// <summary>
        /// Delete all rows from a table
        /// </summary>
        /// <returns></returns>
        public async Task<CommandResponse> CleanAsync() =>
            Utils.CommandResponse(await TableActions.DbCleanTable<TModel>());

        /// <summary>
        /// Delete record from a table store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DeleteAsync(int id)
            => Utils.CommandResponse(await DeleteActions.DbDelete<TModel>(id));

        /// <summary>
        /// Delete record from a table store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DeleteAsync(double id)
            => Utils.CommandResponse(await DeleteActions.DbDelete<TModel>(id));

        /// <summary>
        /// Delete record from a table store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DeleteAsync(decimal id)
            => Utils.CommandResponse(await DeleteActions.DbDelete<TModel>(id));

        /// <summary>
        /// Delete record from a table store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DeleteAsync(string id)
            => Utils.CommandResponse(await DeleteActions.DbDelete<TModel>(id));

        /// <summary>
        /// Delete record from a table store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DeleteAsync(DateTime id)
            => Utils.CommandResponse(await DeleteActions.DbDelete<TModel>(id));

        private List<List<TModel>> Split(List<TModel> original, int sizeList)
        {
            List<List<TModel>> result = new List<List<TModel>>();
            int index = 0;
            if (original is not null && original.Count > 0)
            {
                while (index < original.Count)
                {
                    int count = Math.Min(sizeList, original.Count - index);
                    if (count > 0)
                    {
                        List<TModel> List = original.GetRange(index, Math.Min(sizeList, original.Count - index));
                        result.Add(List);
                    }
                    index += sizeList;
                }
            }
            else
            {
                result.Add(new List<TModel>());
            }
            return result;
        }

    }
}
