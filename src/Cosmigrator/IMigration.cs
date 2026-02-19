using Microsoft.Azure.Cosmos;

namespace Cosmigrator;

/// <summary>
/// Defines the contract for a Cosmos DB migration.
/// Each migration class must implement this interface to be discovered and executed
/// by the <see cref="MigrationRunner"/>.
/// </summary>
public interface IMigration
{
    /// <summary>
    /// Unique identifier for the migration, used for ordering and history tracking.
    /// Convention: "YYYYMMDD_NNNNNN" (e.g. "20240101_000001").
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Human-readable name describing what the migration does.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The name of the target Cosmos DB container this migration operates on.
    /// </summary>
    string ContainerName { get; }

    /// <summary>
    /// Optional default value for property additions.
    /// Used when a migration adds a new property with a default value to existing documents.
    /// Can be any type: int, string, null, Guid, etc.
    /// </summary>
    object? DefaultValue => null;

    /// <summary>
    /// Applies the migration forward.
    /// </summary>
    /// <param name="container">The target Cosmos DB container.</param>
    /// <param name="client">The Cosmos DB client for advanced operations.</param>
    Task UpAsync(Container container, CosmosClient client);

    /// <summary>
    /// Rolls back the migration.
    /// </summary>
    /// <param name="container">The target Cosmos DB container.</param>
    /// <param name="client">The Cosmos DB client for advanced operations.</param>
    Task DownAsync(Container container, CosmosClient client);
}
