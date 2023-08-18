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
        public StoreSet(IJSObjectReference js, Settings setup)
        {
            if(Settings.EnableDebug) Console.WriteLine($"StoreSet constructor for : {Utils.GetGenericTypeName(this.GetType())}");
            DeleteActions = new(js, setup);
            InsertActions = new(js, setup);
            UpdateActions = new(js, setup);
            SelectActions = new(js, setup);
            TableActions = new(js, setup);
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
        public async Task<TModel> SelectAsync(object id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(int id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(double id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(decimal id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(long id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(string id) =>
            await SelectActions.SingleRecord<TModel>(id);

        /// <summary>
        /// Get a single record from store model
        /// </summary>
        /// <param name="id">primary key id to search</param>
        /// <returns></returns>
        public async Task<TModel> SelectAsync(DateTime id) =>
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
            if(isOffline)
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
            List<ResponseJsDb> result = default;

            List<List<TModel>> rowsList = Split(toAdd, limitRowTansaction);
            List<Task> tasks = new List<Task>();
            foreach(var rows in rowsList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    if(isOffline)
                        result = await InsertActions.DbInserOffline(rows);
                    else
                        result = await InsertActions.DbInsert(rows);
                }));
            }
            await Task.WhenAll(tasks);

            return Utils.CommandResponse(result);
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
            if(isOffline)
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
            List<ResponseJsDb> result = default;

            List<List<TModel>> rowsList = Split(toUpdate, limitRowTansaction);
            List<Task> tasks = new List<Task>();
            foreach(var rows in rowsList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    if(isOffline)
                        result = await UpdateActions.DbUpdateOffLine(rows);
                    else
                        result = await UpdateActions.DbUpdate(rows);
                }));
            }
            await Task.WhenAll(tasks);
            return Utils.CommandResponse(result);
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

        private List<List<TModel>> Split<TModel>(List<TModel> original, int sizeList)
        {
            List<List<TModel>> Result = new List<List<TModel>>();
            int Index = 0;
            if(original is not null)
            {
                while(Index < original.Count)
                {
                    List<TModel> List = original.GetRange(Index, Math.Min(sizeList, original.Count - Index));
                    Result.Add(List);
                    Index += sizeList;
                }
            }
            else
            {
                Result.Add(new List<TModel>());
            }
            return Result;
        }

    }
}
