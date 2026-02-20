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

        var documents = await helper.ReadDocumentsAsync(container, "SELECT * FROM c WHERE NOT IS_DEFINED(c.age)");

        if (documents.Count == 0)
        {
            logger.LogInformation("No documents needed updating");
            return;
        }

        foreach (var doc in documents)
        {
            doc["age"] = JsonValue.Create(DefaultValue);
        }

        logger.LogInformation("{Count} document(s) to update", documents.Count);
        await helper.BulkUpsertAsync(container, documents);
    }

    /// <inheritdoc />
    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<_20240101_000001_AddAgePropertyToUsers>();
        var helper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>());

        logger.LogInformation("Removing 'age' property from all Users...");

        var documents = await helper.ReadDocumentsAsync(container, "SELECT * FROM c WHERE IS_DEFINED(c.age)");

        if (documents.Count == 0)
        {
            logger.LogInformation("No documents needed updating");
            return;
        }

        foreach (var doc in documents)
        {
            doc.Remove("age");
        }

        await helper.BulkUpsertAsync(container, documents);
    }
}
