
(function () {
    var db;
    window.MyDb = {
        /**
         * Initializing instance of a DB with a model send
         * @param {any} model
         */
        Init: (model) => {
            db = new jsDB(JSON.parse(model))
        },                
        Connected: () => db.Connected(),
        /**
         * Request all data from a table name. Return the data or JSON response
         * @param {string} table table name to request the data
         * @param {function} callBack function to receive the result.
         */
        Select: (table, callBack) => db.Select(table, callBack),
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
         * @param {function} callBack function to receive result
         */
        Insert: (table, data, callBack) => db.Insert(table, data, callBack),
        /**
         * Update data into the table. The data always must be content all the columns, if not the function retreive the actual data to keep always same data into a table. Alway return a JSON response
         * @param {string} table table name
         * @param {JSON} data data with the model format to update
         * @param {function} callBack function to receive the result
         */
        Update: (table, data, callBack) => Update(table, data, callBack),
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

