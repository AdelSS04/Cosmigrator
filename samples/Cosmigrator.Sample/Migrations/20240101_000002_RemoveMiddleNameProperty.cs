using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmigrator.Sample.Migrations;

/// <summary>
/// Scenario B — Remove a property.
/// Iterates all documents in the "Users" container, removes the "middleName"
/// property using JObject manipulation, and upserts the document.
/// </summary>
public class _20240101_000002_RemoveMiddleNameProperty : IMigration
{
    /// <inheritdoc />
    public string Id => "20240101_000002";

    /// <inheritdoc />
    public string Name => "RemoveMiddleNameProperty";

    /// <inheritdoc />
    public string ContainerName => "Users";

    /// <inheritdoc />
    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000002_RemoveMiddleNameProperty>();
        var helper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>());

        logger.LogInformation("Removing 'middleName' property from all Users...");

        var documents = await helper.ReadAllDocumentsAsync(container);
        var modified = new List<JsonObject>();

        foreach (var doc in documents)
        {
            if (doc["middleName"] != null)
            {
                doc.Remove("middleName");
                modified.Add(doc);
            }
        }

        if (modified.Count == 0)
        {
            logger.LogInformation("No documents had 'middleName' property");
            return;
        }

        logger.LogInformation("{Count} document(s) to update", modified.Count);
        await helper.BulkUpsertAsync(container, modified);
    }

    /// <inheritdoc />
    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000002_RemoveMiddleNameProperty>();
        var helper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>());

        logger.LogInformation("Re-adding 'middleName' property with empty default...");

        var documents = await helper.ReadAllDocumentsAsync(container);
        var modified = new List<JsonObject>();

        foreach (var doc in documents)
        {
            if (doc["middleName"] == null)
            {
                doc["middleName"] = string.Empty;
                modified.Add(doc);
            }
        }

        if (modified.Count == 0)
        {
            logger.LogInformation("No documents needed updating");
            return;
        }

        await helper.BulkUpsertAsync(container, modified);
    }
}
