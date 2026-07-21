class jsDB {
    DB_NAME = 'MyDB';      //db name
    DB_VERSION = 1;        //db version
    MODELS;                //Table model definitions

    constructor() {
        console.info('Database jsDB instance created!');
    }

    Initialize(dbModel) {
        return new Promise((resolve, reject) => {
            if (!this.OpenDB()) {
                reject([this.SetResponse(false, `${this.DB_NAME} => IndexedDB not compatible!`)]);
            }
            else {
                if (dbModel.name) {
                    this.DB_NAME = dbModel.name;
                    this.DB_VERSION = dbModel.version;
                    this.MODELS = dbModel.tables; 
                    const dbconnect = this.OpenDB().open(this.DB_NAME, this.DB_VERSION);
                    dbconnect.onblocked = ev => {
                        // If some other tab is loaded with the database, then it needs to be closed
                        // before we can proceed.
                        alert("Please close all other tabs with this site open!");
                        reject([this.SetResponse(false, `${this.DB_NAME} => OnBlocked Error: Please close all other tabs with this site open!`)]);
                        this.close();
                    };
                    dbconnect.onupgradeneeded = ev => {
                        //create database with the model send
                        const db = ev.target.result;
                        const t = this.MODELS.length;
                        //get tables
                        if (t > 0) {
                            for (let ti = 0; ti < t; ti++) {
                                let tabla = this.MODELS[ti];
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
                                    //if table exist delete, data will lost
                                    if (db.objectStoreNames.contains(tabla.name)) {
                                        db.deleteObjectStore(tabla.name);
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
                        else reject([this.SetResponse(false, `${this.DB_NAME} => OnUpgrade Error: No tables defined.`)]);
                    };
                    dbconnect.onerror = e => {
                        console.warn(`${this.DB_NAME} => Connection Error: ${e.target.error.message}`);
                        reject([this.SetResponse(false, `${this.DB_NAME} => Connection Error: ${e.target.error.message}`)]);
                    }
                    dbconnect.onsuccess = () => {
                        resolve();
                        return;
                    }
                }
                else reject([this.SetResponse(false, `${this.DB_NAME} => Error: Please provide db model {name: 'MyDB', version: 1, tables: [{name: 'Table1', options: {keyPath : 'Id', autoIncrement: true/false}, columns: [{name: 'ColumnName', keyPath: true/false, autoIncrement: true/false, unique: true/false }]}]}`)]);
            }
        });
    }

    /**
     * Open a indexedDb with all the compatibilities from the browsers
     */
    OpenDB() {
        try {
            const dbconnect = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB
            return dbconnect;
        } catch (error) {
            throw [this.SetResponse(false, `Not suported indexedDb ${error.message}`)];
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
     * Return true or false if the key exist info the object send
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
     * @param {object} obj2 object with the data to add info the model
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
            if (typeof obj2[attrname1] !== 'undefined') obj3[attrname1] = obj2[attrname1];     //only if the property exists update
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
        const model = context.MODELS.find(el => el.name === table);
        if (model) {
            let defaultObj = '{';       //to create a default json for the table
            //compare object receive with table definition
            //check primary key            
            if (model.options && model.options.keyPath) {
                //have a primary key required
                defaultObj += '"' + model.options.keyPath + '": null,';
            }
            else {
                defaultObj += '"ssnId": null,';
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
        return this.CheckModelWith(this.GetDefault(table), s);
    }
    /**
     * Same as CheckModel, but against a default model the caller already has.
     * @remarks GetDefault builds its result by concatenating a Json string and parsing it, so it is
     * not something to call once per row when every row in a batch shares the same table.
     * @param {obj} model default model for the table, from GetDefault
     * @param {obj} s object to compare
     * @returns {obj} return same object with all the fields
     */
    CheckModelWith(model, s) {
        if (!model) return null;
        let canContinue = true;
        for (let key in s) {
            if (!Object.hasOwnProperty.call(model, key)) {
                if (key !== 'OffLine') canContinue = false;
                break;
            }
        }
        return canContinue ? this.MergeObjects(model, s) : null;
    }
    IsArray(object) {
        return object.constructor === Array;
    }
    Connected() {
        return 'Connected to ' + this.DB_NAME + ' with version ' + this.DB_VERSION;
    }
    /**
     * Select the correct database
     * @param {any} database
     * @param {any} version
     * @param {any} models
     */
    SetDataBaseName(database, version, models) {
        this.DB_NAME = database;
        this.DB_VERSION = version;
        if (models !== null && models !== undefined) this.MODELS = JSON.parse(models);
    }
    /**
     * Request all data from a table name. Return the data or JSON response
     * @param {string} table table name to request the data
     */
    async Select(table) {
        const context = this;
        try {
            return new Promise((resolve, error) => {
                if (table == null) error([context.SetResponse(false, 'Not store name selected')]);
                else {
                    const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
                    dbconnect.onsuccess = function () {
                        const db = this.result;
                        try {
                            const transaction = db.transaction(table, 'readonly');
                            const store = transaction.objectStore(table);
                            const request = store.getAll();
                            request.onerror = ev => {
                                db.close();
                                resolve([context.SetResponse(false, `${table}: ${ev.target.error.message}`)]);
                                console.warn(`${table}: ${ev.target.error.message}`);
                            };
                            request.onsuccess = async ev => {
                                db.close();
                                const data = await JsonToBytes(request.result);
                                resolve(data);
                            };
                        } catch (e) {
                            db.close();
                            error([context.SetResponse(false, `${table}: ${e.message}`)]);
                            console.warn(`${table}: ${e.message}`);
                        }
                    }
                    dbconnect.onerror = function (e) {
                        this.close();
                        console.warn(`${table}: ${e.message}`);
                    }
                }
            });
        } catch (e) {
            console.warn(e);
        }
    }
    /**
     * Request data from a table name with the keyPath with a value. Return the data or JSON response
     * @param {string} table table name
     * @param {any} id id value
     */
    async SelectId(table, id) {
        const context = this;
        return new Promise((resolve, error) => {
            if (table == null) error([context.SetResponse(false, 'Not store name selected')]);
            else {
                const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
                dbconnect.onsuccess = function () {
                    const db = this.result;
                    try {
                        const transaction = db.transaction(table, 'readonly');
                        const store = transaction.objectStore(table);
                        const range = IDBKeyRange.only(value);
                        const request = store.getAll(range);
                        request.onerror = ev => {
                            db.close();
                            resolve([context.SetResponse(false, `${table}.${id}: ${ev.target.error.message}`)]);
                            console.warn(`${table},${id}: ${ev.target.error.message}`);
                        };
                        request.onsuccess = async () => {
                            db.close();
                            const data = await JsonToBytes(request.result);
                            resolve(data);
                        }
                    } catch (e) {
                        db.close();
                        error([context.SetResponse(false, `${table}.${id}: ${e.message}`)]);
                    }
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
    async SelectWhere(table, column, value) {
        const context = this;
        return new Promise((resolve, error) => {
            if (table == null) error([context.SetResponse(false, 'Not store name selected')]);
            else {
                const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
                dbconnect.onsuccess = function () {
                    const db = this.result;
                    try {
                        const transaction = db.transaction(table, 'readonly');
                        const store = transaction.objectStore(table);
                        const index = store.index(column);
                        const range = IDBKeyRange.only(value);
                        const request = index.getAll(range);
                        request.onerror = ev => {
                            db.close();
                            resolve([context.SetResponse(false, `${table}.${column}.${value}: ${ev.target.error.message}`)]);
                            console.warn(`${table}.${column}.${value}: ${ev.target.error.message}`);
                        };
                        request.onsuccess = async () => {
                            db.close();
                            const data = await JsonToBytes(request.result);
                            resolve(data);
                        }
                    } catch (e) {
                        db.close();
                        error([context.SetResponse(false, `${table}.${column}.${value}: ${e.message}`)]);
                        console.warn(`${table}.${column}.${value}: ${e.message}`);
                    }
                }
            }
        });

    }
    /**
     * Insert data info a table. Alway return a JSON response
     * @param {string} table table name
     * @param {JSON} data data with the model format to insert
     */
    async Insert(table, data) {
        const context = this;
        let transactionResult = new Array();
        return new Promise((ok, bad) => {
            if (table == null) {
                bad([context.SetResponse(false, 'Not store name selected')]);
                return;
            }
            // Settle exactly once. onerror and onabort BOTH fire when a request fails (the error
            // bubbles to the transaction and aborts it), and a resolved promise ignores the second
            // call anyway — this just keeps us from closing the connection twice.
            let settled = false;
            const finish = () => {
                if (settled) return;
                settled = true;
                ok(transactionResult);
            };
            try {
                if (!context.IsArray(data)) {
                    let array = new Array();
                    array.push(data);
                    data = array;
                }
                let toAdd = new Array();
                let canContinues = true;
                // Every row in the batch belongs to the same table, so everything that depends only
                // on the table is resolved once instead of once per row. GetDefault in particular
                // builds its object by concatenating a Json string and parsing it, and it used to
                // run TWICE per row: once inside CheckModel and once here.
                const defaultModel = context.GetDefault(table);
                const model = context.MODELS.find(el => el.name === table);
                const keyPath = model && model.options ? model.options.keyPath : null;
                const autoIncrement = !!(model && model.options && model.options.autoIncrement);
                data.forEach(element => {
                    // CheckModel validates the row and returns MergeObjects(defaultModel, element).
                    // The row was then merged a second time from the same two inputs, producing the
                    // same object, and the first result was thrown away. Merge once and keep it.
                    const o = context.CheckModelWith(defaultModel, element);
                    if (o === null) {
                        transactionResult.push(context.SetResponse(false, "Model doesn't match"));
                        canContinues = false;
                    }
                    else {
                        try {
                            if (keyPath && o[keyPath] != null && autoIncrement) {
                                delete o[keyPath];
                            }
                            toAdd.push(o);
                        } catch (e) {
                            transactionResult.push(context.SetResponse(false, `Insert => ${table}: ${e.message}`));
                            console.warn(`Insert => ${table}: ${e.message}`);
                        }
                    }
                });
                if (!canContinues) {
                    finish();
                    return;
                }

                const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
                // Opening can fail or be blocked. Nothing used to handle that, which was survivable
                // only because the old timeout resolved regardless; now it would hang forever.
                dbconnect.onerror = ev => {
                    transactionResult.push(context.SetResponse(false, `Insert => ${table}: ${ev.target.error ? ev.target.error.message : 'cannot open database'}`));
                    finish();
                };
                dbconnect.onblocked = () => {
                    transactionResult.push(context.SetResponse(false, `Insert => ${table}: database blocked`));
                    finish();
                };
                dbconnect.onsuccess = function () {
                    const db = this.result;
                    try {
                        const transaction = db.transaction(table, 'readwrite');
                        const store = transaction.objectStore(table);
                        const rows = toAdd.length;
                        let failed = 0;
                        for (let r = 0; r < rows; r++) {
                            const request = store.add(toAdd[r]);
                            // add, so a duplicated key IS rejected -- this is an insert, not an
                            // upsert. But an unhandled request error propagates and aborts the WHOLE
                            // transaction, which is why one duplicate used to throw away every other
                            // row in the batch and why nothing could report which row was at fault.
                            // preventDefault() stops that propagation: the offending row is the only
                            // one that does not get written, the rest of the transaction commits,
                            // and the caller gets a per-row failure in the response list.
                            request.onerror = ev => {
                                failed++;
                                const key = keyPath ? toAdd[r][keyPath] : undefined;
                                const reason = ev.target.error ? ev.target.error.message : 'insert failed';
                                transactionResult.push(context.SetResponse(false, `Insert => ${table}: ${key === undefined ? `row ${r}` : `record ${key}`}: ${reason}`));
                                ev.preventDefault();
                            };
                        }
                        // Resolve from the transaction's own events. This used to resolve on a
                        // setTimeout of rows * 50ms — a guess at how long the write would take —
                        // which made the cost grow by 50ms per row regardless of how fast IndexedDB
                        // actually was, and let the caller continue while the transaction was still
                        // open and the connection unclosed. oncomplete is the real "the rows are
                        // committed" signal, and it is what Clean and Delete already use.
                        transaction.oncomplete = () => {
                            db.close();
                            // The rows that failed already pushed their own response above, so this
                            // one only reports what actually went in. Aggregating the successes into
                            // a single response keeps the payload small on big batches; a failure is
                            // always reported individually because that is the part you can act on.
                            const inserted = rows - failed;
                            if (inserted > 0 || failed === 0)
                                transactionResult.push(context.SetResponse(true, failed === 0 ? 'Insert done!' : `Insert done! ${inserted} of ${rows} rows`));
                            finish();
                        };
                        transaction.onerror = ev => {
                            db.close();
                            transactionResult.push(context.SetResponse(false, ev.target.error ? ev.target.error.message : `Insert => ${table}: failed`));
                            if (ev.target.error) console.warn(ev.target.error.message);
                            finish();
                        };
                        // A failed request aborts the whole transaction and oncomplete never fires.
                        // Without this the promise would never settle — the old timeout hid it by
                        // reporting success for rows that were never written.
                        transaction.onabort = ev => {
                            db.close();
                            transactionResult.push(context.SetResponse(false, `Insert => ${table}: transaction aborted${ev.target.error ? ': ' + ev.target.error.message : ''}`));
                            finish();
                        };
                    } catch (e) {
                        db.close();
                        transactionResult.push(context.SetResponse(false, `Insert => ${table}: ${e.message}`));
                        console.warn(`Insert => ${table}: ${e.message}`);
                        finish();
                    }
                };
            } catch (e) {
                bad([context.SetResponse(false, `Insert => ${table}: ${e.message}`)]);
            }
        });

    }
    /**
     * Update data info the table. The data always must be content all the columns, 
     * if not the function retrieve the actual data to keep always same data info a table. 
     * Alway return a JSON response
     * @param {string} table table name
     * @param {JSON} data data with the model format to update
     */
    async Update(table, data) {
        const context = this;
        let transactionResult = new Array();
        return new Promise((ok, bad) => {
            if (table == null) {
                bad([context.SetResponse(false, 'Not store name selected')]);
                return;
            }
            // See Insert: settle once, from the transaction's own events.
            let settled = false;
            const finish = () => {
                if (settled) return;
                settled = true;
                ok(transactionResult);
            };
            try {
                if (!context.IsArray(data)) {
                    let array = new Array();
                    array.push(data);
                    data = array;
                }
                const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
                dbconnect.onerror = ev => {
                    transactionResult.push(context.SetResponse(false, `Update => ${table}: ${ev.target.error ? ev.target.error.message : 'cannot open database'}`));
                    finish();
                };
                dbconnect.onblocked = () => {
                    transactionResult.push(context.SetResponse(false, `Update => ${table}: database blocked`));
                    finish();
                };
                dbconnect.onsuccess = function () {
                    const db = this.result;
                    try {
                        const transaction = db.transaction(table, 'readwrite');
                        const store = transaction.objectStore(table);
                        // Resolved once for the whole batch, not once per row: see Insert. GetDefault
                        // parses a Json string it builds itself, and CheckModel calls it every time.
                        const defaultModel = context.GetDefault(table);
                        const model = context.MODELS.find(el => el.name === table);
                        //get who is the keypath
                        const keyPath = (model && model.options && model.options.keyPath) ? model.options.keyPath : 'ssnId';
                        data.forEach(element => {
                            const obj = context.CheckModelWith(defaultModel, element);
                            if (obj === null) {
                                transactionResult.push(context.SetResponse(false, "Model doesn't match"));
                            }
                            else {
                                try {
                                    let ssnId = obj[keyPath];
                                    let dat = null;
                                    let objRequest;
                                    if (ssnId) {
                                        let request = store.get(ssnId);
                                        //retrieve the actual data for the index about the element
                                        request.onsuccess = function () {
                                            dat = context.UpdateModel(request.result, obj);
                                            objRequest = store.put(dat);
                                            objRequest.onsuccess = () => {
                                                transactionResult.push(context.SetResponse(true, `Success in updating record ${ssnId}`));
                                            };
                                            objRequest.onerror = ev => {
                                                transactionResult.push(context.SetResponse(false, `Error in updating record ${ssnId}`));
                                                console.warn(ev.target.error.message);
                                            };
                                        }
                                    }
                                    else {
                                        dat = context.MergeObjects(obj, element);
                                        objRequest = store.put(dat);
                                        objRequest.onsuccess = () => {
                                            transactionResult.push(context.SetResponse(true, `Success in updating record ${ssnId}`));
                                        };
                                        objRequest.onerror = ev => {
                                            transactionResult.push(context.SetResponse(false, `Error in updating record ${ssnId}`));
                                            console.warn(ev.target.error.message);
                                        };
                                    }

                                } catch (e) {
                                    transactionResult.push(context.SetResponse(false, `Update => ${table}: ${e.message} | 2`));
                                    console.warn(`Update => ${table}: ${e.message} | 2`);
                                }
                            }
                        });
                        // Resolved from the transaction, not from a timeout. The get/put pairs above
                        // are queued from inside their own callbacks, so oncomplete is the only
                        // reliable point at which every row really has been written. This also closes
                        // the connection, which Update never did at all.
                        transaction.oncomplete = () => {
                            db.close();
                            finish();
                        };
                        transaction.onerror = ev => {
                            db.close();
                            transactionResult.push(context.SetResponse(false, ev.target.error ? ev.target.error.message : `Update => ${table}: failed`));
                            if (ev.target.error) console.warn(ev.target.error.message);
                            finish();
                        };
                        transaction.onabort = ev => {
                            db.close();
                            transactionResult.push(context.SetResponse(false, `Update => ${table}: transaction aborted${ev.target.error ? ': ' + ev.target.error.message : ''}`));
                            finish();
                        };
                    } catch (e) {
                        db.close();
                        transactionResult.push(context.SetResponse(false, `Update => ${table}: ${e.message} | 1`));
                        console.warn(`Update => ${table}: ${e.message} | 1`);
                        finish();
                    }
                };
            } catch (e) {
                bad([context.SetResponse(false, `Update => ${table}: ${e.message}`)]);
            }
        });
    }
    /**
     * Delete one or many rows from a table. Alway return a JSON response
     * @param {string} table table name
     * @param {any} id value from the keypath, or an array of values to delete in one transaction
     */
    async Delete(table, id) {
        const context = this;
        return new Promise((resolve, error) => {
            if (table == null) {
                error([context.SetResponse(false, 'Not store name selected')]);
                return;
            }
            // A single id and a list of ids take the same path now. Deleting N rows used to mean N
            // calls, each opening its own connection and its own transaction; they all go into one
            // transaction here, which is what made the batch cost N times more than it had to.
            const ids = context.IsArray(id) ? id : [id];
            let settled = false;
            const finish = result => {
                if (settled) return;
                settled = true;
                resolve(result);
            };
            if (ids.length === 0) {
                finish([context.SetResponse(true, 'Delete done! Nothing to delete')]);
                return;
            }
            const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
            dbconnect.onerror = ev => finish([context.SetResponse(false, `Delete => ${table}: ${ev.target.error ? ev.target.error.message : 'cannot open database'}`)]);
            dbconnect.onblocked = () => finish([context.SetResponse(false, `Delete => ${table}: database blocked`)]);
            dbconnect.onsuccess = function () {
                const db = this.result;
                try {
                    const transaction = db.transaction(table, 'readwrite');
                    const store = transaction.objectStore(table);
                    for (let i = 0; i < ids.length; i++) {
                        store.delete(ids[i]);
                    }
                    transaction.oncomplete = () => {
                        db.close();
                        finish([context.SetResponse(true, ids.length === 1 ? 'Delete done!' : `Delete done! ${ids.length} rows`)]);
                    };
                    transaction.onerror = ev => {
                        db.close();
                        if (ev.target.error) console.warn(ev.target.error.message);
                        finish([context.SetResponse(false, ev.target.error ? ev.target.error.message : `Delete => ${table}: failed`)]);
                    };
                    // Same reason as in Insert: an aborted transaction never fires oncomplete, and
                    // there was no timeout here to paper over it — the promise simply hung.
                    transaction.onabort = ev => {
                        db.close();
                        finish([context.SetResponse(false, `Delete => ${table}: transaction aborted${ev.target.error ? ': ' + ev.target.error.message : ''}`)]);
                    };
                } catch (e) {
                    db.close();
                    console.warn(`Delete => ${table}: ${e.message}`);
                    if (!settled) {
                        settled = true;
                        error([context.SetResponse(false, `Delete => ${table}: ${e.message}`)]);
                    }
                }
            };
        });
    }
    /**
     * Delete all the data from a table. Alway return a JSON response
     * @param {string} table table name
     */
    async Clean(table) {
        const context = this;
        return new Promise((resolve, error) => {
            if (table == null) error([context.SetResponse(false, 'Not store name selected')]);
            else {
                const dbconnect = context.OpenDB().open(context.DB_NAME, context.DB_VERSION);
                dbconnect.onsuccess = function () {
                    const db = this.result;
                    try {
                        const transaction = db.transaction(table, 'readwrite');
                        const store = transaction.objectStore(table);
                        store.clear();
                        transaction.oncomplete = () => {
                            db.close();
                            resolve([context.SetResponse(true, `Table ${table} are empty`)]);
                        };
                        transaction.onerror = ev => {
                            db.close();
                            resolve([context.SetResponse(false, `Clean => ${table}: ${ev.target.error ? ev.target.error.message : 'failed'}`)]);
                            if (ev.target.error) console.warn(ev.target.error.message);
                        };
                        // Without this an aborted transaction leaves the promise pending forever.
                        transaction.onabort = ev => {
                            db.close();
                            resolve([context.SetResponse(false, `Clean => ${table}: transaction aborted${ev.target.error ? ': ' + ev.target.error.message : ''}`)]);
                        };
                    } catch (e) {
                        db.close();
                        error([context.SetResponse(false, `Clean => ${table}: ${e.message}`)]);
                        console.warn(e);
                    }
                };
            }
        });
    }
    /**
     * Drop a database.
     * @param {string} table table name
     */
    async Drop(database) {
        const context = this;
        return new Promise((resolve, error) => {
            if (database == null) error(context.SetResponse(false, 'Not database name selected'));
            else {
                const dbconnect = context.OpenDB();
                const request = dbconnect.deleteDatabase(database);

                request.onsuccess = function (event) {
                    resolve(context.SetResponse(true, `Database ${database} are drop`));
                };

                request.onerror = function (event) {
                    error(context.SetResponse(false, `Drop => ${database}: ${event.target.error}`));
                    console.warn(`Drop => ${database}: ${event.target.error}`);
                };

                request.onblocked = function (event) {
                    error(context.SetResponse(false, `Databse ${database} blocked. ${event.target.error}`));
                };
            }
        });
    }
    /**
     * To store files info a db for compatibility with most of the browsers
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
async function JsonToBytes(data) {
    const jsonString = JSON.stringify(data);
    const encoder = new TextEncoder();
    return encoder.encode(jsonString);
}
/**
 * Decode a UTF-8 payload coming from .NET back into an object. The counterpart of JsonToBytes:
 * .NET sends byte[] (marshalled straight through as a Uint8Array) instead of a string, which
 * avoids building and re-parsing a JS string for the whole payload on every write.
 * @param {Uint8Array} data payload received from .NET
 */
function BytesToJson(data) {
    if (data === null || data === undefined) return null;
    // Already an object (a caller that still passes JSON) -> take it as-is, so both shapes work.
    if (typeof data === 'object' && !(data instanceof Uint8Array) && !(data instanceof ArrayBuffer)) return data;
    if (typeof data === 'string') return JSON.parse(data);
    const bytes = data instanceof Uint8Array ? data : new Uint8Array(data);
    return JSON.parse(new TextDecoder().decode(bytes));
}

let Conn = (function () {
    let db;
    let MyDb = {
        /**
         * Initializing instance of a DB with a models send
         * @param {any} dbModel
        */
        Init: (dbModel) => new Promise((ok, bad) => {
            try {
                db = new jsDB();
                db.Initialize(JSON.parse(dbModel)).then(() => {
                    ok(db.DB_NAME);
                }).catch(e => {
                    console.warn(`Init failed: ${e.message}`);
                    bad(`Init: ${e.message}`);
                });
            } catch (e) {
                console.warn(`Init failed: ${e.message}`);
                bad(`Init: ${e.message}`);
            }
        }),
        Connected: (database, version) => new Promise((ok, bad) => {
            db.SetDataBaseName(database, version, null);
            try {
                ok(db.Connected());
            } catch (e) {
                bad(e);
            }
        }),
        /**
         * Request all data from a table name. Return the data or JSON response
         * @param {string} table table name to request the data
         * @param {string} database database name
         * @param {string} version database version
         * @param {string} models models to use
         */
        Select: (table, database, version, models) => new Promise(async (ok, bad) => {
            try {
                db.SetDataBaseName(database, version, models);
                await db.Select(table).then(function (result) {
                    ok(result);
                }).catch(function (error) {
                    bad(error);
                });
            } catch (e) {
                bad(e);
            }
        }),
        /**
         * Request data from a table name with the keyPath with a value. Return the data or JSON response
         * @param {string} table table name
         * @param {any} id id value
         * @param {string} database database name
         * @param {string} version database version
         * @param {string} models models to use
         */
        SelectId: (table, id, database, version, models) => new Promise(async (ok, bad) => {
            try {
                db.SetDataBaseName(database, version, models);
                await db.SelectId(table, id).then(function (result) {
                    ok(result);
                }).catch(function (error) {
                    bad(error);
                });
            } catch (e) {
                bad(e);
            }
        }),
        /**
         * Request data from a table name filtered by column name and value. Return the data or JSON response
         * @param {string} table table name
         * @param {string} column column name to filter
         * @param {any} value value for the filter
         * @param {string} database database name
         * @param {string} version database version
         * @param {string} models models to use
         */
        SelectWhere: (table, column, value, database, version, models) => new Promise(async (ok, bad) => {
            try {
                db.SetDataBaseName(database, version, models);
                // Was calling db.Select(table, column, value). Select only accepts (table), so the
                // extra arguments were dropped and this returned the WHOLE store instead of the
                // matching rows — the index lookup in SelectWhere below was never reached.
                await db.SelectWhere(table, column, value).then(function (result) {
                    ok(result);
                }).catch(function (error) {
                    bad(error);
                });
            } catch (e) {
                bad(e);
            }
        }),
        /**
         * Insert data info a table. Alway return a JSON response
         * @param {string} table table name
         * @param {JSON} data data with the models format to insert
         * @param {string} database database name
         * @param {string} version database version
         * @param {string} models models to use
         */
        Insert: (table, data, database, version, models) => new Promise(async (ok, bad) => {
            try {
                db.SetDataBaseName(database, version, models);
                await db.Insert(table, BytesToJson(data)).then(async function (result) {
                    ok(await JsonToBytes(result));
                }).catch(function (error) {
                    bad(error);
                });
            } catch (e) {
                bad(e);
            }
        }),
        /**
         * Update data info the table. The data always must be content all the columns, if not the function retrieve the actual data to keep always same data info a table. Alway return a JSON response
         * @param {string} table table name
         * @param {JSON} data data with the models format to update
         * @param {string} database database name
         * @param {string} version database version
         * @param {string} models models to use
         */
        Update: (table, data, database, version, models) => new Promise(async (ok, bad) => {
            try {
                db.SetDataBaseName(database, version, models);
                await db.Update(table, BytesToJson(data)).then(async function (result) {
                    ok(await JsonToBytes(result));
                }).catch(function (error) {
                    bad(error);
                });
            } catch (e) {
                bad(e);
            }
        }),
        /**
         * Delete one row from a table. Alway return a JSON response
         * @param {string} table table name
         * @param {any} id value from the keypath
         * @param {string} database database name
         * @param {string} version database version
         * @param {string} models models to use
         */
        Delete: (table, id, database, version, models) => new Promise(async (ok, bad) => {
            try {
                db.SetDataBaseName(database, version, models);
                await db.Delete(table, id).then(async function (result) {
                    ok(await JsonToBytes(result));
                }).catch(function (error) {
                    bad(error);
                });
            } catch (e) {
                bad(e);
            }
        }),
        /**
         * Delete all the data from a table. Alway return a JSON response
         * @param {string} table table name
         * @param {string} database database name
         * @param {string} version database version
         * @param {string} models models to use
         */
        Clean: (table, database, version, models) => new Promise(async (ok, bad) => {
            try {
                db.SetDataBaseName(database, version, models);
                await db.Clean(table).then(function (result) {
                    ok(result);
                }).catch(function (error) {
                    bad(error);
                });
            } catch (e) {
                bad(e);
            }
        }),
        /**
         * This is equal to Clean. Alway return a JSON response
         * @param {string} table table name
         * @param {string} database database name
         * @param {string} version database version
         * @param {string} models models to use
         */
        Drop: (database) => new Promise(async (ok, bad) => {
            try {
                await db.Drop(database).then(function (result) {
                    ok(result);
                }).catch(function (error) {
                    bad(error);
                });
            } catch (e) {
                bad(e);
            }
        }),
        /**
         * To store files info a db for compatibility with most of the browsers
         * @param {bytes} buffer file buffer
         * @param {string} type file type
         * @param {string} database database name
         * @param {string} version database version
         * @param {string} models models to use
         */
        HelperArrayBufferToBlob: (buffer, type, database, version, models) => new Promise((ok, bad) => {
            try {
                db.SetDataBaseName(database, version, models);
                db.HelperArrayBufferToBlob(buffer, type).then(function (result) {
                    ok(result);
                }).catch(function (error) {
                    bad(error);
                });
            } catch (e) {
                bad(e);
            }
        }),
        /**
         * Promise to get a file from a DB for compatibility with most of the browsers
         * @param {bytes} blob arraybuffer byte to retrieve
         * @param {string} database database name
         * @param {string} version database version
         * @param {string} models models to use
         */
        HelperBlobToArrayBuffer: (blob, database, version, models) => new Promise((ok, bad) => {
            try {
                db.SetDataBaseName(database, version, models);
                db.HelperBlobToArrayBuffer(blob).then(function (result) {
                    ok(result);
                }).catch(function (error) {
                    bad(error);
                });
            } catch (e) {
                bad(e);
            }
        })
    };
    return MyDb;
})();

export { Conn as MyDb }