# DB Engineering Review — Lothal.Basket.Service

> **Reviewer perspective:** Database engineer reviewing all data-layer code.
> Data is the most valuable asset — find every way it could be corrupted, lost, or leaked.

---

## 1. SCHEMA & DATA MODELING

### ✅ GOOD: Monetary values stored as DECIMAL
**Location:** `AppDbContext.cs:32`, `Migration:50`
`UnitPrice` is explicitly mapped to `decimal(18,2)` / `numeric(18,2)`. Floating-point arithmetic errors avoided.

---

### ✅ GOOD: Timestamps with timezone
**Location:** `Migration:33-34`, `StockDocument.cs:22`, `ServiceCollectionExtensions.cs DDL:50`
`OccurredOn`, `ProcessedOn` → `timestamp with time zone`. Stock DDL uses `TIMESTAMPTZ`. Timezone-aware throughout.

---

### ✅ GOOD: Cascade delete configured
**Location:** `AppDbContext.cs:26`
`BasketItems` cascade-deletes when parent `Basket` is deleted. Migration confirms the FK constraint.

---

### ⚠️ FINDING — DB-1: `BasketStatus` enum NOT constrained at database level
**Location:** `Basket.cs:8`, `BasketStatus.cs`, `Migration` (no constraint)
**Risk:** Application validates `BasketStatus` (Active=0, Completed=1) in C# only. Database accepts any integer. A bug, direct SQL write, or future migration that adds enum values without a corresponding DB constraint can insert invalid status codes silently.

```csharp
// Current — no DB level enforcement
public BasketStatus Status { get; set; } = BasketStatus.Active;
```

**Fix — add a CHECK constraint in migration:**
```sql
ALTER TABLE "Baskets"
  ADD CONSTRAINT chk_basket_status CHECK ("Status" IN (0, 1));
```

**Or via EF in `OnModelCreating`:**
```csharp
modelBuilder.Entity<Domain.Entities.Basket>(entity =>
{
    entity.Property(e => e.Status)
          .HasConversion<int>();   // already implicit
    entity.ToTable(t => t.HasCheckConstraint(
        "CK_Baskets_Status", "\"Status\" IN (0, 1)"));
});
```

---

### ⚠️ FINDING — DB-2: `Baskets` table has no index on `CustomerId`
**Location:** `AppDbContext.cs`, `Migration`
**Risk:** Any query fetching baskets *by customer* (e.g., "show my cart", abandoned basket cleanup) will do a sequential scan on `Baskets`. As the table grows this becomes a slow query.

**Fix — add index in migration:**
```csharp
migrationBuilder.CreateIndex(
    name: "IX_Baskets_CustomerId",
    table: "Baskets",
    column: "CustomerId");
```

---

### ⚠️ FINDING — DB-3: `OutboxMessages` has no index on `ProcessedOn` (or unprocessed filter)
**Location:** `Migration:27-40`
**Risk:** The Outbox pattern requires polling for *unprocessed* messages (`WHERE ProcessedOn IS NULL`). Without an index on `ProcessedOn`, every poll is a full table scan. Under high event rates, this table grows unbounded and polling becomes progressively slower.

**Fix — partial index (PostgreSQL supports this):**
```sql
CREATE INDEX CONCURRENTLY IX_OutboxMessages_Unprocessed
    ON "OutboxMessages" ("OccurredOn")
    WHERE "ProcessedOn" IS NULL;
```

**Or via migration:**
```csharp
migrationBuilder.CreateIndex(
    name: "IX_OutboxMessages_Unprocessed",
    table: "OutboxMessages",
    column: "OccurredOn",
    filter: "\"ProcessedOn\" IS NULL");
```

---

### ⚠️ FINDING — DB-4: `Basket.CustomerId` stored as unbounded `TEXT` with no unique constraint
**Location:** `Basket.cs:6`, `Migration:19`
**Risk:** There is no unique constraint preventing a customer from having multiple active baskets simultaneously. Business logic in code may enforce one-basket-per-customer, but the database does not. A race condition (two parallel `CreateBasket` requests) bypasses the code check and creates duplicate active baskets.

