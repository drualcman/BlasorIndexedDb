
(function () {
    var db;
    window.MyDb = {
        /**
         * Initializing instance of a DB with a model send
         * @param {any} model
         */
        Init: (model) => db = new jsDB(JSON.parse(model)),
        Connected: () => db.Connected(),
        /**
         * Request all data from a table name. Return the data or JSON response
         * @param {string} table table name to request the data
         */
        Select: (table) => db.SelectId(table),
        /**
         * Request data from a table name with the keyPath with a value. Return the data or JSON response
         * @param {string} table table name
         * @param {any} id id value
         */
        SelectId: (table, id) => db.SelectId(table, id),
        /**
         * Request data from a table name filtered by column name and value. Return the data or JSON response
         * @param {string} table table name
         * @param {string} column column name to filter
         * @param {any} value value for the filter
         */
        SelectWhere: (table, column, value) => db.SelectWhere(table, column, value),
        /**
         * Insert data into a table. Alway return a JSON response
         * @param {string} table table name
         * @param {JSON} data data with the model format to insert
         */
        Insert: (table, data) => db.Insert(table, JSON.parse(data)),
        /**
         * Update data into the table. The data always must be content all the columns, if not the function retreive the actual data to keep always same data into a table. Alway return a JSON response
         * @param {string} table table name
         * @param {JSON} data data with the model format to update
         */
        Update: (table, data) => db.Update(table, JSON.parse(data)),
        /**
         * Delete one row from a table. Alway return a JSON response
         * @param {string} table table name
         * @param {any} id value from the keypath
         */
        Delete: (table, id) => db.Delete(table, id),
        /**
         * Create a table. Alway return a JSON response
         * @param {string} table table name
         */
        Create: (table) => db.Create(table),
        /**
         * Drop a table. Alway return a JSON response
         * @param {string} table table name
         */
        Drop: (table) => db.Drop(table),
        /**
         * To store files into a db for compatibility with most of the browsers
         * @param {bytes} buffer file buffer
         * @param {string} type file type
         */
        HelperArrayBufferToBlob: (buffer, type) => db.HelperArrayBufferToBlob(buffer, type),
        /**
         * Promise to get a file from a da from a DB for compatibility with most of the browsers
         * @param {bytes} blob arraybuffer byte to retrive
         */
        HelperBlobToArrayBuffer: (blob) => db.HelperBlobToArrayBuffer(blob)
    };
})();

