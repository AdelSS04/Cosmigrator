---
title: MigrationRunner
sidebar_position: 3
---

# MigrationRunner

Orchestrates discovering, running, rolling back, and reporting on Cosmos DB migrations. This is the central engine of Cosmigrator.

```csharp
namespace Cosmigrator;

public class MigrationRunner
{
    public MigrationRunner(
        CosmosClient client,
        string databaseName,
        ILoggerFactory loggerFactory,
        params Assembly[] migrationAssemblies);

    public async Task InitializeAsync();
    public async Task RunPendingMigrationsAsync();
    public async Task RollbackAsync(int steps = 1);
    public async Task PrintStatusAsync();
    public void PrintDiscoveredMigrations();
}
```

## Constructor

| Parameter | Type | Description |
|-----------|------|-------------|
| `client` | `CosmosClient` | The Cosmos DB client |
| `databaseName` | `string` | Target database name |
| `loggerFactory` | `ILoggerFactory` | Logger factory for typed loggers |
| `migrationAssemblies` | `params Assembly[]` | Assemblies to scan for `IMigration` implementations. Defaults to entry assembly if none specified |

The constructor immediately calls `MigrationDiscovery.DiscoverAll` to find all migrations.

## Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `InitializeAsync()` | `Task` | Initializes the `__MigrationHistory` container reference. Must be called before any other operations |
| `RunPendingMigrationsAsync()` | `Task` | Discovers and runs all pending migrations in `Id` order. Exits with code `0` on success, `1` on failure |
| `RollbackAsync(int steps)` | `Task` | Rolls back the last `steps` applied migrations in reverse order. Default: `1` |
| `PrintStatusAsync()` | `Task` | Prints the status of all discovered migrations (Applied / Pending / RolledBack) |
| `PrintDiscoveredMigrations()` | `void` | Lists all discovered migration classes from the assembly |

## InitializeAsync

Must be called before any other method. Initializes the reference to the `__MigrationHistory` container.

```csharp
var runner = new MigrationRunner(client, "MyDb", loggerFactory, assembly);
await runner.InitializeAsync();
```

## RunPendingMigrationsAsync

```csharp
await runner.RunPendingMigrationsAsync();
```

1. Loads applied migrations from history
2. Filters discovered migrations to those not yet applied
3. Sorts pending by `Id`
4. For each: calls `UpAsync`, then `MarkAsAppliedAsync`
5. If any migration throws, logs the error and exits with code `1`
6. On complete success, exits with code `0`

## RollbackAsync

```csharp
await runner.RollbackAsync(steps: 3);
```

1. Loads applied migrations, sorted in reverse `Id` order
2. Takes the last `steps` migrations
3. For each: finds the matching `IMigration` class, calls `DownAsync`, then `MarkAsRolledBackAsync`
4. If the migration class isn't found in the assembly, logs a warning and skips

## PrintStatusAsync

Cross-references discovered migrations with history records:

```csharp
await runner.PrintStatusAsync();
// Output:
// [20250219_000001] AddEmailToUsers - Applied at 2025-02-19 14:30:00 UTC
// [20250219_000002] RemoveMiddleName - Pending
```

## PrintDiscoveredMigrations

Lists all discovered migrations without checking history:

```csharp
runner.PrintDiscoveredMigrations();
// Output:
// [20250219_000001] AddEmailToUsers -> Users
// [20250219_000002] RemoveMiddleName -> Users
// Total: 2 migration(s) discovered
```
