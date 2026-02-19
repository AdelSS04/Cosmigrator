using System.Collections.ObjectModel;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmigrator.Sample.Migrations;

/// <summary>
/// Scenario E — Add a Composite Index.
/// Updates the container's IndexingPolicy to add a new composite index
/// on "/lastName" (ascending) and "/firstName" (ascending).
/// </summary>
public class _20240101_000005_AddCompositeIndexToUsers : IMigration
{
    /// <inheritdoc />
    public string Id => "20240101_000005";

    /// <inheritdoc />
    public string Name => "AddCompositeIndexToUsers";

    /// <inheritdoc />
    public string ContainerName => "Users";

    /// <inheritdoc />
    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000005_AddCompositeIndexToUsers>();

        logger.LogInformation("Adding composite index [/lastName ASC, /firstName ASC] to 'Users'...");

        var response = await container.ReadContainerAsync();
        var properties = response.Resource;

        var compositeIndex = new Collection<CompositePath>
        {
            new CompositePath { Path = "/lastName", Order = CompositePathSortOrder.Ascending },
            new CompositePath { Path = "/firstName", Order = CompositePathSortOrder.Ascending }
        };

        properties.IndexingPolicy.CompositeIndexes.Add(compositeIndex);
        await container.ReplaceContainerAsync(properties);

        logger.LogInformation("Composite index added successfully");
    }

    /// <inheritdoc />
    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000005_AddCompositeIndexToUsers>();

        logger.LogInformation("Removing composite index [/lastName ASC, /firstName ASC] from 'Users'...");

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
            logger.LogInformation("Composite index removed successfully");
        }
        else
        {
            logger.LogWarning("Composite index not found. Nothing to remove");
        }
    }
}