**Fix — unique partial index for active baskets:**
```sql
CREATE UNIQUE INDEX UIX_Baskets_CustomerId_Active
    ON "Baskets" ("CustomerId")
    WHERE "Status" = 0;   -- BasketStatus.Active
```

---

### ⚠️ FINDING — DB-5: `BasketItem.ProductId` stored as `text/Guid` with no FK to a Products table
**Location:** `BasketItem.cs:10`, `Migration:48`
**Risk:** `ProductId` is a string in the domain but the migration column type is `uuid`. There is no foreign key to a product catalog table. Products can be deleted or changed without cascading to basket items, leaving orphaned references and stale price data.

**Note:** This is a cross-service FK which intentionally may not exist (microservice boundary). If intentional, document it explicitly. If product catalog is in the same DB, add a FK.

---

### ⚠️ FINDING — DB-6: Stock schema created via raw `EnsureSchema()` — no migration versioning
**Location:** `ServiceCollectionExtensions.cs:40-61`
**Risk:** `EnsureSchema()` runs `CREATE TABLE IF NOT EXISTS` on every startup. It cannot:
- Detect schema drift
- Apply column additions/modifications
- Be rolled back
- Be audited (no migration history)

If `stocks` already exists with different column types (e.g., `quantity` was previously `BIGINT`), the `IF NOT EXISTS` silently succeeds and leaves the schema in the old state.

**Fix:** Replace with EF Core Migrations or Flyway:
```csharp
// In Program.cs startup, use EF migrations:
await dbContext.Database.MigrateAsync();
```
Or add a Flyway volume in `docker-compose.yml` pointing to `scripts/`.

---

## 2. QUERY SAFETY

### ✅ GOOD: No string interpolation in SQL
**Location:** `PostgresStockRepository.cs`
All Dapper queries use `@Param` named parameters. No string interpolation found. SQL injection risk: **none** in stock queries.

---

### ✅ GOOD: EF Core queries are parameterized
**Location:** `BasketRepository.cs:66-68`
EF Core's LINQ-to-SQL always generates parameterized queries.

---

### ⚠️ FINDING — DB-7: N+1 pattern in `BulkIncreaseAsync`
**Location:** `PostgresStockRepository.cs:105-109`

```csharp
// Current — sends one UPDATE per item in a loop
foreach (var item in items)
{
    await conn.ExecuteAsync(
        new CommandDefinition(sql, new { Barcode = item.Barcode, Amount = item.Amount },
            transaction: transaction, cancellationToken: ct));
}
```

**Risk:** For 1,000 items this sends 1,000 individual `UPDATE` statements within one transaction. Each round-trip has network overhead. Under load this causes slow batch processing and long-held transactions (lock contention).

**Fix — use `UNNEST` for a single-statement bulk update:**
```sql
UPDATE stocks AS s
SET warehouse_quantity = s.warehouse_quantity + u.amount,
    last_updated_at    = NOW()
FROM (SELECT UNNEST(@Barcodes::text[]) AS barcode,
             UNNEST(@Amounts::int[])  AS amount) AS u
WHERE s.barcode = u.barcode
```

```csharp
await conn.ExecuteAsync(new CommandDefinition("""
    UPDATE stocks AS s
    SET warehouse_quantity = s.warehouse_quantity + u.amount,
        last_updated_at    = NOW()
    FROM (SELECT UNNEST(@Barcodes::text[]) AS barcode,
                 UNNEST(@Amounts::int[])   AS amount) AS u
    WHERE s.barcode = u.barcode
    """,
    new
    {
        Barcodes = items.Select(i => i.Barcode).ToArray(),
        Amounts  = items.Select(i => i.Amount).ToArray()
    },
    transaction: transaction,
    cancellationToken: ct));
```

---

### ⚠️ FINDING — DB-8: `GetFromDbAsync` has no LIMIT — unbounded query risk
**Location:** `BasketRepository.cs:62-69`

```csharp
return await _context.Baskets
    .Include(b => b.Items)
    .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
```

