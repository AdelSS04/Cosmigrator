---
title: MigrationHistory
sidebar_position: 4
---

# MigrationHistory

Manages the `__MigrationHistory` container in Cosmos DB. Tracks which migrations have been applied, rolled back, or are pending.

```csharp
namespace Cosmigrator;

public class MigrationHistory(Database database, ILogger<MigrationHistory> logger)
{
    public Task InitializeAsync();
    public async Task<List<MigrationRecord>> GetAllRecordsAsync();
    public async Task<List<MigrationRecord>> GetAppliedMigrationsAsync();
    public async Task MarkAsAppliedAsync(IMigration migration);
    public async Task MarkAsRolledBackAsync(string migrationId);
}
```

## Constructor

Uses a C# 12 primary constructor.

| Parameter | Type | Description |
|-----------|------|-------------|
| `database` | `Database` | The Cosmos DB database instance |
| `logger` | `ILogger<MigrationHistory>` | Logger instance |

## Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `InitializeAsync()` | `Task` | Gets a reference to the `__MigrationHistory` container. The container must already exist |
| `GetAllRecordsAsync()` | `Task<List<MigrationRecord>>` | Returns all migration records ordered by `id` ascending |
| `GetAppliedMigrationsAsync()` | `Task<List<MigrationRecord>>` | Returns only records with status `Applied`, ordered by `id` |
| `MarkAsAppliedAsync(IMigration)` | `Task` | Creates or updates a record with status `Applied` and current UTC timestamp |
| `MarkAsRolledBackAsync(string)` | `Task` | Updates an existing record to status `RolledBack`. Logs a warning if the record doesn't exist |

## InitializeAsync

Gets a reference to the `__MigrationHistory` container. Does **not** create the container — it must already exist in your database.

```csharp
var history = new MigrationHistory(database, logger);
await history.InitializeAsync();
```

Throws `InvalidOperationException` if you call any other method before `InitializeAsync`.

## GetAllRecordsAsync

Queries `SELECT * FROM c ORDER BY c.id ASC` and returns all `MigrationRecord` documents.

## GetAppliedMigrationsAsync

Loads all records via `GetAllRecordsAsync`, then filters in-memory to those with `Status == MigrationStatus.Applied`.

## MarkAsAppliedAsync

```csharp
await history.MarkAsAppliedAsync(migration);
```

Creates a `MigrationRecord` with:
- `Id` = `migration.Id`
- `Name` = `migration.Name`
- `AppliedAt` = `DateTime.UtcNow`
- `Status` = `MigrationStatus.Applied`

Uses `UpsertItemAsync` — safe to call multiple times.

## MarkAsRolledBackAsync

```csharp
await history.MarkAsRolledBackAsync("20250219_000001");
```

1. Reads the existing record by `migrationId`
2. Updates `Status` to `RolledBack` and `AppliedAt` to current UTC time
3. Upserts the modified record
4. If the record doesn't exist (404), logs a warning and continues
