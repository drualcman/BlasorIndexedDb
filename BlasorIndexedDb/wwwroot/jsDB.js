class jsDB {
    DB_NAME = 'MyDB';      //db name
    DB_VERSION = 1;        //db version
    MODELS;                //Table model definitions
    constructor(model) {
        if (!this.OpenDB()) {
            throw "IndexedDB not compatible!"
        }
        else {
            if (model.name) {
                this.DB_NAME = model.name;
                this.DB_VERSION = model.version;
                this.MODELS = model.tables;
                const dbconnect = this.OpenDB().open(this.DB_NAME, this.DB_VERSION);
                dbconnect.onblocked = ev => {
                    // If some other tab is loaded with the database, then it needs to be closed
                    // before we can proceed.
                    alert("Please close all other tabs with this site open!");
                };
                dbconnect.onupgradeneeded = ev => {
                    //create database with the model send
                    const db = ev.target.result;
                    const tables = model.tables;
                    const t = tables.length;
                    //get tables
                    if (t > 0) {
                        for (let ti = 0; ti < t; ti++) {
                            let tabla = tables[ti];
                            let columns = tabla.columns;
                            let c = columns.length;
                            //get columns
                            if (c > 0) {
                                let options = tabla.options;
                                let keyPath;
                                let increment;
                                if (options) {
                                    if (options.keyPath) {
                                        keyPath = options.keyPath;
                                        if (options.autoIncrement) increment = options.autoIncrement;
                                        else increment = false;
                                    }
                                    else {
                                        keyPath = 'ssnId';
                                        increment = true;
                                    }
                                }
                                else {
                                    keyPath = 'ssnId';
                                    increment = true;
                                }
                                //add table
                                let objectStore = db.createObjectStore(tabla.name, { keyPath: keyPath, autoIncrement: increment });
                                for (let ci = 0; ci < c; ci++) {
                                    //add columns
                                    let column = columns[ci];
                                    objectStore.createIndex(column.name, column.name, { keyPath: column.keyPath, autoIncrement: column.autoIncrement, unique: column.unique });
                                }
                            }
                        }
                    }
                    else throw "No tables defined.";
                };
                dbconnect.onerror = e => {
                    console.warn(e.error.message);
                    throw e.target.error.message;
                }
                dbconnect.onsuccess = () => {
                    return;
                }
            }
            else throw "Please provide db model {name: 'MyDB', version: 1, tables: [{name: 'Table1', options: {keyPath : 'Id', autoIncrement: true/false}, columns: [{name: 'ColumnName', keyPath: true/false, autoIncrement: true/false, unique: true/false }]}]}"
        }
    }
    /**
     * Open a indexedDb with all the compatibilities from the browsers
     */
    OpenDB() {
        try {
            const dbconnect = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB, IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.OIDBTransaction || window.msIDBTransaction;
            return dbconnect;
        } catch (error) {
            throw [this.SetResponse(false, error.message)];
        }
    }
    /**
     * Return a JSON with {result: true/false, message: "message to show to the user"}
     * @param {bool} r 
     * @param {string} m message to show
     */
    SetResponse(r, m) {
        return { Result: r, Message: m };
    }
    /**
     * Return true or false if the key exist into the object send
     * @param {object} obj 
     * @param {string} val 
     */
    HasKeyPath(obj, val) {
        let found = false;
        for (let key in obj) {
            if (Object.hasOwnProperty.call(obj, key)) {
                let element = obj[key];
                for (let i in element) {
                    if (i == val) found = true;
                }
            }
        }
        return found;
    }
    /**
     * Merge all properties from 2 objects. Can't have object inside object.
     * @param {object} obj1 object model with the fields
     * @param {object} obj2 object with the data to add into the model
     */
    MergeObjects(obj1, obj2) {
        var obj3 = {};
        var obj4 = {};
        //set the 2 objects exactly with the same properties
        for (let k1 in obj1) {
            if (Object.hasOwnProperty.call(obj1, k1)) {
                let e1 = obj1[k1];
                for (let k2 in obj2) {
                    if (k1 == k2) obj3[k2] = obj2[k2];
                    else continue;
                }
            }
        }
        //set the values equal on the merged object
        for (let attrname in obj1) { obj4[attrname] = obj1[attrname]; }
        for (let attrname1 in obj3) { obj4[attrname1] = obj3[attrname1]; }
        return obj4;
    }
    /**
     * Update.
     * @param {object} obj1 record from a db
     * @param {object} obj2 record to update
     */
    UpdateModel(obj1, obj2) {
        var obj3 = {};
        //set the values equal on the merged object
        for (let attrname in obj1) { obj3[attrname] = obj1[attrname]; }
        for (let attrname1 in obj3) {
            if (obj2[attrname1]) obj3[attrname1] = obj2[attrname1];     //only if the property exists update
            else continue;
        }
        return obj3;
    }
    /**
     * Get default model from the table's definition
     * @param {string} table table to compare the model selected 
     * @returns {obj} return object with model
     */
    GetDefault(table) {
        const context = this;
        const model = context.MODELS.find(el => el.name === table)
        if (model) {
            let defaultObj = '{';       //to create a default json for the table
            //compare object receive with table definition
            //check primary key            
            if (model.options && model.options.keyPath && model.options.autoIncrement && model.options.autoIncrement == false) {
                //have a primary key required
                if (context.HasKeyPath(o, model.options.keyPath)) {
                    defaultObj += '"' + model.options.keyPath + '": null,';
                }
                else defaultObj += '"ssnId": null,';
            }
            else {
                if (model.options && model.options.keyPath) defaultObj += '"' + model.options.keyPath + '": null,';
                else defaultObj += '"ssnId": null,';
            }
            //create a default json object for the table
            const c = model.columns.length;
            for (let index = 0; index < c; index++) {
                defaultObj += '"' + model.columns[index].name + '": null,';
            }
            defaultObj = defaultObj.substring(0, defaultObj.length - 1);        //remove last coma
            defaultObj += '}';
            return JSON.parse(defaultObj);
        }
        else {
            return null;
        }
    }
    /**
     * Check if the data send match with the table definition
     * @param {string} table table to compare the model selected 
     * @param {obj} s object to compare
     * @returns {obj} return same object with all the fields
     */
    CheckModel(table, s) {
        const context = this;
        const model = this.GetDefault(table);
        if (model) {
            let canContinue = true;
            for (let key in s) {
                if (!Object.hasOwnProperty.call(model, key)) {
                    if (key !== 'OffLine') canContinue = false;
                    break;
                }
            }
            if (canContinue) return context.MergeObjects(model, s);
            else return null;
        }
        else {
            return null;
        }
    }
    IsArray(object) {
        return object.constructor === Array;
    }
    Connected() {
        return 'Connected to ' + this.DB_NAME + ' with version ' + this.DB_VERSION;
    }
    /**
     * Request all data from a table name. Return the data or JSON response
     * @param {string} table table name to request the data
     */
    Select(table) {
        const context = this;
        return new Promise(function (resolve, error) {
            const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readonly');
                    const store = transaction.objectStore(table);
                    const request = store.getAll();
                    request.onerror = ev => {
                        db.close();
                        console.warn(ev.target.error.message);
                        resolve([context.SetResponse(false, ev.target.error.message)]);
                    };
                    request.onsuccess = () => {
                        db.close();
                        resolve(request.result);
                    };
                } catch (e) {
                    db.close();
                    console.error(e);
                    error([context.SetResponse(false, e.message)]);
                }
            }
        });
    }
    /**
     * Request data from a table name with the keyPath with a value. Return the data or JSON response
     * @param {string} table table name
     * @param {any} id id value
     */
    SelectId(table, id) {
        const context = this;
        return new Promise(function (resolve, error) {
            const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readonly');
                    const store = transaction.objectStore(table);
                    const request = store.getAll(id);
                    request.onerror = ev => {
                        db.close();
                        console.warn(ev.target.error.message);
                        resolve([context.SetResponse(false, ev.target.error.message)]);
                    };
                    request.onsuccess = () => {
                        db.close();
                        resolve(request.result);
                    }
                } catch (e) {
                    db.close();
                    console.error(e);
                    error([context.SetResponse(false, e.message)]);
                }
            }
        });
    }
    /**
     * Request data from a table name filtered by column name and value. Return the data or JSON response
     * @param {string} table table name
     * @param {string} column column name to filter
     * @param {any} value value for the filter
     */
    SelectWhere(table, column, value) {
        const context = this;
        return new Promise(function (resolve, error) {
            const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readonly');
                    const store = transaction.objectStore(table);
                    const index = store.index(column);
                    const request = index.getAll(value);
                    request.onerror = ev => {
                        db.close();
                        console.warn(ev.target.error.message);
                        resolve([context.SetResponse(false, ev.target.error.message)]);
                    };
                    request.onsuccess = () => {
                        db.close();
                        resolve(request.result);
                    }
                } catch (e) {
                    db.close();
                    console.error(e);
                    error([context.SetResponse(false, e.message)]);
                }
            }
        });

    }
    /**
     * Insert data into a table. Alway return a JSON response
     * @param {string} table table name
     * @param {JSON} data data with the model format to insert
     */
    Insert(table, data) {
        const context = this;
        let transactionResult = new Array();
        return new Promise(function (ok, bad) {
            try {
                let result = new Promise(function (resolve, error) {
                    try {
                        if (!context.IsArray(data)) {
                            let array = new Array();
                            array.push(data);
                            data = array;
                        }
                        data.forEach(element => {
                            const obj = context.CheckModel(table, element);
                            if (obj === null) {
                                transactionResult.push(context.SetResponse(false, "Model doesn't match"));
                            }
                            else {
                                let dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
                                dbconnect.onsuccess = function () {
                                    let db = this.result;
                                    try {
                                        let transaction = db.transaction(table, 'readwrite');
                                        let store = transaction.objectStore(table);
                                        let defaultModel = context.GetDefault(table);
                                        //get who is the keypath
                                        let model = context.MODELS.find(el => el.name === table);
                                        let keyPath = model.options.keyPath;
                                        let o = context.MergeObjects(defaultModel, element);
                                        if (o[keyPath] === null) {
                                            delete o[keyPath];
                                        }
                                        store.add(o);
                                        transaction.onerror = ev => {
                                            db.close();
                                            transactionResult.push(context.SetResponse(false, ev.target.error.message));
                                            console.warn(ev.target.error.message);
                                        };
                                        transaction.oncomplete = () => {
                                            db.close();
                                            transactionResult.push(context.SetResponse(true, 'Insert done!'));
                                        };
                                    } catch (e) {
                                        db.close();
                                        transactionResult.push(context.SetResponse(false, e.message));
                                        console.error(e);
                                    }
                                };
                            }
                        });
                        resolve(data.length);
                    } catch (e) {
                        error([context.SetResponse(false, e.message)]);
                    }
                });

                result.then(function (rows) {
                    setTimeout(() => {
                        ok(transactionResult);
                    }, rows * 50);
                }, bad);
            } catch (e) {
                bad([context.SetResponse(false, e.message)]);
            }
        });

    }
    /**
     * Update data into the table. The data always must be content all the columns, 
     * if not the function retrieve the actual data to keep always same data into a table. 
     * Alway return a JSON response
     * @param {string} table table name
     * @param {JSON} data data with the model format to update
     */
    Update(table, data) {
        const context = this;
        let transactionResult = new Array();
        return new Promise(function (ok, bad) {
            try {
                let result = new Promise(function (resolve, error) {
                    try {
                        if (!context.IsArray(data)) {
                            let array = new Array();
                            array.push(data);
                            data = array;
                        }
                        data.forEach(element => {
                            let obj = context.CheckModel(table, element);
                            if (obj === null) {
                                transactionResult.push(context.SetResponse(false, "Model doesn't match"));
                            }
                            else {
                                let dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
                                dbconnect.onsuccess = function () {
                                    let db = this.result;
                                    try {
                                        let transaction = db.transaction(table, 'readwrite');
                                        let store = transaction.objectStore(table);
                                        //get the keypath to retrieve the actual data must be updated
                                        let model = context.MODELS.find(el => el.name === table);
                                        let keyPath = model.options.keyPath;
                                        if (!keyPath) {
                                            keyPath = 'ssnId';
                                        }
                                        let o = context.CheckModel(table, element);
                                        let ssnId = o[keyPath];
                                        if (ssnId) {
                                            let request = store.get(ssnId);
                                            //retrieve the actual data for the index about the element
                                            request.onsuccess = function () {
                                                let dat = context.UpdateModel(request.result, o);
                                                let objRequest = store.put(dat);
                                                objRequest.onsuccess = () => {
                                                    transactionResult.push(context.SetResponse(true, `Success in updating record ${ssnId}`));
                                                };
                                                objRequest.onerror = () => {
                                                    transactionResult.push(context.SetResponse(false, `Error in updating record ${ssnId}`));
                                                };
                                            }
                                        }
                                        else {
                                            let data = context.MergeObjects(o, element);
                                            store.put(data);
                                            transaction.oncomplete = () => {
                                                db.close();
                                                transactionResult.push(context.SetResponse(true, 'Insert done!'));
                                            };
                                            transaction.onerror = ev => {
                                                db.close();
                                                transactionResult.push(context.SetResponse(false, ev.target.error.message));
                                                console.warn(ev.target.error.message);
                                            };
                                        }
                                    } catch (e) {
                                        db.close();
                                        transactionResult.push(context.SetResponse(false, e.message));
                                        console.error(e);
                                    }
                                };
                            }
                        });
                        resolve(data.length);
                    } catch (e) {
                        error([context.SetResponse(false, e.message)]);
                    }
                });
                result.then(function (rows) {
                    setTimeout(() => {
                        ok(transactionResult);
                    }, rows * 50);
                }, bad);
            } catch (e) {
                bad([context.SetResponse(false, e.message)]);
            }
        });
    }
    /**
     * Delete one row from a table. Alway return a JSON response
     * @param {string} table table name
     * @param {any} id value from the keypath
     */
    Delete(table, id) {
        const context = this;
        return new Promise(function (resolve, error) {
            const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readwrite');
                    const store = transaction.objectStore(table);
                    store.delete(id);
                    transaction.onerror = ev => {
                        db.close();
                        console.warn(ev.target.error.message);
                        resolve([context.SetResponse(false, ev.target.error.message)]);
                    };
                    transaction.oncomplete = r => {
                        db.close();
                        resolve([context.SetResponse(true, 'Delete done!')]);
                    };
                } catch (e) {
                    db.close();
                    console.error(e);
                    error([context.SetResponse(false, e.message)]);
                }
            };
        });
    }
    /**
     * Delete all the data from a table. Alway return a JSON response
     * @param {string} table table name
     */
    Clean(table) {
        const context = this;
        return new Promise(function (resolve, error) {
            const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readwrite');
                    const store = transaction.objectStore(table);
                    store.clear();
                    transaction.oncomplete = () => {
                        db.close();
                        resolve([context.SetResponse(true, `Table ${table} is empty`)]);
                    };
                    transaction.onerror = ev => {
                        db.close();
                        console.warn(ev.target.error.message);
                        resolve([context.SetResponse(false, ev.target.error.message)]);
                    };
                } catch (e) {
                    db.close();
                    error([context.SetResponse(false, e.message)]);
                    console.error(e);
                }
            };
        });
    }
    /**
     * Drop a table. Alway return a JSON response
     * @param {string} table table name
     */
    Drop(table) {
        return this.Clean(table);
        // const context = this;
        // return new Promise(function (resolve, error) {
        //     const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION + 1);
        //     dbconnect.onupgradeneeded = ev => {
        //         var db = dbconnect.result;
        //         db.deleteObjectStore(table);          
        //         db.close();
        //     };
        //     dbconnect.onsuccess = function () {     
        //         const dbconnectw = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);    
        //         dbconnectw.onupgradeneeded = ev => {
        //             var db = dbconnectw.result;         
        //             db.close();
        //         };      
        //         dbconnectw.onsuccess = function () {   
        //             resolve([context.SetResponse(true, 'Drop done!')]);
        //         };
        //         dbconnectw.onerror = function (ev) {               
        //             resolve([context.SetResponse(false, ev.target.error.message)]);
        //         };
        //     };
        //     dbconnect.onerror = function (ev) {               
        //         error([context.SetResponse(false, ev.target.error.message)]);
        //     };
        // });
    }
    /**
     * To store files into a db for compatibility with most of the browsers
     * @param {bytes} buffer file buffer
     * @param {string} type file type
     */
    HelperArrayBufferToBlob(buffer, type) {
        return new Blob([buffer], { type: type });
    }
    /**
     * Promise to get a file from a DB for compatibility with most of the browsers
     * @param {bytes} blob arraybuffer byte to retrieve
     */
    HelperBlobToArrayBuffer(blob) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.addEventListener('loadend', () => {
                resolve(reader.result);
            });
            reader.addEventListener('error', reject);
            reader.readAsArrayBuffer(blob);
        });
    }
}