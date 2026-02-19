using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmigrator.Sample.Migrations;

/// <summary>
/// Scenario D — Change/Add a Unique Key Policy.
/// Since Cosmos DB unique keys cannot be modified after container creation, this
/// migration performs a full container swap:
///   1. Create a temporary container with the new UniqueKeyPolicy
///   2. Copy all documents from the original container to temp
///   3. Delete the original container
///   4. Recreate the original container with the new UniqueKeyPolicy
///   5. Copy all documents from temp back to the new original container
///   6. Delete the temp container
/// Includes retry logic and progress logging via <see cref="BulkOperationHelper"/>.
/// </summary>
public class _20240101_000004_AddUniqueKeyPolicyToOrders : IMigration
{
    /// <inheritdoc />
    public string Id => "20240101_000004";

    /// <inheritdoc />
    public string Name => "AddUniqueKeyPolicyToOrders";

    /// <inheritdoc />
    public string ContainerName => "Orders";

    private const string TempContainerName = "Orders_temp_migration";
    private const string PartitionKeyPath = "/customerId";
    private const string PartitionKeyProperty = "customerId";

    /// <inheritdoc />
    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000004_AddUniqueKeyPolicyToOrders>();
        var bulkHelper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>(), batchSize: 100);

        var database = client.GetDatabase(container.Database.Id);

        logger.LogInformation("Starting unique key policy migration for 'Orders'...");

        // Read existing container properties to preserve everything
        var existingPropertiesResponse = await container.ReadContainerAsync();
        var existingProperties = existingPropertiesResponse.Resource;

        // Add the new unique key to existing policy
        existingProperties.UniqueKeyPolicy.UniqueKeys.Add(new UniqueKey
        {
            Paths = { "/orderNumber" }
        });

        logger.LogInformation("Step 1/6: Creating temporary container with updated UniqueKeyPolicy...");
        // Clone properties for temp container (same name different, everything else identical)
        var tempContainerProperties = new ContainerProperties(TempContainerName, PartitionKeyPath)
        {
            UniqueKeyPolicy = existingProperties.UniqueKeyPolicy,
            IndexingPolicy = existingProperties.IndexingPolicy,
            DefaultTimeToLive = existingProperties.DefaultTimeToLive,
            ConflictResolutionPolicy = existingProperties.ConflictResolutionPolicy,
            AnalyticalStoreTimeToLiveInSeconds = existingProperties.AnalyticalStoreTimeToLiveInSeconds
        };
        var tempContainerResponse = await database.CreateContainerIfNotExistsAsync(tempContainerProperties);
        var tempContainer = tempContainerResponse.Container;
        logger.LogInformation("Temporary container created");

        logger.LogInformation("Step 2/6: Copying documents from original to temp container...");
        await bulkHelper.CopyAllDocumentsAsync(container, tempContainer, PartitionKeyProperty);

        logger.LogInformation("Step 3/6: Deleting original container...");
        await container.DeleteContainerAsync();

        logger.LogInformation("Step 4/6: Recreating original container with new UniqueKeyPolicy...");
        // Use the same properties as temp container (with updated unique keys)
        var newContainerProperties = new ContainerProperties(ContainerName, PartitionKeyPath)
        {
            UniqueKeyPolicy = existingProperties.UniqueKeyPolicy,
            IndexingPolicy = existingProperties.IndexingPolicy,
            DefaultTimeToLive = existingProperties.DefaultTimeToLive,
            ConflictResolutionPolicy = existingProperties.ConflictResolutionPolicy,
            AnalyticalStoreTimeToLiveInSeconds = existingProperties.AnalyticalStoreTimeToLiveInSeconds
        };
        var newContainerResponse = await database.CreateContainerAsync(newContainerProperties);
        var newContainer = newContainerResponse.Container;

        logger.LogInformation("Step 5/6: Copying documents back from temp to new original container...");
        await bulkHelper.CopyAllDocumentsAsync(tempContainer, newContainer, PartitionKeyProperty);

        logger.LogInformation("Step 6/6: Deleting temporary container...");
        await tempContainer.DeleteContainerAsync();
        logger.LogInformation("Migration complete");
    }

    /// <inheritdoc />
    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000004_AddUniqueKeyPolicyToOrders>();
        var bulkHelper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>(), batchSize: 100);

        var database = client.GetDatabase(container.Database.Id);

        logger.LogInformation("Rolling back unique key policy migration for 'Orders'...");

        // Read existing container properties
        var existingPropertiesResponse = await container.ReadContainerAsync();
        var existingProperties = existingPropertiesResponse.Resource;

        // Remove the /orderNumber unique key
        var uniqueKeyToRemove = existingProperties.UniqueKeyPolicy.UniqueKeys
            .FirstOrDefault(key => key.Paths.Count == 1 && key.Paths[0] == "/orderNumber");

        if (uniqueKeyToRemove != null)
        {
            existingProperties.UniqueKeyPolicy.UniqueKeys.Remove(uniqueKeyToRemove);
        }

        logger.LogInformation("Step 1/6: Creating temporary container with original UniqueKeyPolicy (without /orderNumber)...");
        var tempContainerProperties = new ContainerProperties(TempContainerName, PartitionKeyPath)
        {
            UniqueKeyPolicy = existingProperties.UniqueKeyPolicy,
            IndexingPolicy = existingProperties.IndexingPolicy,
            DefaultTimeToLive = existingProperties.DefaultTimeToLive,
            ConflictResolutionPolicy = existingProperties.ConflictResolutionPolicy,
            AnalyticalStoreTimeToLiveInSeconds = existingProperties.AnalyticalStoreTimeToLiveInSeconds
        };
        var tempContainerResponse = await database.CreateContainerIfNotExistsAsync(tempContainerProperties);
        var tempContainer = tempContainerResponse.Container;

        logger.LogInformation("Step 2/6: Copying documents to temp container...");
        await bulkHelper.CopyAllDocumentsAsync(container, tempContainer, PartitionKeyProperty);

        logger.LogInformation("Step 3/6: Deleting current container with UniqueKeyPolicy...");
        await container.DeleteContainerAsync();

        logger.LogInformation("Step 4/6: Recreating container with original UniqueKeyPolicy (without /orderNumber)...");
        var originalProperties = new ContainerProperties(ContainerName, PartitionKeyPath)
        {
            UniqueKeyPolicy = existingProperties.UniqueKeyPolicy,
            IndexingPolicy = existingProperties.IndexingPolicy,
            DefaultTimeToLive = existingProperties.DefaultTimeToLive,
            ConflictResolutionPolicy = existingProperties.ConflictResolutionPolicy,
            AnalyticalStoreTimeToLiveInSeconds = existingProperties.AnalyticalStoreTimeToLiveInSeconds
        };
        var newContainerResponse = await database.CreateContainerAsync(originalProperties);
        var newContainer = newContainerResponse.Container;

        logger.LogInformation("Step 5/6: Copying documents back...");
        await bulkHelper.CopyAllDocumentsAsync(tempContainer, newContainer, PartitionKeyProperty);

        logger.LogInformation("Step 6/6: Deleting temporary container...");
        await tempContainer.DeleteContainerAsync();
        logger.LogInformation("Rollback complete");
    }
}
