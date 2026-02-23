---
title: Add a Property
sidebar_position: 1
---

# Add a Property

Add a new property with a default value to all documents in a container.

## Use case

You're adding an `age` field to all user documents. Existing documents don't have this property, and you want to backfill them with a default value.

## Migration code

```csharp
using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

public class _20250219_000001_AddAgeToUsers : IMigration
{
    public string Id => "20250219_000001";
    public string Name => "AddAgeToUsers";
    public string ContainerName => "Users";
    public object? DefaultValue => null;

    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        // Only fetch documents that don't already have the property
        var docs = await helper.ReadDocumentsAsync(
            container,
            "SELECT * FROM c WHERE NOT IS_DEFINED(c.age)");

        if (docs.Count == 0) return;

        foreach (var doc in docs)
            doc["age"] = JsonValue.Create(DefaultValue);

        await helper.BulkUpsertAsync(container, docs);
    }

    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadDocumentsAsync(
            container,
            "SELECT * FROM c WHERE IS_DEFINED(c.age)");

        if (docs.Count == 0) return;

        foreach (var doc in docs)
            doc.Remove("age");

        await helper.BulkUpsertAsync(container, docs);
    }
}
```

## How it works

1. `ReadDocumentsAsync` uses a SQL query with `WHERE NOT IS_DEFINED(c.age)` to only fetch documents that need updating — no unnecessary reads
2. Each document gets the new property set to `DefaultValue` (which can be any type: `int`, `string`, `null`, `Guid.Empty`, etc.)
3. `BulkUpsertAsync` writes all modified documents back in configurable batches with automatic 429 retry

## The DefaultValue property

`IMigration.DefaultValue` is an optional interface member with a default implementation of `null`. Override it to set any default:

```csharp
public object? DefaultValue => 0;           // int
public object? DefaultValue => "";          // empty string
public object? DefaultValue => Guid.Empty;  // Guid
public object? DefaultValue => null;        // null (default)
```

## Targeted queries

The `ReadDocumentsAsync` method accepts any valid Cosmos DB SQL query. Using `WHERE NOT IS_DEFINED(c.age)` ensures you only process documents that actually need the new property, which is much more efficient than loading everything with `ReadAllDocumentsAsync`.
