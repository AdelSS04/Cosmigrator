---
title: Rename a Property
sidebar_position: 3
---

# Rename a Property

Rename a property on all documents while preserving the original value.

## Use case

You're renaming `userName` to `displayName` across all user documents.

## Migration code

```csharp
using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

public class _20250219_000003_RenameUserNameToDisplayName : IMigration
{
    public string Id => "20250219_000003";
    public string Name => "RenameUserNameToDisplayName";
    public string ContainerName => "Users";

    private const string OldPropertyName = "userName";
    private const string NewPropertyName = "displayName";

    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadDocumentsAsync(
            container,
            $"SELECT * FROM c WHERE IS_DEFINED(c.{OldPropertyName})");

        if (docs.Count == 0) return;

        foreach (var doc in docs)
        {
            doc[NewPropertyName] = doc[OldPropertyName]!.DeepClone();
            doc.Remove(OldPropertyName);
        }

        await helper.BulkUpsertAsync(container, docs);
    }

    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadDocumentsAsync(
            container,
            $"SELECT * FROM c WHERE IS_DEFINED(c.{NewPropertyName})");

        if (docs.Count == 0) return;

        foreach (var doc in docs)
        {
            doc[OldPropertyName] = doc[NewPropertyName]!.DeepClone();
            doc.Remove(NewPropertyName);
        }

        await helper.BulkUpsertAsync(container, docs);
    }
}
```

## How it works

1. Query for documents that have the old property name
2. Copy the value using `DeepClone()` to preserve nested objects and arrays
3. Remove the old property
4. Bulk upsert the modified documents
5. `DownAsync` reverses the operation — copies `displayName` back to `userName`

## Why DeepClone?

`JsonNode.DeepClone()` creates an independent copy of the value. Without it, the `JsonNode` is still attached to the old property — removing the old property would also remove the value from the new property. `DeepClone()` severs that link.

```csharp
// Without DeepClone — the value is shared, removing old breaks new
doc["displayName"] = doc["userName"];     // ❌ shared reference
doc.Remove("userName");                   // removes both

// With DeepClone — independent copy
doc["displayName"] = doc["userName"]!.DeepClone();  // ✅ independent copy
doc.Remove("userName");                              // only removes old
```
