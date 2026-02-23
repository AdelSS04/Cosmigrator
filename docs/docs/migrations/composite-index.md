---
title: Composite Index
sidebar_position: 5
---

# Add a Composite Index

Add a composite index to a container's indexing policy without modifying documents.

## Use case

You need to support efficient `ORDER BY lastName, firstName` queries on your `Users` container. Cosmos DB requires a composite index for multi-property ordering.

## Migration code

```csharp
using System.Collections.ObjectModel;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

public class _20250219_000005_AddCompositeIndexToUsers : IMigration
{
    public string Id => "20250219_000005";
    public string Name => "AddCompositeIndexToUsers";
    public string ContainerName => "Users";

    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger("CompositeIndexMigration");

        var response = await container.ReadContainerAsync();
        var properties = response.Resource;

        // Check if composite index already exists
        var alreadyExists = properties.IndexingPolicy.CompositeIndexes
            .Any(ci =>
                ci.Count == 2 &&
                ci.Any(p => p.Path == "/lastName"
                    && p.Order == CompositePathSortOrder.Ascending) &&
                ci.Any(p => p.Path == "/firstName"
                    && p.Order == CompositePathSortOrder.Ascending));

        if (alreadyExists) return;

        // Add the composite index
        var compositeIndex = new Collection<CompositePath>
        {
            new() { Path = "/lastName",  Order = CompositePathSortOrder.Ascending },
            new() { Path = "/firstName", Order = CompositePathSortOrder.Ascending }
        };

        properties.IndexingPolicy.CompositeIndexes.Add(compositeIndex);
        await container.ReplaceContainerAsync(properties);
    }

    public async Task DownAsync(Container container, CosmosClient client)
    {
        var response = await container.ReadContainerAsync();
        var properties = response.Resource;

        var indexToRemove = properties.IndexingPolicy.CompositeIndexes
            .FirstOrDefault(ci =>
                ci.Count == 2 &&
                ci.Any(p => p.Path == "/lastName") &&
                ci.Any(p => p.Path == "/firstName"));

        if (indexToRemove != null)
        {
            properties.IndexingPolicy.CompositeIndexes.Remove(indexToRemove);
            await container.ReplaceContainerAsync(properties);
        }
    }
}
```

## How it works

Unlike document-level migrations, index changes operate on the **container properties** rather than individual documents:

1. Read the current container properties with `container.ReadContainerAsync()`
2. Check if the index already exists (idempotency)
3. Add the composite index to `IndexingPolicy.CompositeIndexes`
4. Apply with `container.ReplaceContainerAsync(properties)`

## Key differences from document migrations

| Aspect | Document migration | Index migration |
|--------|-------------------|-----------------|
| Uses `BulkOperationHelper` | Yes | No |
| Modifies documents | Yes | No |
| Uses `ReplaceContainerAsync` | No | Yes |
| Downtime | Depends on document count | Near-zero |
| RU cost | Proportional to documents | Minimal |

## Idempotency

The migration checks if the composite index already exists before adding it. This means running the migration multiple times is safe — it won't create duplicate indexes.

## Rollback

`DownAsync` finds the matching composite index by its paths and removes it. If the index was already removed (e.g., manually), the rollback is a no-op.
