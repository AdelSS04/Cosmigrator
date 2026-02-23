---
title: Unique Key Policy
sidebar_position: 4
---

# Change Unique Key Policy

Add or modify a unique key policy on a Cosmos DB container. Since Cosmos DB doesn't support modifying unique keys after container creation, this requires a full container swap.

## Use case

You need to add a unique key constraint on `/orderNumber` to your `Orders` container to prevent duplicate order numbers.

## Migration code

```csharp
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

public class _20250219_000004_AddUniqueKeyToOrders : IMigration
{
    public string Id => "20250219_000004";
    public string Name => "AddUniqueKeyToOrders";
    public string ContainerName => "Orders";

    private const string TempContainerName = "Orders_temp_migration";
    private const string PartitionKeyPath = "/customerId";
    private const string PartitionKeyProperty = "customerId";

    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger("UniqueKeyMigration");
        var bulkHelper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>(), batchSize: 100);

        var database = client.GetDatabase(container.Database.Id);

        // Read existing container properties
        var existingPropsResponse = await container.ReadContainerAsync();
        var existingProps = existingPropsResponse.Resource;

        // Check if the unique key already exists
        var alreadyExists = existingProps.UniqueKeyPolicy.UniqueKeys
            .Any(k => k.Paths.Count == 1 && k.Paths[0] == "/orderNumber");

        if (alreadyExists) return;

        // Add the new unique key
        existingProps.UniqueKeyPolicy.UniqueKeys.Add(new UniqueKey
        {
            Paths = { "/orderNumber" }
        });

        // Step 1: Create temp container with updated policy
        var tempProps = new ContainerProperties(TempContainerName, PartitionKeyPath)
        {
            UniqueKeyPolicy = existingProps.UniqueKeyPolicy,
            IndexingPolicy = existingProps.IndexingPolicy,
        };
        var tempResponse = await database.CreateContainerIfNotExistsAsync(tempProps);
        var tempContainer = tempResponse.Container;

        // Step 2: Copy documents to temp container
        await bulkHelper.CopyAllDocumentsAsync(container, tempContainer, PartitionKeyProperty);

        // Step 3: Delete original container
        await container.DeleteContainerAsync();

        // Step 4: Recreate with new policy
        var newProps = new ContainerProperties(ContainerName, PartitionKeyPath)
        {
            UniqueKeyPolicy = existingProps.UniqueKeyPolicy,
            IndexingPolicy = existingProps.IndexingPolicy,
        };
        var newResponse = await database.CreateContainerAsync(newProps);
        var newContainer = newResponse.Container;

        // Step 5: Copy documents back
        await bulkHelper.CopyAllDocumentsAsync(tempContainer, newContainer, PartitionKeyProperty);

        // Step 6: Delete temp container
        await tempContainer.DeleteContainerAsync();
    }

    public async Task DownAsync(Container container, CosmosClient client)
    {
        // Reverse: recreate without the unique key
        // Same swap pattern in reverse
    }
}
```

## The 6-step swap

Since Cosmos DB doesn't allow modifying unique key policies after creation, you must:

1. **Create temp container** with the desired unique key policy
2. **Copy all documents** from original → temp using `BulkOperationHelper.CopyAllDocumentsAsync`
3. **Delete original container**
4. **Recreate original** with the new unique key policy
5. **Copy all documents** back from temp → new original
6. **Delete temp container**

`BulkOperationHelper.CopyAllDocumentsAsync` handles the read + bulk upsert with automatic batching and 429 retry.

## Risks

This migration deletes and recreates the container. During the swap:

- The container is temporarily unavailable
- If the migration fails mid-way, you need the temp container to recover data
- Run this during a maintenance window

## Preserving container settings

When recreating the container, make sure to copy all relevant properties from the original:

- `IndexingPolicy`
- `DefaultTimeToLive`
- `ConflictResolutionPolicy`
- `AnalyticalStoreTimeToLiveInSeconds`
