# What changed in BlazorIndexedDb

This round started as a performance complaint and ended up uncovering a handful of things that
were quietly wrong. Writes are the headline: a batch that took four minutes now takes
milliseconds of database time. But the more interesting part is *why* it took four minutes, and
what else that same mistake was hiding.

---

## Writes were waiting on a guess

`Insert` and `Update` resolved their promise like this:

```javascript
setTimeout(() => ok(transactionResult), rows * 50);
```

Fifty milliseconds per row, hardcoded. Not a timeout guarding a real signal — the signal itself.
The write had almost certainly finished long before, and the caller sat waiting for a stopwatch
that knew nothing about the database.

The cost is exactly linear and exactly predictable, which is how it was found. A real batch of
4944 rows: predicted 4944 x 50 = 247200ms, measured **247506ms**. A batch of 94 rows: predicted
4700ms, measured **4716ms**. The database was never the slow part.

Both now resolve from `transaction.oncomplete`, which is the browser telling you the rows are
committed. It is the same signal `Clean` and `Delete` were already using.

There was a second bug hiding inside the first one. Because the timer resolved independently of
the transaction, the caller resumed while the transaction was still open and the connection still
unclosed. `Update` in fact **never closed its connection at all**, in any code path. That is
fixed too.

## The timer was also reporting failures as successes

Once you resolve on a timer, you resolve no matter what happened. So:

- A transaction that **aborted** reported success. In IndexedDB an aborted transaction rolls back
  every row in it, so this reported success for a write where nothing at all was stored.
- A database that **failed to open**, or that was **blocked** by another connection, reported
  success. Nothing handled either event.

All three now report a failure, and `onabort` was added to `Delete` and `Clean` as well, which
never had it either. Those two had no timer papering over the gap, so an aborted transaction there
simply left the promise pending forever.

This is worth being blunt about: if you have been using this library on large writes, some of them
may have been failing and telling you they succeeded.

## One bad row no longer discards the batch

`Insert` uses `store.add`, so a key that already exists is refused. That is deliberate and it is
not changing — this is an insert, not an upsert, and it should behave like a primary key in SQL.

What was wrong was the blast radius. In IndexedDB a request error that nobody handles *propagates*
and aborts the whole transaction. One duplicated key in a batch of 4944 did not refuse one row, it
threw away all 4944. And since the transaction died, no response could tell you which row was at
fault — there was nothing left to report on.

Each request now handles its own error and calls `preventDefault()`, which stops the propagation:

```javascript
const request = store.add(row);
request.onerror = ev => {
    transactionResult.push(context.SetResponse(false, `... record ${key}: ${reason}`));
    ev.preventDefault();
};
```

The refused row is the only one not written. Everything else commits. And the response now carries
one entry per refused row, naming its key and the reason.

That last part matters more than it looks: splitting a write into chunks in order to find out
which rows failed is no longer necessary. That was the only thing chunking bought you here.

## Deleting many rows took one connection per row

There was no bulk delete. Removing N rows meant calling `DeleteAsync` N times, and each call
opened its own database connection and its own transaction. The same shape of problem as the
timer, just never measured.

```csharp
await DB.PlayList.DeleteAsync(new List<int> { 1, 2, 3 });
```

One call, one connection, one transaction, N deletes inside it.

The five single-id overloads (`int`, `double`, `decimal`, `string`, `DateTime`) behave exactly as
before, but they were five copies of the same method body — about 150 duplicated lines — and now
share one code path.

## The indexed lookup existed but nothing could reach it

`SelectWhere` looks a value up through the store index, so only the matching rows ever cross into
.NET. It had two problems at once:

1. The interop wrapper called `db.Select(table, column, value)`. `Select` only takes the table
   name, so the extra arguments were silently dropped and it returned the **whole store**.
2. The C# side read the response through a path that could not deserialize what JavaScript sends —
   a `Uint8Array` arrives as an array of numbers, and `System.Text.Json` expects base64 for a
   `byte[]`.
3. And even with both fixed, **no method on `StoreSet` called it.** It was unreachable.

All three are fixed and the overload is now public:

```csharp
//resolved by the index, only matching rows are transferred
var mine = await DB.PlayList.SelectAsync(nameof(PlayList.Ownner), "me");

//the existing overload, which reads everything and filters in C#
var also = await DB.PlayList.SelectAsync(x => x.Ownner == "me");
```

