(function () {
    var db;
    window.MyDb = {
        /**
         * Initializing instance of a DB with a model send
         * @param {any} model
         */
        Init: (model) => {
            console.log(model);
            if (model) db = new jsDB(JSON.parse(model))
        },                
        Connected: () => db.Connected(),
        /**
         * Request all data from a table name. Return the data or JSON response
         * @param {string} table table name to request the data
         */
        Select: async function(table) {
            let data = new Promise(function (resolve, error) {
                try {
                    if (db) {
                        db.Select(table, function (result) {
                            resolve(result);
                        });
                    }
                    else error("DataBase not yet initialize");
                } catch (e) {
                    error(e);
                }
            });
            let result = await data;        //wait till the promise is done
            return result;                  //return the value
        },
        /**
         * Request data from a table name with the keyPath with a value. Return the data or JSON response
         * @param {string} table table name
         * @param {any} id id value
         * @param {function} callBack function to receive the data
         */
        SelectId: (table, id, callBack) => db.SelectId(table, id, callBack),
        /**
         * Request data from a table name filtered by column name and value. Return the data or JSON response
         * @param {string} table table name
         * @param {string} column column name to filter
         * @param {any} value value for the filter
         * @param {function} callBack function to receive the data
         */
        SelectWhere: (table, column, value, callBack) => db.SelectWhere(table, column, value, callBack),
        /**
         * Insert data into a table. Alway return a JSON response
         * @param {string} table table name
         * @param {JSON} data data with the model format to insert
         */
        Insert: async function (table, data) {
            console.log('insert 1');
            let resolve = new Promise(function (resolve, error) {
                console.log('insert 2', db);
                if (db) {
                    db.Insert(table, JSON.parse(data), function (result) {
                        resolve(result.result);
                    });
                }
                else error(false);

            });
            let result = await resolve;        //wait till the promise is done
            return result;                  //return the value
        },
        /**
         * Update data into the table. The data always must be content all the columns, if not the function retreive the actual data to keep always same data into a table. Alway return a JSON response
         * @param {string} table table name
         * @param {JSON} data data with the model format to update
         * @param {function} callBack function to receive the result
         */
        Update: (table, data, callBack) => Update(table, JSON.parse(data), callBack),
        /**
         * Dete one row from a table. Alway return a JSON response
         * @param {string} table table name
         * @param {any} id value from the keypath
         * @param {function} callBack Function to receive the result
         */
        Delete: (table, id, callBack) => Delete(table, id, callBack),
        /**
         * Drop a table. Alway return a JSON response
         * @param {string} table table name
         * @param {function} callBack function to receive the result
         */
        Drop: (table, callBack) => Drop(table, callBack),
        /**
         * To store files into a db for compatibility with most of the browsers
         * @param {bytes} buffer file buffer
         * @param {string} type file type
         */
        HelperArrayBufferToBlob: (buffer, type) => HelperArrayBufferToBlob(buffer, type),
        /**
         * Promise to get a file from a da from a DB for compatibility with most of the browsers
         * @param {bytes} blob arraybuffer byte to retrive
         */
        HelperBlobToArrayBuffer: (blob) => HelperBlobToArrayBuffer(blob)
    };
})();

