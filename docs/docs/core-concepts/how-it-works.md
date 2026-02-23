---
title: How It Works
sidebar_position: 1
---

# How It Works

Cosmigrator follows a simple pipeline: **discover → compare → execute → record**.

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                   MigrationHost                     │
│   Builds host, reads config, creates CosmosClient   │
└──────────────────────┬──────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────┐
│                 MigrationRunner                     │
│   Orchestrates discover → compare → execute         │
├──────────────┬──────────────────┬───────────────────┤
│              │                  │                    │
│  MigrationDiscovery    MigrationHistory    IMigration │
│  (reflection scan)     (__MigrationHistory)  (your code) │
└──────────────┴──────────────────┴───────────────────┘
```

## Step by step

### 1. Bootstrap

`MigrationHost.RunAsync` builds a .NET Generic Host with:
- Configuration from `appsettings.json`, environment variables, and CLI args
- Serilog logging with console output
- A `CosmosClient` configured with `System.Text.Json` serialization and bulk execution

### 2. Discovery

`MigrationDiscovery.DiscoverAll` scans the provided assembly (or the entry assembly) for all concrete classes implementing `IMigration`. It instantiates each via `Activator.CreateInstance` and sorts them by `Id`.

```csharp
var migrations = MigrationDiscovery.DiscoverAll(Assembly.GetExecutingAssembly());
// Returns: List<IMigration> sorted by Id
```

### 3. Comparison

`MigrationRunner` reads all records from the `__MigrationHistory` container, filters to those with status `Applied`, and compares against discovered migrations. Any migration whose `Id` is not in the applied set is considered **pending**.

### 4. Execution

Pending migrations execute in `Id` order. For each:

1. Get the target container: `database.GetContainer(migration.ContainerName)`
2. Call `migration.UpAsync(container, client)`
3. Record in history: `MigrationHistory.MarkAsAppliedAsync(migration)`

If any migration throws, execution halts and the process exits with code `1`.

### 5. History recording

Each applied migration creates a `MigrationRecord` in the `__MigrationHistory` container:

```json
{
  "id": "20250219_000001",
  "name": "AddEmailToUsers",
  "appliedAt": "2025-02-19T14:30:00Z",
  "status": "Applied"
}
```

The `id` field is both the document ID and partition key. This means each migration has exactly one record.

## Key design decisions

- **No automatic container creation** — you provision containers yourself (Terraform, Bicep, etc.)
- **No dependency injection in migrations** — migrations create their own helpers. This keeps them self-contained and portable
- **Exit codes for CI/CD** — `0` for success, `1` for failure, making it easy to use as pipeline steps
- **Id-based ordering** — use the convention `YYYYMMDD_NNNNNN` for chronological execution
