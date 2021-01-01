class jsDB{
    #DB_NAME = 'MyDB';      //db name
    #DB_VERSION = 1;        //db version
    #MODELS;                //Table model definitions
    constructor (model){        
        if (!this.#OpenDB()) {
            throw "IndexedDB not compatible!"
        }
        else {
            if (model.name){    
                this.#DB_NAME = model.name;
                this.#DB_VERSION = model.version;
                this.#MODELS  = model.tables;            
                let dbconnect = this.#OpenDB().open(this.#DB_NAME, this.#DB_VERSION);
                dbconnect.onblocked = ev => {       
                    // If some other tab is loaded with the database, then it needs to be closed
                    // before we can proceed.
                    alert("Please close all other tabs with this site open!");
                };
                dbconnect.onupgradeneeded = ev => {
                    //create database with the model send
                    let db = ev.target.result;
                    let tables = model.tables;
                    let t = tables.length;
                    //get tables
                    if (t > 0){
                        for (let ti = 0; ti < t; ti++) {
                            const tabla = tables[ti];
                            let columns = tabla.columns;
                            let c = columns.length;
                            //get columns
                            if (c > 0){
                                let options = tabla.options;
                                let keyPath;
                                let increment;
                                if (options){
                                    if (options.keyPath){
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
                                let objectStore = db.createObjectStore(tabla.name, { keyPath: keyPath, autoIncrement : increment });
                                for (let ci = 0; ci < c; ci++) {
                                    //add columns
                                    const column = columns[ci];
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
                    console.info('DB ' + model.name + ' open DONE');
                }
            }
            else throw "Please provide db model {name: 'MyDB', version: 1, tables: [{name: 'Table1', options: {keyPath : 'Id', autoIncrement: true/false}, columns: [{name: 'ColumnName', keyPath: true/false, autoIncrement: true/false, unique: true/false }]}]}"
        }
    }
    /**
     * Open a indexedDb with all the compatibilities from the browsers
     */
    #OpenDB(){
        try {
            let dbconnect = window.indexedDB || window.webkitIndexedDB || window.mozIndexedDB || window.OIndexedDB || window.msIndexedDB, IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.OIDBTransaction || window.msIDBTransaction;
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
        return {result: r, message: m};
    }  
    /**
     * Return true or false if the key exist into the object send
     * @param {object} obj 
     * @param {string} val 
     */
    #HasKeyPath(obj, val) {
        let found = false;
        for (const key in obj) {
            if (Object.hasOwnProperty.call(obj, key)) {
                const element = obj[key];
                for (var i in element) {   
                    if (i == val) found = true;
                }
            }
        }    
        return found;
    } 
    /**
     * Merge all properties from 2 objects. Can't have object inside object.
     * @param {object} obj1 
     * @param {object} obj2 
     */
    #MergeObjects(obj1, obj2) {
        var obj3 = {};
        var obj4 = {};
        //set the 2 objects exactly with the same properties
        for(let k1 in obj1) {
            if (Object.hasOwnProperty.call(obj1, k1)) {
                const e1 = obj1[k1];
                for(let k2 in obj2) { 
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
     * Check if the data send match with the table definition
     * @param {object} model 
     * @param {JSON} d data to check with the model
     * @param {bool} keypath forse to have a keypath on the default json objet
     * @param {function} callBack function to receive the error directly
     * @param {function} method function sender
     */
    #CheckModel(model, d, keypath, callBack, method){  
        let context = this;      
        if (model){
            let canContinue;
            let defaultObj = '{';       //to create a default json for the table
            //compare object receive with table definition
            //check primary key
            if (model.options.keyPath && model.options.autoIncrement && model.options.autoIncrement == false){
                //have a primary key required
                if (context.#HasKeyPath(d, model.options.keyPath)) canContinue = true;
                else{
                    if (keypath) defaultObj += '"' + model.options.keyPath + '": null,';
                    canContinue = false;                
                    if (callBack) callBack(context.#SetResponse(false, model.options.keyPath + " is required and can't be found."));
                } 
            }    
            else {
                if (keypath && model.options.keyPath) defaultObj += '"' + model.options.keyPath + '": null,';
                canContinue = true;
            }
            //sure the object received have all the keys to send               
            if (canContinue){
                let data = [];
                //create a default json object for the table
                let c = model.columns.length;
                for (let index = 0; index < c; index++) {
                    defaultObj += '"' +model.columns[index].name + '": null,';
                }
                defaultObj = defaultObj.substring(0,defaultObj.length-1);
                defaultObj += '}';
                let obj = JSON.parse(defaultObj);
                for (const key in d) {
                    if (Object.hasOwnProperty.call(d, key)) {
                        const element = d[key];                        
                        //recovery the original object to fill all the data must be updated
                        if (element[model.options.keyPath]){
                            context.SelectId(model.name, element[model.options.keyPath], function (result) {
                                if (result){
                                    let original = context.#MergeObjects(obj, result);
                                    let send = context.#MergeObjects(original, element);
                                    data.push(send);
                                }
                                else {
                                    data.push(context.#MergeObjects(obj, element));
                                }
                            });
                        }
                        else {
                            data.push(context.#MergeObjects(obj, element));
                        }
                    }
                }
                //wait a bit to ensure the transaction is completed
                setTimeout(() => {
                    method(data);   
                }, 150 * d.length);
            }
            else method(null);
        }
        else {
            if (callBack) callBack(context.#SetResponse(false, "Model can't be null."));
            throw "Model can't be null.";
        }
    }
    Connected(){
        return 'Connected to ' + this.#DB_NAME + ' with version ' + this.#DB_VERSION;
    }
    /**
     * Request all data from a table name. Return the data or JSON response
     * @param {string} table table name to request the data
     * @param {function} callBack function to receive the result.
     */   
    Select(table, callBack) {
        let context = this;
        let dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
        dbconnect.onsuccess = function() {
            let db = this.result;
            try {
                let transaction = db.transaction(table, 'readonly');
                let store = transaction.objectStore(table);  
                let request = store.getAll();
                request.onerror = ev => {
                    db.close();
                    if (callBack) callBack(context.#SetResponse(false, ev.target.error.message));
                    else throw ev.target.error.message; 
                };                          
                request.onsuccess = () => {
                    db.close();
                    if (callBack) callBack(request.result);
                    else throw "Not callback action send";
                };
            } catch (error) {
                db.close();
                if (callBack) callBack(context.#SetResponse(false, error.message));   
                else throw error.message;                  
            }
        }
    }
    /**
     * Request data from a table name with the keyPath with a value. Return the data or JSON response
     * @param {string} table table name
     * @param {any} id id value
     * @param {function} callBack function to receive the data
     */
    SelectId(table, id, callBack) {
        let context = this;
        let dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
        dbconnect.onsuccess = function() {
            let db = this.result;
            try {
                let transaction = db.transaction(table, 'readonly');
                let store = transaction.objectStore(table);  
                let request = store.get(id);
                request.onerror = ev => {
                    db.close();
                    if (callBack) callBack(context.#SetResponse(false, ev.target.error.message));
                    else throw ev.target.error.message; 
                };
                request.onsuccess = function() {
                    db.close();
                    if (callBack) callBack(this.result);
                    else throw "Not callback action send";                    
                }                
            } catch (error) {
                db.close();
                if (callBack) callBack(context.#SetResponse(false, error.message));   
                else throw error.message;                   
            }
        }
    }   
    /**
     * Request data from a table name filtered by column name and value. Return the data or JSON response
     * @param {string} table table name
     * @param {string} column column name to filter
     * @param {any} value value for the filter
     * @param {function} callBack function to receive the data
     */
    SelectWhere(table, column, value, callBack) {
        let context = this;
        let dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
        dbconnect.onsuccess = function() {
            let db = this.result;
            try {
                let transaction = db.transaction(table, 'readonly');
                let store = transaction.objectStore(table);
                let index  = store.index(column);
                let request = index.get(value);
                request.onerror = ev => {
                    db.close();
                    if (callBack) callBack(context.#SetResponse(false, ev.target.error.message));
                    else throw ev.target.error.message; 
                };
                request.onsuccess = function() {
                    db.close();
                    if (callBack) callBack(this.result);
                    else throw "Not callback action send";
                }                    
            } catch (error) {
                db.close(); 
                if (callBack) callBack(context.#SetResponse(false, error.message));   
                else throw error.message;                
            }
        }
    }
    /**
     * Insert data into a table. Alway return a JSON response
     * @param {string} table table name
     * @param {JSON} data data with the model format to insert
     * @param {function} callBack function to receive result
     */
    Insert(table, data, callBack){
        let context = this;
        context.#CheckModel(context.#MODELS.find(el=> el.name = table), data, false, callBack, function (obj) {
            if (obj){
                let dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
                dbconnect.onsuccess = function() {
                    let db = this.result;
                    try {
                        let transaction = db.transaction(table, 'readwrite');
                        let store = transaction.objectStore(table);                
                        obj.forEach(el => store.add(el));
                        transaction.onerror = ev => {                    
                            db.close();
                            if (callBack) callBack(context.#SetResponse(false, ev.target.error.message));
                            else throw ev.target.error.message;           
                        };
                        transaction.oncomplete = () => {                    
                            db.close();
                            if (callBack) callBack(context.#SetResponse(true, 'Insert done!'));
                        };                
                    } catch (error) {
                        db.close();
                        if (callBack) callBack(context.#SetResponse(false, error.message));   
                        else throw error.message;                
                    }
                };   
            }
            else {
                if (callBack) callBack(context.#SetResponse(false, "Model can't be null."));
                else throw "Model can't be null."
            }
        });
    } 
    /**
     * Update data into the table. The data always must be content all the columns, if not the function retreive the actual data to keep always same data into a table. Alway return a JSON response
     * @param {string} table table name
     * @param {JSON} data data with the model format to update
     * @param {dunction} callBack function to receive the result
     */
    Update(table, data, callBack){
        let context = this;
        context.#CheckModel(context.#MODELS.find(el=> el.name = table), data, true, callBack, function (obj) {
            if (obj){
                let dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
                dbconnect.onsuccess = function() {
                    let db = this.result;
                    try {
                        let transaction = db.transaction(table, 'readwrite');
                        let store = transaction.objectStore(table); 
                        obj.forEach(el => store.put(el));
                        transaction.onerror = ev => {         
                            db.close();
                            if (callBack) callBack(context.#SetResponse(false, ev.target.error.message));
                            else throw ev.target.error.message;           
                        };
                        transaction.oncomplete = () => {         
                            db.close();
                            if (callBack) callBack(context.#SetResponse(true,'Update done!'));
                        };         
                    } catch (error) { 
                        db.close();
                        if (callBack) callBack(context.#SetResponse(false, error.message));   
                        else throw error.message;          
                    }
                };  
            }
            else {
                if (callBack) callBack(context.#SetResponse(false, "Model can't be null."));
                else throw "Model can't be null."
            }
        });
    }
    /**
     * Dete one row from a table. Alway return a JSON response
     * @param {string} table table name
     * @param {any} id value from the keypath
     * @param {function} callBack Function to receive the result
     */
    Delete(table, id, callBack){
        let context = this;
        let dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
        dbconnect.onsuccess = function() {
            let db = this.result;
            try {
                let transaction = db.transaction(table, 'readwrite');
                let store = transaction.objectStore(table);
                store.delete(id);
                transaction.onerror = ev => {
                    db.close();
                    if (callBack) callBack({result: false, message: ev.target.error.message});
                    else throw ev.target.error.message;           
                };
                transaction.onsuccess  = () => {
                    db.close();
                    if (callBack) callBack({result: true, message: 'Delete done!'});
                };                
            } catch (error) {
                db.close();
                if (callBack) callBack(context.#SetResponse(false, error.message));   
                else throw error.message;                        
            }
        };        
    }
    /**
     * Drop a table. Alway return a JSON response
     * @param {string} table table name
     * @param {function} callBack function to receive the result
     */
    Drop(table, callBack){
        let context = this;
        let dbconnect = context.#OpenDB().open(context.#DB_NAME, context.#DB_VERSION);
        dbconnect.onsuccess = function() {
            let db = this.result;
            try {
                let transaction = db.transaction(table, 'readwrite');
                let store = transaction.objectStore(table);
                let req = store.clear();
                req.onerror = ev => {
                    db.close();
                    if (callBack) callBack({result: false, message: ev.target.error.message});
                    else throw ev.target.error.message;           
                };
                req.onsuccess  = () => {
                    db.close();
                    if (callBack) callBack({result: true, message: 'Drop done!'});
                };
            } catch (error) {
                db.close();
                if (callBack) callBack(context.#SetResponse(false, error.message));   
                else throw error.message;          
            }
        };        
    }
    /**
     * To store files into a db for compatibility with most of the browsers
     * @param {bytes} buffer file buffer
     * @param {string} type file type
     */
    HelperArrayBufferToBlob(buffer, type) {
        return new Blob([buffer], {type: type});
    }
    /**
     * Promise to get a file from a da from a DB for compatibility with most of the browsers
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
