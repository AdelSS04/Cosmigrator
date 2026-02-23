---
title: Remove a Property
sidebar_position: 2
---

# Remove a Property

Remove a property from all documents in a container.

## Use case

You're deprecating the `middleName` field from user documents and want to clean it up across all existing records.

## Migration code

```csharp
using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

public class _20250219_000002_RemoveMiddleName : IMigration
{
    public string Id => "20250219_000002";
    public string Name => "RemoveMiddleName";
    public string ContainerName => "Users";

    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadDocumentsAsync(
            container,
            "SELECT * FROM c WHERE IS_DEFINED(c.middleName)");

        if (docs.Count == 0) return;

        foreach (var doc in docs)
            doc.Remove("middleName");

        await helper.BulkUpsertAsync(container, docs);
    }

    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadDocumentsAsync(
            container,
            "SELECT * FROM c WHERE NOT IS_DEFINED(c.middleName)");

        if (docs.Count == 0) return;

        foreach (var doc in docs)
            doc["middleName"] = "";

        await helper.BulkUpsertAsync(container, docs);
    }
}
```

## How it works

1. `UpAsync` uses `WHERE IS_DEFINED(c.middleName)` to find only documents that still have the property
2. `JsonObject.Remove()` deletes the property from the in-memory document
3. `BulkUpsertAsync` writes the updated documents back — Cosmos DB replaces the entire document, so the property is gone
4. `DownAsync` re-adds the property with an empty string default for rollback

## Notes

- Removing a property from a Cosmos DB document requires upserting the entire document without that property
- The SQL filter ensures you only process relevant documents
- The rollback adds the property back, but the original values are lost — if you need to preserve them, consider a more sophisticated approach (e.g., copying to a backup property first)