`FirstOrDefaultAsync` is safe here (single row by PK). However, the `Include(b => b.Items)` performs an eager load of ALL basket items. If a basket accumulates hundreds of items (no item count limit enforced), a single `GetByIdAsync` can return a very large payload causing memory pressure.

**Fix — add a guard and document the assumption:**
```csharp
// In AddItemToBasketCommandHandler — enforce a max items limit:
if (basket.Items.Count >= 100)
    throw new InvalidOperationException("Basket item limit reached (max 100 items).");
```

---

### ⚠️ FINDING — DB-9: `CheckoutBasketCommandHandler` — missing DB persistence + missing transaction boundary
**Location:** `CheckoutBasketCommandHandler.cs:20-35`

```csharp
basket.Status = BasketStatus.Completed;

// Publish to NATS
await _natsConnection.PublishAsync("baskets.checkout", payload, ...);

// Remove from cache
await _repository.DeleteFromCacheAsync(request.Id, cancellationToken);
```

**Risk (Critical):** Three separate operations with **no transaction**:
1. Status is set in-memory only — never written to PostgreSQL
2. NATS publish can succeed but cache delete can fail → basket is sent to checkout but remains "active" in Redis
3. NATS publish happens *before* cache delete — if the process crashes between them, the checkout event is published but the basket is never cleaned up (ghost basket)

**Fix — use transactional outbox pattern (already wired but not used here):**
```csharp
public async Task<bool> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
{
    var basket = await _repository.GetFromCacheAsync(request.Id, cancellationToken)
              ?? await _repository.GetFromDbAsync(request.Id, cancellationToken);
    if (basket == null) return false;

    basket.Status = BasketStatus.Completed;

    // 1. Persist status + outbox message atomically in ONE transaction
    await _repository.CompleteBasketAsync(basket, payload: JsonSerializer.Serialize(basket), cancellationToken);
    // CompleteBasketAsync: UPDATE status + INSERT outbox — single SaveChangesAsync

    // 2. Remove from cache AFTER DB commit
    await _repository.DeleteFromCacheAsync(request.Id, cancellationToken);

    // 3. NATS publish is done by the Outbox processor (not here)
    return true;
}
```

---

### ⚠️ FINDING — DB-10: `CreateBasketCommandHandler` — basket written to cache only, never to DB
**Location:** `CreateBasketCommandHandler.cs:26`

```csharp
await _repository.AddToCacheAsync(basket, cancellationToken);  // Redis only
```

**Risk:** If Redis restarts or the key expires (TTL=30 days), the basket is permanently lost. There is no fallback because `GetByIdAsync` checks DB second, but `AddToDbAsync` is never called during basket creation.

**Fix:**
```csharp
// Write-through: persist to DB first, then cache
await _repository.AddToDbAsync(basket, cancellationToken);
await _repository.AddToCacheAsync(basket, cancellationToken);
```

---

### ⚠️ FINDING — DB-11: `AddItemToBasketCommandHandler` — item changes written to cache only
**Location:** `AddItemToBasketCommandHandler.cs:52`

```csharp
await _repository.AddToCacheAsync(basket, cancellationToken);  // Redis only, no DB update
```

**Risk:** Same as DB-10. All item additions are in-memory/Redis only. If Redis is lost before checkout, the entire basket contents are lost — including item quantities, prices (at the time of adding), and current state.

**Fix:** Either write-through on every mutation or flush to DB periodically (but write-through is simpler and safer):
```csharp
await _repository.UpdateInDbAsync(basket, cancellationToken);
await _repository.AddToCacheAsync(basket, cancellationToken);
```

---

## 3. DATA INTEGRITY

### ✅ GOOD: Idempotent UPSERT with `ON CONFLICT`
**Location:** `PostgresStockRepository.cs:49-57`
`UpsertAsync` and `TryRecordTransactionAsync` both use `ON CONFLICT DO UPDATE / DO NOTHING`. Re-processing the same NATS message is safe.

---

### ✅ GOOD: Transaction for bulk update with rollback
**Location:** `PostgresStockRepository.cs:93-118`
`BulkIncreaseAsync` wraps all updates in a `BeginTransactionAsync` with explicit `RollbackAsync` on failure. Partial failure cannot leave partial state.

