---
title: IMigration
sidebar_position: 1
---

# IMigration

The core interface that every migration class must implement. Defined in the `Cosmigrator` namespace.

```csharp
namespace Cosmigrator;

public interface IMigration
{
    string Id { get; }
    string Name { get; }
    string ContainerName { get; }
    object? DefaultValue => null;

    Task UpAsync(Container container, CosmosClient client);
    Task DownAsync(Container container, CosmosClient client);
}
```

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Unique identifier used for ordering and history tracking. Convention: `"YYYYMMDD_NNNNNN"` |
| `Name` | `string` | Human-readable name describing what the migration does |
| `ContainerName` | `string` | Name of the target Cosmos DB container this migration operates on |
| `DefaultValue` | `object?` | Optional default value for property additions. Default implementation returns `null` |

## Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `UpAsync(Container, CosmosClient)` | `Task` | Applies the migration forward. Receives the target container and the Cosmos client for advanced operations |
| `DownAsync(Container, CosmosClient)` | `Task` | Rolls back the migration. Receives the same parameters as `UpAsync` |

## Parameters

### UpAsync / DownAsync

| Parameter | Type | Description |
|-----------|------|-------------|
| `container` | `Microsoft.Azure.Cosmos.Container` | The target container resolved from `ContainerName` |
| `client` | `Microsoft.Azure.Cosmos.CosmosClient` | The Cosmos client — use for operations that need database-level access (e.g., creating containers) |

## Id convention

The `Id` property determines execution order. Migrations are sorted lexicographically by `Id`, so use a timestamp-based format:

```
YYYYMMDD_NNNNNN
```

Examples:
- `"20250101_000001"` — first migration
- `"20250101_000002"` — second migration
- `"20250215_000001"` — migration added on Feb 15

## DefaultValue

`DefaultValue` has a default interface implementation of `null`. Override it to specify a default when adding new properties:

```csharp
public object? DefaultValue => 0;           // int
public object? DefaultValue => "";          // string
public object? DefaultValue => Guid.Empty;  // Guid
public object? DefaultValue => null;        // null (default)
```

## Example

```csharp
public class _20250219_000001_AddEmailToUsers : IMigration
{
    public string Id => "20250219_000001";
    public string Name => "AddEmailToUsers";
    public string ContainerName => "Users";
    public object? DefaultValue => "";

    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadDocumentsAsync(
            container,
            "SELECT * FROM c WHERE NOT IS_DEFINED(c.email)");

        foreach (var doc in docs)
            doc["email"] = JsonValue.Create(DefaultValue);

        if (docs.Count > 0)
            await helper.BulkUpsertAsync(container, docs);
    }

    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadDocumentsAsync(
            container,
            "SELECT * FROM c WHERE IS_DEFINED(c.email)");

        foreach (var doc in docs)
            doc.Remove("email");

        if (docs.Count > 0)
            await helper.BulkUpsertAsync(container, docs);
    }
}
```