Both are still useful. The predicate overload takes any expression; the indexed one is far cheaper
but needs an actual index on the column.

## Reading a record by its key never worked

`SelectId` builds a key range to look the record up. It built it from `value`:

```javascript
async SelectId(table, id) {
    ...
    const range = IDBKeyRange.only(value);   // the parameter is called id
```

There is no `value` in that scope, so every call threw a `ReferenceError` before touching the
store. The surrounding `catch` turned it into a rejection, and `SingleRecord` on the C# side
swallows rejections and returns `null`.

The result: `GetAsync(id)` always came back empty, for everyone, and never reported an error. If
you worked around it by reading the whole store and filtering in C#, you can stop.

`SingleRecord` still returns `null` when a lookup fails — not finding a record and failing to look
for one are the same answer to the caller — but the exception is written to the console when
`Settings.EnableDebug` is on, which is where this should have been visible all along.

## A read that could not open the database hung

`Select` handled a failed open like this:

```javascript
dbconnect.onerror = function (e) {
    this.close();       // "this" is the open request, which has no close()
```

The handler threw a `TypeError` of its own, and the promise was never settled. Not an error, not an
empty result — the caller simply waited forever.

None of the three read paths handled a *blocked* open either, which is the case that waits longest:
the open is queued behind another connection that has not closed. `Select`, `SelectId` and
`SelectWhere` now all report a failed or blocked open.

With this, no path in the library can leave a promise pending.

## A blocked database is not a failure

`blocked` is the event IndexedDB fires when your operation is queued behind another connection that
has not closed yet. It is not an error. The operation goes ahead on its own as soon as the other
connections close, and `onsuccess` fires normally.

`DropDatabase` treated it as a failure and rejected immediately:

```javascript
request.onblocked = function (event) {
    error(context.SetResponse(false, `Databse ${database} blocked. ${event.target.error}`));
};
```

So it reported a failure for a delete that then went ahead and completed anyway — the caller
believed the database was still there while it was in the process of disappearing. And a blocked
event carries no error, so `event.target.error` was `null` and the rejection had no message to show.

Every blocked handler now warns to the console and leaves `onsuccess` and `onerror` as the events
that settle the promise.

This is also much less likely to be reached than it was: a blocked drop means connections are still
open, and until this release `Update` never closed its connection and `Select` leaked one whenever
an open failed.

## Everything crosses to JavaScript as bytes now

`Insert`, `Update` and `Delete` exchanged JSON as **strings**: .NET built the entire payload into
a string, marshalled it, and JavaScript parsed it back — then the response made the same trip in
reverse, materialized as a `JsonElement` and serialized *again*.

They now exchange UTF-8 encoded bytes in both directions, which is what the read side had always
done. A `byte[]` crosses as a `Uint8Array` with no string in between, and responses deserialize
straight from the bytes.

While doing this, the `JsonSerializerOptions` turned out to be rebuilt on every call, in two
separate copies that had already drifted apart from each other. They are shared now, which also
restores the serializer's metadata cache.

None of this is visible from your code.

## Handling failures

Writes report failures in the response rather than throwing. That is on purpose: an exception
raised on the JavaScript side reaches .NET as a message and nothing else, so throwing would
destroy the per-row detail described above.

If you would rather have Entity Framework's behaviour, ask for it:

```csharp
CommandResponse response = await DB.PlayList.AddAsync(items);

//only the refused rows
foreach (var error in response.Errors)
    Console.WriteLine(error.Message);

//or throw, the way SaveChanges does
(await DB.PlayList.AddAsync(items)).EnsureSuccess();
```

`EnsureSuccess()` throws a `ResponseException` carrying every refused row's message, and returns
the response unchanged when everything went in, so it chains. `Result` still goes false whenever
any row was refused, so code that only checks the flag needs no change.

---

## Upgrading

No API was removed and no signature changed, so nothing should stop compiling. Two behaviours do
change, and both are corrections:

- **Writes that were silently failing now report failure.** If a large write starts returning
  `Result == false` after upgrading, it was most likely failing before too — you just were not
  told. `Errors` will say which rows and why.
- **`SelectWhere` returns the matching rows instead of the whole store.** If you were filtering
  its result yourself as a workaround, that workaround is now redundant.
- **`GetAsync(id)` returns records where it used to return `null`.** Code written against the old
  behaviour — treating that `null` as "not found" and falling back to something else — will now
  take the other branch.
