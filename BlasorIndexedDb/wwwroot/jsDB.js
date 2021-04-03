class jsDB {
    #DB_NAME = 'MyDB';      //db name
    #DB_VERSION = 1;        //db version
    #MODELS;                //Table model definitions
    constructor(model) {
        if (!this.#OpenDB()) {
            throw "IndexedDB not compatible!"
        }
        else {
            if (model.name) {
                this.#DB_NAME = model.name;
                this.#DB_VERSION = model.version;
                this.#MODELS = model.tables;
                const dbconnect = this.#OpenDB().open(this.#DB_NAME, this.#DB_VERSION);
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
    #OpenDB() {
        try {
            const dbconnect = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB, IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.OIDBTransaction || window.msIDBTransaction;
            return dbconnect;
        } catch (error) {
            throw this.#SetResponse(false, error.message);
        }
    }
    /**
     * Return a JSON with {result: true/false, message: "message to show to the user"}
     * @param {bool} r 
     * @param {string} m message to show
     */
    #SetResponse(r, m) {
        return { result: r, message: m };
    }
    /**
     * Return true or false if the key exist into the object send
     * @param {object} obj 
     * @param {string} val 
     */
    #HasKeyPath(obj, val) {
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
    #MergeObjects(obj1, obj2) {
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
    #UpdateModel(obj1, obj2) {
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
    #GetDefault(table) {
        const context = this;
        const model = context.#MODELS.find(el => el.name === table)
        if (model) {
            let defaultObj = '{';       //to create a default json for the table
            //compare object receive with table definition
            //check primary key            
            if (model.options && model.options.keyPath && model.options.autoIncrement && model.options.autoIncrement == false) {
                //have a primary key required
                if (context.#HasKeyPath(o, model.options.keyPath)) {
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
    #CheckModel(table, s) {
        const context = this;
        const model = this.#GetDefault(table);
        if (model) {
            let canContinue = true;
            for (let key in s) {
                if (!Object.hasOwnProperty.call(model, key)) {
                    canContinue = false;
                    break;
                }
            }
            if (canContinue) return context.#MergeObjects(model, s);
            else return null;
        }
        else {
            return null;
        }
    }
    #IsArray(object) {
        return object.constructor === Array;
    }
    Connected() {
        return 'Connected to ' + this.#DB_NAME + ' with version ' + this.#DB_VERSION;
    }
    /**
     * Request all data from a table name. Return the data or JSON response
     * @param {string} table table name to request the data
     */
    Select(table) {
        const context = this;
        return new Promise(function (resolve, error) {
            const dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readonly');
                    const store = transaction.objectStore(table);
                    const request = store.getAll();
                    request.onerror = ev => {
                        db.close();
                        error(context.#SetResponse(false, ev.target.error.message));
                    };
                    request.onsuccess = () => {
                        db.close();
                        resolve(request.result);
                    };
                } catch (e) {
                    db.close();
                    error(context.#SetResponse(false, e.message));
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
            const dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readonly');
                    const store = transaction.objectStore(table);
                    const request = store.getAll(id);
                    request.onerror = ev => {
                        db.close();
                        error(context.#SetResponse(false, ev.target.error.message));
                    };
                    request.onsuccess = function () {
                        db.close();
                        resolve(request.result);
                    }
                } catch (e) {
                    db.close();
                    error(context.#SetResponse(false, e.message));
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
            const dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readonly');
                    const store = transaction.objectStore(table);
                    const index = store.index(column);
                    const request = index.getAll(value);
                    request.onerror = ev => {
                        db.close();
                        error(context.#SetResponse(false, ev.target.error.message));
                    };
                    request.onsuccess = function () {
                        db.close();
                        resolve(request.result);
                    }
                } catch (e) {
                    db.close();
                    error(context.#SetResponse(false, e.message));
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
        return new Promise(function (resolve, error) {
            if (!context.#IsArray(data)) {
                let array = [];
                array.push(data);
                data = array;
            }
            data.forEach(element => {
                const obj = context.#CheckModel(table, element);
                if (obj === null) {
                    error(context.#SetResponse(false, "Model doesn't match"));
                    return;
                }
            });
            const dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readwrite');
                    const store = transaction.objectStore(table);
                    const defaultModel = context.#GetDefault(table);
                    //get who is the keypath
                    const model = context.#MODELS.find(el => el.name === table);
                    let keyPath = model.options.keyPath;
                    if (!keyPath) {
                        keyPath = 'ssnId';
                    }
                    data.forEach(el => {
                        let o = context.#MergeObjects(defaultModel, el);
                        store.add(o);
                    });
                    transaction.onerror = ev => {
                        db.close();
                        resolve([context.#SetResponse(false, ev.target.error.message)]);
                    };
                    transaction.oncomplete = () => {
                        db.close();
                        resolve([context.#SetResponse(true, 'Insert done!')]);
                    };
                } catch (e) {
                    db.close();
                    error([context.#SetResponse(false, e.message)]);
                }
            };

        });
    }
    /**
     * Update data into the table. The data always must be content all the columns, if not the function retreive the actual data to keep always same data into a table. Alway return a JSON response
     * @param {string} table table name
     * @param {JSON} data data with the model format to update
     */
    Update(table, data) {
        const context = this;
        return new Promise(function (resolve, error) {
            if (!context.#IsArray(data)) {
                let array = [];
                array.push(data);
                data = array;
            }
            data.forEach(element => {
                const obj = context.#CheckModel(table, element);
                if (obj === null) {
                    error(context.#SetResponse(false, "Model doesn't match"));
                    return;
                }
            });
            const dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
            dbconnect.onsuccess = function () {
                let db = this.result;
                try {
                    const transaction = db.transaction(table, 'readwrite');
                    const store = transaction.objectStore(table);
                    //get the keypath to retrieve the actual data must be updated
                    const model = context.#MODELS.find(el => el.name === table);
                    let keyPath = model.options.keyPath;
                    if (!keyPath) {
                        keyPath = 'ssnId';
                    }
                    let result = new Array();
                    data.forEach(el => {
                        let o = context.#CheckModel(table, el);
                        let ssnId = o[keyPath];
                        if (ssnId) {
                            //retrieve the actual data for the index about the element
                            let request = store.get(ssnId);

                            request.onsuccess = function (e) {
                                let data = context.#UpdateModel(e.target.result, o);
                                const objRequest = store.put(data);

                                objRequest.onsuccess = function (e) {
                                    result.push(context.#SetResponse(true, `Success in updating record ${ssnId}`));
                                };

                                objRequest.onerror = ev => {
                                    result.push(context.#SetResponse(false, `Error in updating record ${ssnId}`));
                                };
                            }
                        }
                        else {
                            let data = context.#MergeObjects(o, el);
                            store.put(data);
                        }
                    });
                    transaction.onerror = ev => {
                        db.close();
                        result.push(context.#SetResponse(false, ev.target.error.message));
                        resolve(result);
                    };
                    transaction.oncomplete = () => {
                        db.close();
                        resolve(result);
                    };
                } catch (e) {
                    db.close();
                    error([context.#SetResponse(false, e.message)]);
                }
            };

        });
    }
    /**
     * Dete one row from a table. Alway return a JSON response
     * @param {string} table table name
     * @param {any} id value from the keypath
     */
    Delete(table, id) {
        const context = this;
        return new Promise(function (resolve, error) {
            const dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readwrite');
                    const store = transaction.objectStore(table);
                    store.delete(id);
                    transaction.onerror = ev => {
                        db.close();
                        resolve({ result: false, message: ev.target.error.message });
                    };
                    transaction.onsuccess = () => {
                        db.close();
                        resolve({ result: true, message: 'Delete done!' });
                    };
                } catch (e) {
                    db.close();
                    error(context.#SetResponse(false, e.message));
                }
            };
        });
    }
    /**
     * Drop a table. Alway return a JSON response
     * @param {string} table table name
     */
    Drop(table) {
        const context = this;
        return new Promise(function (resolve, error) {
            const dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readwrite');
                    const store = transaction.objectStore(table);
                    const req = store.clear();
                    req.onerror = ev => {
                        db.close();
                        error({ result: false, message: ev.target.error.message });
                    };
                    req.onsuccess = () => {
                        db.close();
                        resolve({ result: true, message: 'Drop done!' });
                    };
                } catch (e) {
                    db.close();
                    error(context.#SetResponse(false, e.message));
                }
            };
        });
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
     * @param {bytes} blob arraybuffer byte to retrive
     */
    HelperBlobToArrayBuffer(blob) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.addEventListener('loadend', (e) => {
                resolve(reader.result);
            });
            reader.addEventListener('error', reject);
            reader.readAsArrayBuffer(blob);
        });
    }
}