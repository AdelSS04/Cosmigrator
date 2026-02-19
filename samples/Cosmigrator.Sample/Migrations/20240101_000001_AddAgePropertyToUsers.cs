using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmigrator.Sample.Migrations;

/// <summary>
/// Scenario A — Add a new property with a default value.
/// Iterates all documents in the "Users" container, adds an "age" property
/// with a default value (defined in DefaultValue property from IMigration interface) if it doesn't exist.
/// The default value can be customized to any type: int, string, null, Guid.Empty, etc.
/// </summary>
public class _20240101_000001_AddAgePropertyToUsers : IMigration
{
    /// <inheritdoc />
    public string Id => "20240101_000001";

    /// <inheritdoc />
    public string Name => "AddAgePropertyToUsers";

    /// <inheritdoc />
    public string ContainerName => "Users";

    /// <inheritdoc />
    public object? DefaultValue => null;

    /// <inheritdoc />
    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000001_AddAgePropertyToUsers>();
        var helper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>());

        logger.LogInformation("Adding 'age' property with default value {DefaultValue} to all Users...", DefaultValue);

        var documents = await helper.ReadAllDocumentsAsync(container);
        var modified = new List<JsonObject>();

        foreach (var doc in documents)
        {
            if (doc["age"] == null)
            {
                doc["age"] = JsonValue.Create(DefaultValue);
                modified.Add(doc);
            }
        }

        if (modified.Count == 0)
        {
            logger.LogInformation("No documents needed updating");
            return;
        }

        logger.LogInformation("{Count} document(s) to update", modified.Count);
        await helper.BulkUpsertAsync(container, modified);
    }

    /// <inheritdoc />
    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000001_AddAgePropertyToUsers>();
        var helper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>());

        logger.LogInformation("Removing 'age' property from all Users...");

        var documents = await helper.ReadAllDocumentsAsync(container);
        var modified = new List<JsonObject>();

        foreach (var doc in documents)
        {
            if (doc["age"] != null)
            {
                doc.Remove("age");
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