---

### ⚠️ FINDING — DB-12: Soft-delete NOT implemented — completed baskets are hard-deleted from cache only
**Location:** `CheckoutBasketCommandHandler.cs:32`

```csharp
await _repository.DeleteFromCacheAsync(request.Id, cancellationToken);
```

**Risk:** The basket entity has a `Status = Completed` field but upon checkout it is only removed from Redis — it is never persisted to PostgreSQL with the Completed status. There is no audit trail of completed baskets. Order history, refund processing, and analytics are all impossible.

**Fix:** Set `Status = Completed`, call `AddToDbAsync` (or `UpdateInDbAsync`), *then* remove from cache. The DB record becomes the permanent audit log.

---

### ⚠️ FINDING — DB-13: No optimistic locking for concurrent basket updates
**Location:** `BasketRepository.cs`, `AddItemToBasketCommandHandler.cs`
**Risk:** Two concurrent requests adding items to the same basket:
1. Request A reads basket from Redis (3 items)
2. Request B reads basket from Redis (3 items)
3. Request A adds item X → writes 4 items to Redis
4. Request B adds item Y → writes 4 items to Redis (overwrites A's write)
→ **Item X is silently lost.**

**Fix — use Redis optimistic locking (WATCH/MULTI/EXEC) or a distributed lock:**
```csharp
// Option 1: ETags / version field on basket
public class Basket
{
    ...
    public int Version { get; set; }  // increment on every write
}

// Option 2: Redis WATCH pattern or Redlock
// Option 3: Move all basket mutations to PostgreSQL with EF Core row version:
modelBuilder.Entity<Basket>()
    .Property(b => b.Version)
    .IsRowVersion();
```

---

### ⚠️ FINDING — DB-14: No batch size limit in `BulkIncreaseAsync`
**Location:** `PostgresStockRepository.cs:87`

```csharp
public async Task BulkIncreaseAsync(List<StockItemIncrease> items, CancellationToken ct = default)
{
    if (items == null || items.Count == 0) return;
    // No upper limit check — 1,000,000 items = one massive transaction
```

**Risk:** Unbounded list can cause:
- Transaction that holds table locks for minutes
- Memory exhaustion (the entire list is held in RAM)
- Statement timeout → full rollback of all work

**Fix — chunk with a batch ceiling:**
```csharp
private const int BatchSize = 500;

public async Task BulkIncreaseAsync(List<StockItemIncrease> items, CancellationToken ct = default)
{
    if (items is null || items.Count == 0) return;

    foreach (var batch in items.Chunk(BatchSize))
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);
        try
        {
            // ... execute batch
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
```

---

### ⚠️ FINDING — DB-15: `processed_transactions` table will grow unbounded
**Location:** `PostgresStockRepository.cs:71-85`, `ServiceCollectionExtensions.cs DDL`
**Risk:** Every NATS message writes a row to `processed_transactions`. There is no TTL, no cleanup job, no partition. This table grows indefinitely. A high-throughput stock feed (e.g., 1M events/day) will exhaust disk space and slow down the deduplication INSERT.

**Fix — add `created_at` index and a background cleanup job:**
```sql
-- Index for efficient cleanup
CREATE INDEX CONCURRENTLY IX_processed_transactions_created_at
    ON processed_transactions (created_at);

-- Cleanup job (cron/background service) — keep 7 days
DELETE FROM processed_transactions WHERE created_at < NOW() - INTERVAL '7 days';
```

Or use PostgreSQL table partitioning by `created_at` (range partitioning) for automatic data lifecycle management.

---

### ⚠️ FINDING — DB-16: Redis `SeedAsync` has no TTL — keys live forever
**Location:** `RedisStockReservationService.cs:80`

```csharp
await db.StringSetAsync(StockKey(barcode), quantity);  // No expiry!
```

**Risk:** Stock keys seeded from PostgreSQL never expire. If a product is discontinued or its barcode is reassigned, the stale Redis value persists indefinitely and is used for stock checks. Memory grows unbounded as more products are added.

**Fix:**
```csharp
await db.StringSetAsync(StockKey(barcode), quantity, expiry: TimeSpan.FromHours(24));
```

---

### ⚠️ FINDING — DB-17: Redis `ReleaseAsync` can create a key from nothing
**Location:** `RedisStockReservationService.cs:73`

```csharp
var remaining = await db.StringIncrementAsync(StockKey(barcode), quantity);
```

**Risk:** If the Redis key does not exist (e.g., expired TTL, Redis restart), `INCRBY` creates it with value = `quantity`. A stock release of 5 on a non-existent key sets available stock to 5 — **phantom stock appears from nothing**. The system becomes inconsistent between PostgreSQL (correct) and Redis (fabricated).

**Fix — use Lua to check existence before incrementing:**
```lua
local current = redis.call('GET', KEYS[1])
if current == false then
    return -1  -- key not found, caller must reseed
end
return tonumber(redis.call('INCRBY', KEYS[1], ARGV[1]))
```

```csharp
public async Task ReleaseAsync(string barcode, int quantity, CancellationToken ct = default)
{
    var db = _redis.GetDatabase();
    var result = (long?)await db.ScriptEvaluateAsync(ReleaseLua, [StockKey(barcode)], [(RedisValue)quantity]);
    if (result == -1)
    {
        _logger.LogWarning("Redis key not found during release for {Barcode} — skipping INCRBY to avoid phantom stock", barcode);
        // Optionally reseed from PostgreSQL
    }
}
```

---

## 4. MIGRATION SAFETY

### ✅ GOOD: EF Core migrations have `Down()` implemented
**Location:** `20260311193007_AddOutboxMessage.cs:71-81`
The `Down()` migration drops all three tables in correct dependency order (items → outbox → baskets). Rollback is possible.

---

### ✅ GOOD: Migration uses correct PostgreSQL types
`uuid`, `text`, `numeric(18,2)`, `timestamp with time zone`, `integer` — all native PostgreSQL types. No lossy type mappings.

---

### ⚠️ FINDING — DB-18: Migration name `AddOutboxMessage` misleads — it ALSO creates the initial `Baskets` and `BasketItems` schema
**Location:** `20260311193007_AddOutboxMessage.cs`
**Risk:** When a developer reads migration history, they assume `AddOutboxMessage` only added the outbox table. In reality it creates the entire initial schema. This makes it impossible to understand what the database looked like *before* this migration. Missing an `InitialSchema` migration as a separate step.

**Fix:** Rename to `InitialSchema_WithOutbox` or split into two migrations:
- `20260311_000000_InitialSchema.cs` — Baskets + BasketItems
- `20260311_193007_AddOutboxMessage.cs` — OutboxMessages only

---

### ⚠️ FINDING — DB-19: Stock schema bootstrapped outside of migration system — no Down migration exists
**Location:** `ServiceCollectionExtensions.cs:40-61`
**Risk:** `EnsureSchema()` has no rollback. There is no way to:
- Roll back stock schema changes
- Know what version the schema is at
- Apply column modifications (you can only `CREATE IF NOT EXISTS`, not `ALTER`)

If a column needs renaming or a constraint needs adding, a developer must write raw SQL directly on the production database — outside any controlled deployment pipeline.

**Fix:** Migrate stocks to EF Core or Flyway migration management. If raw Dapper is preferred for Stock, use Flyway's versioned `.sql` files:
```
scripts/
  V1__initial_schema.sql    → CREATE TABLE stocks, processed_transactions
  V2__add_warehouse_index.sql
```

---

### ⚠️ FINDING — DB-20: `EnsureSchema()` is called synchronously on startup — blocks the startup thread
**Location:** `ServiceCollectionExtensions.cs:35`, `40-61`

```csharp
// Called during DI registration (IServiceCollection.Add*) — synchronous!
EnsureSchema(connectionString);
```

**Risk:** If PostgreSQL is temporarily unavailable at startup (race condition in Docker Compose, pod scheduling), `EnsureSchema()` throws an unhandled exception during DI container construction — the application crashes before it can even start. There is no retry.

**Fix:** Move schema initialization to `IHostedService` or `IAsyncInitializable` with retry:
```csharp
public class StockSchemaInitializer(IConfiguration config, ILogger<StockSchemaInitializer> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        var connectionString = config.GetConnectionString("StockDb")!;
        var policy = Policy.Handle<NpgsqlException>()
            .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 2),
                (ex, ts) => logger.LogWarning("DB not ready, retrying in {Delay}s...", ts.TotalSeconds));

        await policy.ExecuteAsync(async () =>
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(ddl, conn);
            await cmd.ExecuteNonQueryAsync(ct);
        });
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

---

## Summary Table

| ID | Area | Severity | Location | Issue |
|---|---|---|---|---|
| DB-1 | Schema | 🟡 Medium | `AppDbContext.cs` | `BasketStatus` not constrained at DB level |
| DB-2 | Schema | 🟡 Medium | `Migration` | Missing index on `Baskets.CustomerId` |
| DB-3 | Schema | 🟡 Medium | `Migration` | Missing partial index on `OutboxMessages.ProcessedOn IS NULL` |
| DB-4 | Schema | 🔴 High | `Migration` | No unique constraint — one customer can have multiple active baskets |
| DB-5 | Schema | ℹ️ Info | `Migration` | No FK between `BasketItem.ProductId` and products table (cross-service) |
| DB-6 | Schema/Migration | 🔴 High | `ServiceCollectionExtensions.cs` | Stock schema unversioned, no rollback possible |
| DB-7 | Query | 🟡 Medium | `PostgresStockRepository.cs:105` | N+1 UPDATE loop in `BulkIncreaseAsync` |
| DB-8 | Query | 🟡 Medium | `BasketRepository.cs:62` | No basket item count limit → unbounded eager load |
| DB-9 | Integrity | 🔴 Critical | `CheckoutBasketCommandHandler.cs` | No transaction — publish + delete are not atomic; basket never persisted |
| DB-10 | Integrity | 🔴 Critical | `CreateBasketCommandHandler.cs` | Basket created in Redis only — lost on Redis restart |
| DB-11 | Integrity | 🔴 Critical | `AddItemToBasketCommandHandler.cs` | Item updates never written to PostgreSQL |
| DB-12 | Integrity | 🔴 High | `CheckoutBasketCommandHandler.cs` | No audit trail — completed baskets vanish |
| DB-13 | Integrity | 🔴 High | `BasketRepository.cs` | No optimistic locking — concurrent add-to-basket loses writes |
| DB-14 | Integrity | 🟡 Medium | `PostgresStockRepository.cs:87` | No batch size limit → unbounded transaction |
| DB-15 | Integrity | 🟡 Medium | `processed_transactions` DDL | Table grows unbounded — no cleanup |
| DB-16 | Integrity | 🟡 Medium | `RedisStockReservationService.cs:80` | Seeded Redis keys have no TTL |
| DB-17 | Integrity | 🔴 High | `RedisStockReservationService.cs:73` | `INCRBY` on missing key creates phantom stock |
| DB-18 | Migration | ℹ️ Info | `AddOutboxMessage.cs` | Misleading migration name |
| DB-19 | Migration | 🔴 High | `ServiceCollectionExtensions.cs` | Stock has no down migration, no schema version control |
| DB-20 | Migration | 🟡 Medium | `ServiceCollectionExtensions.cs:35` | `EnsureSchema()` is sync, no retry — crashes on DB unavailability |

---

## Critical Path (Fix These First)

1. **DB-9** — `CheckoutBasketCommandHandler`: Add transaction boundary + DB persistence. Data loss on every checkout.
2. **DB-10 + DB-11** — Basket and item writes go to Redis only. PostgreSQL is never the source of truth.
3. **DB-4** — Add unique constraint for active baskets per customer. Race condition creates duplicate carts.
4. **DB-17** — Phantom stock from Redis `INCRBY` on missing key.
5. **DB-13** — Lost writes from concurrent basket item additions.
