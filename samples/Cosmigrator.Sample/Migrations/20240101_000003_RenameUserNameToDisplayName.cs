using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmigrator.Sample.Migrations;

/// <summary>
/// Scenario C — Rename a property.
/// Iterates all documents in the "Users" container, copies the value from the
/// old "userName" property to the new "displayName" property, removes the old key,
/// and upserts the document.
/// </summary>
public class _20240101_000003_RenameUserNameToDisplayName : IMigration
{
    /// <inheritdoc />
    public string Id => "20240101_000003";

    /// <inheritdoc />
    public string Name => "RenameUserNameToDisplayName";

    /// <inheritdoc />
    public string ContainerName => "Users";

    private const string OldPropertyName = "userName";
    private const string NewPropertyName = "displayName";

    /// <inheritdoc />
    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000003_RenameUserNameToDisplayName>();
        var helper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>());

        logger.LogInformation("Renaming '{OldProperty}' -> '{NewProperty}' on all Users...",
            OldPropertyName, NewPropertyName);

        var documents = await helper.ReadAllDocumentsAsync(container);
        var modified = new List<JsonObject>();

        foreach (var doc in documents)
        {
            if (doc[OldPropertyName] != null)
            {
                doc[NewPropertyName] = doc[OldPropertyName]!.DeepClone();
                doc.Remove(OldPropertyName);
                modified.Add(doc);
            }
        }

        if (modified.Count == 0)
        {
            logger.LogInformation("No documents had '{OldProperty}' property", OldPropertyName);
            return;
        }

        logger.LogInformation("{Count} document(s) to update", modified.Count);
        await helper.BulkUpsertAsync(container, modified);
    }

    /// <inheritdoc />
    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000003_RenameUserNameToDisplayName>();
        var helper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>());

        logger.LogInformation("Reverting rename: '{NewProperty}' -> '{OldProperty}'...",
            NewPropertyName, OldPropertyName);

        var documents = await helper.ReadAllDocumentsAsync(container);
        var modified = new List<JsonObject>();

        foreach (var doc in documents)
        {
            if (doc[NewPropertyName] != null)
            {
                doc[OldPropertyName] = doc[NewPropertyName]!.DeepClone();
                doc.Remove(NewPropertyName);
                modified.Add(doc);
            }
        }

        if (modified.Count == 0)
        {
            logger.LogInformation("No documents had '{NewProperty}' property", NewPropertyName);
            return;
        }

        await helper.BulkUpsertAsync(container, modified);
    }
}
