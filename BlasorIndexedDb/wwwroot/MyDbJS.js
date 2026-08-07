class jsDB {
    DB_NAME = 'MyDB';      //db name
    DB_VERSION = 1;        //db version
    MODELS;                //Table model definitions

    constructor() {
        console.info('Database jsDB instance created!');
    }

    /**
     * Bring the database in line with the model, keeping everything that can be kept. A store the
     * model no longer has is dropped, a new one is created, and one whose key changed is rebuilt with
     * its rows carried across. Nothing is deleted wholesale and no version has to be bumped by hand
     * for a store to appear. See Migrate.
     * @param {any} dbModel {name, version, tables} built by the C# side from the context
     */
    Initialize(dbModel) {
        return new Promise((resolve, reject) => {
            if (!this.OpenDB()) {
                reject([this.SetResponse(false, `${this.DB_NAME} => IndexedDB not compatible!`)]);
                return;
            }
            if (!dbModel.name) {
                reject([this.SetResponse(false, `${this.DB_NAME} => Error: Please provide db model {name: 'MyDB', version: 1, tables: [{name: 'Table1', options: {keyPath : 'Id', autoIncrement: true/false}, columns: [{name: 'ColumnName', keyPath: true/false, autoIncrement: true/false, unique: true/false }]}]}`)]);
                return;
            }

            this.DB_NAME = dbModel.name;
            this.DB_VERSION = dbModel.version;
            this.MODELS = dbModel.tables;

            // What the browser holds is read before any version is asked for. The database can be
            // ahead of the model: a model that changes without its version being raised still needs
            // a version change to be applied, and it takes the next one, so from then on the number
            // in the browser is the higher one. Opening that with a lower version is a VersionError,
            // which is why nothing here or in the reads and writes asks for a version it has not
            // checked first.
            const inspect = this.OpenDB().open(this.DB_NAME);
            // Deliberately empty. An open with no version creates the database at version 1 when it
            // is not there, and letting that happen is the point: what comes back is then an empty
            // database whose every store is missing, which is the same case as any other mismatch
            // and is handled below by the upgrade. Aborting here instead would leave the whole
            // first run of a device depending on an aborted version change reporting itself as a
            // failed open, and there is no reason to need that.
            inspect.onupgradeneeded = () => { };
            inspect.onblocked = () => console.warn(`${this.DB_NAME} => open blocked, waiting for the other open connections to close`);
            inspect.onerror = ev => {
                const reason = ev.target.error ? ev.target.error.message : 'cannot open database';
                console.warn(`${this.DB_NAME} => Connection Error: ${reason}`);
                reject([this.SetResponse(false, `${this.DB_NAME} => Connection Error: ${reason}`)]);
            };
            inspect.onsuccess = ev => {
                const db = ev.target.result;
                const current = db.version;
                const changes = this.PendingChanges(db);
                db.close();

                if (changes.length === 0) {
                    resolve();
                    return;
                }

                console.info(`${this.DB_NAME} => applying schema changes: ${changes.join(', ')}`);
                this.Upgrade(this.DB_VERSION > current ? this.DB_VERSION : current + 1)
                    .then(resolve)
                    .catch(reject);
            };
        });
    }

    /**
     * Open at a version that forces a version change, and migrate inside it.
     * @param {number} version version to open with
     */
    Upgrade(version) {
        return new Promise((resolve, reject) => {
            // Checked before opening, so a model that did not make it across is reported as itself
            // rather than as a transaction that aborted for no stated reason.
            if (this.Tables().length === 0) {
                reject([this.SetResponse(false, `${this.DB_NAME} => OnUpgrade Error: No tables defined.`)]);
                return;
            }

            // Filled while the version change runs and read once it has committed, so what is
            // reported is what was actually lost and not what was about to be attempted.
            const losses = [];
            const request = this.OpenDB().open(this.DB_NAME, version);
            request.onblocked = () => console.warn(`${this.DB_NAME} => upgrade blocked, waiting for the other open connections to close`);
            request.onupgradeneeded = ev => this.Migrate(ev.target.result, ev.target.transaction, losses);
            request.onerror = ev => {
                const reason = ev.target.error ? ev.target.error.message : 'cannot open database';
                console.warn(`${this.DB_NAME} => Upgrade Error: ${reason}`);
                reject([this.SetResponse(false, `${this.DB_NAME} => Upgrade Error: ${reason}`)]);
            };
            request.onsuccess = ev => {
                ev.target.result.close();
                this.ReportLosses(losses);
                resolve();
            };
        });
    }

    /**
     * The whole migration, inside the version change transaction it is given. Everything it does is
     * one transaction: rows are read from the old store and written to the new one without leaving
     * it, so there is nothing to copy anywhere else first, and a failure anywhere aborts the lot and
     * leaves the database exactly as it was rather than half migrated.
     * @param {IDBDatabase} db database being upgraded
     * @param {IDBTransaction} transaction the version change transaction
     * @param {Array} losses collects what could not be kept
     */
    Migrate(db, transaction, losses) {
        const tables = this.Tables();

        if (tables.length === 0) {
            // Rejecting is not enough on its own: a version change commits even when the promise is
            // rejected, and what it would leave behind is a database at this version with no stores,
            // which every later open then takes as good.
            transaction.abort();
            return;
        }

        for (const name of [...db.objectStoreNames]) {
            if (tables.some(table => table.name === name)) continue;
            this.DropStore(db, transaction, name, losses);
        }

        for (const table of tables) {
            if (!db.objectStoreNames.contains(table.name)) {
                this.CreateStore(db, table);
                continue;
            }

            const store = transaction.objectStore(table.name);
            // keyPath and autoIncrement are fixed when the store is created, so those are the only
            // changes that cannot be made in place. Indexes can, and the rows never move for them.
            if (this.SameKey(store, table)) this.SyncIndexes(store, table);
            else this.RebuildStore(db, transaction, table, losses);
        }
    }

    /**
     * A store the model no longer has. Its rows are counted before it goes, because that count is
     * the only thing anyone can be told about them afterwards.
     */
    DropStore(db, transaction, name, losses) {
        const loss = { store: name, dropped: 0 };
        losses.push(loss);

        const count = transaction.objectStore(name).count();
        count.onsuccess = () => {
            loss.dropped = count.result;
            db.deleteObjectStore(name);
        };
    }

    /**
     * Create a store from a table of the model, with its indexes.
     */
    CreateStore(db, table) {
        const store = db.createObjectStore(table.name, this.StoreOptions(table));
        for (const column of table.columns) {
            store.createIndex(column.name, column.name, { unique: column.unique === true });
        }
        return store;
    }

    /**
     * Add what the model gained, remove what it dropped, and remake the ones whose uniqueness
     * changed. An index is built from the rows that are already there, so none of this touches them.
     */
    SyncIndexes(store, table) {
        const wanted = table.columns.map(column => column.name);

        for (const name of [...store.indexNames]) {
            if (!wanted.includes(name)) store.deleteIndex(name);
        }

        for (const column of table.columns) {
            const unique = column.unique === true;

            if (store.indexNames.contains(column.name)) {
                if (store.index(column.name).unique === unique) continue;
                store.deleteIndex(column.name);
            }

            store.createIndex(column.name, column.name, { unique: unique });
        }
    }

    /**
     * The store's key changed, so it has to be made again: the rows are read, the store is replaced,
     * and they are written back one at a time. A row the new key cannot be taken from is the only
     * one that cannot come across — it is counted and reported, and preventDefault keeps its failure
     * from aborting the transaction and taking every other row with it.
     */
    RebuildStore(db, transaction, table, losses) {
        const previous = transaction.objectStore(table.name);
        const options = this.StoreOptions(table);
        const loss = {
            store: table.name,
            from: this.DescribeKey(previous.keyPath, previous.autoIncrement),
            to: this.DescribeKey(options.keyPath, options.autoIncrement),
            total: 0,
            failures: []
        };
        losses.push(loss);

        // The keys are read as well as the rows, so a row that cannot be written back can be named
        // by the key it used to have instead of just counted.
        const keys = previous.getAllKeys();
        keys.onsuccess = () => {
            const read = transaction.objectStore(table.name).getAll();
            read.onsuccess = () => {
                const rows = read.result || [];
                loss.total = rows.length;

                db.deleteObjectStore(table.name);
                const store = this.CreateStore(db, table);

                for (let r = 0; r < rows.length; r++) {
                    this.Carry(store, rows[r], keys.result[r], loss);
                }
            };
        };
    }

    /**
     * Write one row into the rebuilt store. A key the row cannot produce throws from add() itself,
     * a duplicate or an index that refuses it comes back on the request, and both end up in the
     * report rather than in an exception nobody sees.
     */
    Carry(store, row, key, loss) {
        try {
            const request = store.add(row);
            request.onerror = ev => {
                loss.failures.push(`key ${JSON.stringify(key)}: ${ev.target.error ? ev.target.error.message : 'refused by the store'}`);
                ev.preventDefault();
            };
        } catch (e) {
            loss.failures.push(`key ${JSON.stringify(key)}: ${e.message}`);
        }
    }

    /**
     * Only what was lost, and nothing at all when nothing was.
     */
    ReportLosses(losses) {
        for (const loss of losses) {
            if (loss.dropped !== undefined) {
                if (loss.dropped > 0) {
                    console.warn(`${this.DB_NAME} => ${loss.store}: ${loss.dropped} row(s) lost, the store is no longer in the model`);
                }
                continue;
            }

            if (loss.failures.length === 0) continue;

            console.warn(`${this.DB_NAME} => ${loss.store}: ${loss.failures.length} of ${loss.total} row(s) lost changing the key from ${loss.from} to ${loss.to}`);
            for (const failure of loss.failures) console.warn(`${this.DB_NAME} => ${loss.store}: ${failure}`);
        }
    }

    /**
     * The tables of the model that are actually created. One with no columns is skipped by the
     * creation, so it is not one of them anywhere else either.
     */
    Tables() {
        if (!this.MODELS || this.MODELS.length === 0) return [];
        return this.MODELS.filter(table => table.columns && table.columns.length > 0);
    }

    /**
     * The store options a table asks for, with the same defaults the creation has always used: a
     * model that names no keyPath gets the generated ssnId.
     */
    StoreOptions(table) {
        const options = table.options;
        if (options && options.keyPath) {
            return { keyPath: options.keyPath, autoIncrement: options.autoIncrement === true };
        }
        return { keyPath: 'ssnId', autoIncrement: true };
    }

    /**
     * Whether the store in the browser is keyed the way the model asks for. String() so a compound
     * keyPath, which is an array, compares by what it contains.
     */
    SameKey(store, table) {
        const options = this.StoreOptions(table);
        return String(store.keyPath) === String(options.keyPath)
            && store.autoIncrement === options.autoIncrement;
    }

    /**
     * How a key is named in the report.
     */
    DescribeKey(keyPath, autoIncrement) {
        return `'${keyPath}'${autoIncrement ? ' (auto)' : ''}`;
    }

    /**
     * What the model asks for that the database does not have yet, readable, for the console and to
     * decide whether a version change is needed at all.
     * @param {IDBDatabase} db database as it is now
     */
    PendingChanges(db) {
        const changes = [];
        const tables = this.Tables();
        const existing = [...db.objectStoreNames];

        for (const name of existing) {
            if (!tables.some(table => table.name === name)) changes.push(`drop ${name}`);
        }

        const known = tables.filter(table => existing.includes(table.name)).map(table => table.name);
        // Reading keyPath, autoIncrement and the index names needs a store, and a store needs a
        // transaction, even when nothing is going to be read from it.
        const transaction = known.length > 0 ? db.transaction(known, 'readonly') : null;

        for (const table of tables) {
            if (!existing.includes(table.name)) {
                changes.push(`create ${table.name}`);
                continue;
            }

            const store = transaction.objectStore(table.name);

            if (!this.SameKey(store, table)) {
                const options = this.StoreOptions(table);
                changes.push(`rebuild ${table.name} ${this.DescribeKey(store.keyPath, store.autoIncrement)}`
                    + ` -> ${this.DescribeKey(options.keyPath, options.autoIncrement)}`);
                continue;
            }

            const indexes = this.IndexChanges(store, table);
            if (indexes.length > 0) changes.push(`reindex ${table.name} [${indexes.join(' ')}]`);
        }

        return changes;
    }

    /**
     * The indexes of a store that do not match the columns of its table.
     */
    IndexChanges(store, table) {
        const changes = [];
        const wanted = table.columns.map(column => column.name);

        for (const name of [...store.indexNames]) {
            if (!wanted.includes(name)) changes.push(`-${name}`);
        }

        for (const column of table.columns) {
            if (!store.indexNames.contains(column.name)) {
                changes.push(`+${column.name}`);
                continue;
            }
            if (store.index(column.name).unique !== (column.unique === true)) changes.push(`~${column.name}`);
        }

        return changes;
    }
    /**
     * A read or a write must never be what creates the database. It has no model to make the stores
     * from, so what it would leave behind is an empty database at the current version, which
     * Initialize used to accept as good for the rest of the application's life. Aborting the version
     * change keeps the database absent and says so.
     * @param {IDBOpenDBRequest} dbconnect open request to guard
     * @param {function} reject called with the reason
     * @param {string} label operation and store, for the message
     */
    GuardCreation(dbconnect, reject, label) {
        dbconnect.onupgradeneeded = ev => {
            ev.target.transaction.abort();
            const message = `${label}: the database ${this.DB_NAME} is not initialized, there are no stores to work with`;
            console.warn(message);
            reject([this.SetResponse(false, message)]);
        };
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
                    const dbconnect = context.OpenDB().open(context.DB_NAME);
                    context.GuardCreation(dbconnect, error, `Select => ${table}`);
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
                    // This used to call this.close() on the open REQUEST, which has no close(), so
                    // it threw a TypeError and the promise was never settled at all: a failed open
                    // hung the caller forever. Rejecting reports the reason instead. onblocked was
                    // missing too, and it is the case that hangs the longest — the open is waiting
                    // for another connection to close.
                    dbconnect.onerror = ev => {
                        console.warn(`${table}: ${ev.target.error ? ev.target.error.message : 'cannot open database'}`);
                        error([context.SetResponse(false, `${table}: ${ev.target.error ? ev.target.error.message : 'cannot open database'}`)]);
                    };
                    // Warn only. blocked means the open is queued behind another connection that has
                    // not closed yet, not that it failed; onsuccess still fires once it does. It
                    // only happens at all when the open needs a version upgrade.
                    dbconnect.onblocked = () => console.warn(`${table}: database blocked, waiting for the other open connections to close`);
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
                const dbconnect = context.OpenDB().open(context.DB_NAME);
                context.GuardCreation(dbconnect, error, `SelectId => ${table}.${id}`);
                dbconnect.onsuccess = function () {
                    const db = this.result;
                    try {
                        const transaction = db.transaction(table, 'readonly');
                        const store = transaction.objectStore(table);
                        // Was IDBKeyRange.only(value). There is no "value" here, the parameter is
                        // "id", so every call threw a ReferenceError before touching the store. The
                        // catch below turned it into a rejection and SingleRecord swallows those and
                        // returns null, which is why looking a record up by its key always came back
                        // empty and never reported anything.
                        const range = IDBKeyRange.only(id);
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
                dbconnect.onerror = ev => error([context.SetResponse(false, `${table}.${id}: ${ev.target.error ? ev.target.error.message : 'cannot open database'}`)]);
                dbconnect.onblocked = () => console.warn(`${table}.${id}: database blocked, waiting for the other open connections to close`);
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
                const dbconnect = context.OpenDB().open(context.DB_NAME);
                context.GuardCreation(dbconnect, error, `SelectWhere => ${table}.${column}`);
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
                // Same gap as Select and SelectId had: nothing handled a failed or blocked open, so
                // the promise stayed pending.
                dbconnect.onerror = ev => error([context.SetResponse(false, `${table}.${column}.${value}: ${ev.target.error ? ev.target.error.message : 'cannot open database'}`)]);
                dbconnect.onblocked = () => console.warn(`${table}.${column}.${value}: database blocked, waiting for the other open connections to close`);
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

                const dbconnect = context.OpenDB().open(context.DB_NAME);
                context.GuardCreation(dbconnect, reason => { transactionResult.push(reason[0]); finish(); }, `Insert => ${table}`);
                // Opening can fail or be blocked. Nothing used to handle that, which was survivable
                // only because the old timeout resolved regardless; now it would hang forever.
                dbconnect.onerror = ev => {
                    transactionResult.push(context.SetResponse(false, `Insert => ${table}: ${ev.target.error ? ev.target.error.message : 'cannot open database'}`));
                    finish();
                };
                // Warn only: blocked means the open is queued behind another connection that has not
                // closed, not that it failed. onsuccess still fires once it does.
                dbconnect.onblocked = () => console.warn(`Insert => ${table}: database blocked, waiting for the other open connections to close`);
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
                const dbconnect = context.OpenDB().open(context.DB_NAME);
                context.GuardCreation(dbconnect, reason => { transactionResult.push(reason[0]); finish(); }, `Update => ${table}`);
                dbconnect.onerror = ev => {
                    transactionResult.push(context.SetResponse(false, `Update => ${table}: ${ev.target.error ? ev.target.error.message : 'cannot open database'}`));
                    finish();
                };
                dbconnect.onblocked = () => console.warn(`Update => ${table}: database blocked, waiting for the other open connections to close`);
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
                                            try {
                                                // A row that is not there yet has nothing to merge into, and
                                                // UpdateModel walks the keys of what it was given: from
                                                // undefined it returned {}, and put({}) throws DataError
                                                // because the key path yields no value. Thrown from inside
                                                // this handler it also escaped the try below, aborting the
                                                // whole transaction — so updating a row somebody else had
                                                // just deleted took every other row in the batch with it.
                                                // put is an upsert, so the row itself is what goes in.
                                                dat = request.result === undefined
                                                    ? obj
                                                    : context.UpdateModel(request.result, obj);
                                                objRequest = store.put(dat);
                                                objRequest.onsuccess = () => {
                                                    transactionResult.push(context.SetResponse(true, `Success in updating record ${ssnId}`));
                                                };
                                                objRequest.onerror = ev => {
                                                    transactionResult.push(context.SetResponse(false, `Error in updating record ${ssnId}`));
                                                    console.warn(ev.target.error.message);
                                                    ev.preventDefault();
                                                };
                                            } catch (e) {
                                                transactionResult.push(context.SetResponse(false, `Update => ${table}: record ${ssnId}: ${e.message}`));
                                                console.warn(`Update => ${table}: record ${ssnId}: ${e.message}`);
                                            }
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
            const dbconnect = context.OpenDB().open(context.DB_NAME);
            context.GuardCreation(dbconnect, reason => finish(reason), `Delete => ${table}`);
            dbconnect.onerror = ev => finish([context.SetResponse(false, `Delete => ${table}: ${ev.target.error ? ev.target.error.message : 'cannot open database'}`)]);
            dbconnect.onblocked = () => console.warn(`Delete => ${table}: database blocked, waiting for the other open connections to close`);
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
                const dbconnect = context.OpenDB().open(context.DB_NAME);
                context.GuardCreation(dbconnect, error, `Clean => ${table}`);
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

                // blocked is not a failure, it is "waiting": other connections to this database are
                // still open, and deleteDatabase proceeds on its own as soon as they close. This
                // used to reject here, which reported a failure for something that then went ahead
                // and happened anyway -- and event.target.error is null on blocked, so the rejection
                // carried no message either. onsuccess and onerror remain the signals that settle.
                request.onblocked = function () {
                    console.warn(`Drop => ${database}: blocked, waiting for the other open connections to close`);
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
